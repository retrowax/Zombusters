package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.model.GameSession
import com.retrowax.zombusters.game.persistence.GameSettings
import com.retrowax.zombusters.game.ui.ZombustersFonts
import korlibs.event.Key
import korlibs.event.MouseButton
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.image.format.readBitmap
import korlibs.korge.input.touch
import korlibs.korge.scene.Scene
import korlibs.korge.view.SContainer
import korlibs.korge.view.addUpdater
import korlibs.korge.view.image
import korlibs.korge.view.solidRect
import korlibs.korge.view.text
import korlibs.io.file.std.resourcesVfs
import korlibs.math.geom.Size
import kotlinx.coroutines.launch
import kotlin.time.Duration

/**
 * Character and level selection screen — legacy SelectPlayerScreen.cs.
 *
 * Multiplayer joining rules (Step 8 — legacy SelectPlayerScreen.cs port):
 *   P1: auto-joined, keyboard/touch
 *   P2-P4: join via gamepad START button; leave via B/East when not ready
 *   Ready: SPACE (P1) or South/A button (gamepad); must be ready for CanStartGame
 *   Level: shared, P1 (keyboard: W/S) or any joined player (gamepad D-pad U/D) changes it
 *   CanStartGame: all joined players are ready (AND at least P1 is joined)
 *
 * Mobile: only P1 slot shown; no gamepad join prompts.
 *
 * Character names (legacy avatarNameList):
 *   0 → Tracy   (sprites available)
 *   1 → Charles (sprites not yet extracted — shows NOT AVAILABLE)
 *   2 → Ryan    (sprites not yet extracted)
 *   3 → Peter   (sprites not yet extracted)
 */
class SelectPlayerScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = "",
    private val levelsUnlocked: Int = GameSettings.levelsUnlocked
) : Scene() {

    // Scene-local join state per player
    private data class PlayerSlot(
        val index: Int,
        var isJoined: Boolean,
        var characterIndex: Int = 0,
        var isReady: Boolean = false
    )

    private val slots = listOf(
        PlayerSlot(0, isJoined = true),
        PlayerSlot(1, isJoined = false),
        PlayerSlot(2, isJoined = false),
        PlayerSlot(3, isJoined = false)
    )

    private var levelSelected = 1

    override suspend fun SContainer.sceneInit() {
        ZombustersFonts.loadFrom(fontResourcesPath)

        val assetBase = "$filesResourcesPath/zombusters"
        val menuBase  = "$assetBase/menu"

        val bgBitmap = tryLoad("$menuBase/background_title.png")
        if (bgBitmap != null) {
            image(bgBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x11, 0x11, 0x22, 0xFF))
        }

        val titleBmp = tryLoad("$menuBase/title.png")
        if (titleBmp != null) {
            image(titleBmp) { x = 115.0; y = 10.0; width = 1000.0; height = 130.0; zIndex = 1.0; smoothing = true }
        } else {
            text("SELECT PLAYER") {
                textSize = 36.0; color = Colors.WHITE
                x = 128.0; y = 150.0; zIndex = 2.0
                font = ZombustersFonts.menuHeader
            }
        }

        // Separator line
        solidRect(GAME_WIDTH.toDouble(), 2.0, Colors.WHITE).apply {
            x = 0.0; y = 150.0; zIndex = 1.9; alpha = 0.4
        }

        // ── 4 player slot columns ───────────────────────────────────────────
        // Desktop: 4 columns at x = 20, 315, 610, 905 (width ~285 each)
        // Mobile: only P1 column shown
        val colX = listOf(20.0, 315.0, 610.0, 905.0)
        val slotY = 165.0

        // Per-slot text view references for dynamic updates
        val headerViews = mutableListOf<korlibs.korge.view.Text>()
        val charViews   = mutableListOf<korlibs.korge.view.Text>()
        val readyViews  = mutableListOf<korlibs.korge.view.Text>()
        val availViews  = mutableListOf<korlibs.korge.view.Text>()

        for (i in 0..3) {
            val x = colX[i]
            val label = "P${i + 1}"
            val pColor = listOf(Colors.CYAN, Colors.LIME, Colors.YELLOW, Colors.ORANGE)[i]

            headerViews += text(label) {
                textSize = 28.0; color = pColor
                this.x = x; this.y = slotY; zIndex = 2.0
                font = ZombustersFonts.menuList
            }
            charViews += text("") {
                textSize = 22.0; color = Colors.WHITE
                this.x = x; this.y = slotY + 42.0; zIndex = 2.0
                font = ZombustersFonts.menuList
            }
            readyViews += text("") {
                textSize = 18.0; color = Colors.LIME
                this.x = x; this.y = slotY + 76.0; zIndex = 2.0
                font = ZombustersFonts.menuInfo
            }
            availViews += text("") {
                textSize = 14.0; color = Colors.ORANGE
                this.x = x; this.y = slotY + 100.0; zIndex = 2.0
                font = ZombustersFonts.menuInfo
            }
        }

        // ── Level selection (right-side, above score area) ──────────────────
        text("START LEVEL") {
            textSize = 20.0; color = Colors.WHITE
            x = GAME_WIDTH - 220.0; y = 160.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }
        val levelView = text("$levelSelected") {
            textSize = 64.0; color = Colors.SALMON
            x = GAME_WIDTH - 170.0; y = 185.0; zIndex = 2.0
            font = ZombustersFonts.digitBig
        }
        text("W/S or D-pad") {
            textSize = 14.0; color = Colors.WHITE
            x = GAME_WIDTH - 220.0; y = 270.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }

        // ── Bottom hints ────────────────────────────────────────────────────
        val hintView = text("ENTER/SPACE — Start  |  ESC — Back") {
            textSize = 18.0; color = Colors.WHITE
            x = 40.0; y = GAME_HEIGHT - 60.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }

        solidRect(GAME_WIDTH.toDouble(), 2.0, Colors.WHITE).apply {
            x = 0.0; y = GAME_HEIGHT - 76.0; zIndex = 1.9; alpha = 0.4
        }

        val capturedViews = views
        val sceneScope = this@SelectPlayerScene

        // Gamepad just-pressed state (3 gamepads = P2-P4)
        val prevGpStart  = BooleanArray(3)
        val prevGpSouth  = BooleanArray(3)   // A / Cross — ready toggle
        val prevGpEast   = BooleanArray(3)   // B / Circle — unjoin
        val prevGpLeft   = BooleanArray(3)   // d-pad left — char prev
        val prevGpRight  = BooleanArray(3)   // d-pad right — char next
        val prevGpUp     = BooleanArray(3)   // d-pad up — level up
        val prevGpDown   = BooleanArray(3)   // d-pad down — level down

        // Mobile detection at scene start (captures once; isTouchDevice doesn't change mid-session)
        var isMobile = capturedViews.input.isTouchDevice

        fun canStartGame(): Boolean = slots[0].isJoined && slots.filter { it.isJoined }.all { it.isReady }

        fun refreshSlot(i: Int) {
            val slot = slots[i]
            val available = GameSession.AVAILABLE_CHARACTER_INDICES.contains(slot.characterIndex)
            if (!slot.isJoined) {
                charViews[i].text   = if (!isMobile && i > 0) "PRESS START" else ""
                charViews[i].color  = Colors.DIMGRAY
                readyViews[i].text  = ""
                availViews[i].text  = ""
            } else {
                charViews[i].text   = GameSession.CHARACTER_NAMES[slot.characterIndex].uppercase()
                charViews[i].color  = if (available) Colors.YELLOW else Colors.DIMGRAY
                readyViews[i].text  = if (slot.isReady) "READY ✓" else "not ready"
                readyViews[i].color = if (slot.isReady) Colors.LIME else Colors.LIGHTGRAY
                availViews[i].text  = if (!available) "(no sprites)" else ""
            }
        }

        fun refreshAll() {
            for (i in 0..3) refreshSlot(i)
            levelView.text = "$levelSelected"
            hintView.text = when {
                canStartGame() && slots.filter { it.isJoined }.size > 1 -> "ENTER/SPACE or START — Begin"
                canStartGame() -> "ENTER / SPACE — Start Game  |  ESC — Back"
                else -> "SPACE — Ready  |  ESC — Back"
            }
        }
        refreshAll()

        var done = false

        fun startGame() {
            if (done || !canStartGame()) return
            done = true
            val joinedCharIndices = slots.filter { it.isJoined }.map { it.characterIndex }
            val sess = GameSession.newMultiGame(joinedCharIndices, levelSelected, levelsUnlocked)
            sceneScope.launch {
                sceneContainer.changeTo {
                    GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session = sess)
                }
                done = false
            }
        }

        fun goBack() {
            if (done) return
            done = true
            sceneScope.launch {
                sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                done = false
            }
        }

        // ── Touch / mouse for mobile P1 ─────────────────────────────────────
        var prevMouseDown = false

        touch {
            end { info ->
                if (done) return@end
                val tx = info.local.x; val ty = info.local.y
                val slot = slots[0]
                when {
                    // Left half: cycle character
                    tx < GAME_WIDTH / 2.0 && ty in 170.0..420.0 -> {
                        slot.characterIndex = (slot.characterIndex + 1) % GameSession.CHARACTER_NAMES.size
                        refreshAll()
                    }
                    // Right half upper: level up
                    tx >= GAME_WIDTH / 2.0 && ty in 170.0..320.0 -> {
                        if (levelSelected < levelsUnlocked) levelSelected++ else levelSelected = 1
                        refreshAll()
                    }
                    // Right half lower: level down
                    tx >= GAME_WIDTH / 2.0 && ty in 320.0..420.0 -> {
                        if (levelSelected > 1) levelSelected-- else levelSelected = levelsUnlocked
                        refreshAll()
                    }
                    // Bottom left: confirm/start
                    ty >= GAME_HEIGHT - 100.0 && tx < GAME_WIDTH / 2.0 -> {
                        if (!slot.isReady) { slot.isReady = true; refreshAll() }
                        else startGame()
                    }
                    // Bottom right: back
                    ty >= GAME_HEIGHT - 100.0 && tx >= GAME_WIDTH / 2.0 -> goBack()
                }
            }
        }

        addUpdater { _: Duration ->
            val ks = capturedViews.input.keys
            isMobile = capturedViews.input.isTouchDevice

            // Mouse click (desktop)
            val mouseDown = capturedViews.input.mouseButtonPressed(MouseButton.LEFT)
            if (!mouseDown && prevMouseDown && !done && !isMobile) {
                val mp = capturedViews.input.mousePos
                val tx = mp.x; val ty = mp.y
                val slot = slots[0]
                when {
                    tx < GAME_WIDTH / 2.0 && ty in 170.0..420.0 -> {
                        slot.characterIndex = (slot.characterIndex + 1) % GameSession.CHARACTER_NAMES.size
                        refreshAll()
                    }
                    tx >= GAME_WIDTH / 2.0 && ty in 170.0..320.0 -> {
                        if (levelSelected < levelsUnlocked) levelSelected++ else levelSelected = 1
                        refreshAll()
                    }
                    tx >= GAME_WIDTH / 2.0 && ty in 320.0..420.0 -> {
                        if (levelSelected > 1) levelSelected-- else levelSelected = levelsUnlocked
                        refreshAll()
                    }
                    ty >= GAME_HEIGHT - 100.0 && tx < GAME_WIDTH / 2.0 -> {
                        if (!slots[0].isReady) { slots[0].isReady = true; refreshAll() }
                        else startGame()
                    }
                    ty >= GAME_HEIGHT - 100.0 && tx >= GAME_WIDTH / 2.0 -> goBack()
                }
            }
            prevMouseDown = mouseDown

            // ── Keyboard input for P1 ──────────────────────────────────────
            if (ks.justPressed(Key.ESCAPE)) { goBack(); return@addUpdater }

            // P1 character selection
            if (ks.justPressed(Key.LEFT) || ks.justPressed(Key.A)) {
                slots[0].characterIndex = (slots[0].characterIndex - 1 + GameSession.CHARACTER_NAMES.size) % GameSession.CHARACTER_NAMES.size
                refreshAll()
            }
            if (ks.justPressed(Key.RIGHT) || ks.justPressed(Key.D)) {
                slots[0].characterIndex = (slots[0].characterIndex + 1) % GameSession.CHARACTER_NAMES.size
                refreshAll()
            }

            // Level selection (P1 keyboard)
            if (ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                if (levelSelected < levelsUnlocked) levelSelected++ else levelSelected = 1
                refreshAll()
            }
            if (ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                if (levelSelected > 1) levelSelected-- else levelSelected = levelsUnlocked
                refreshAll()
            }

            // Ready toggle (SPACE) or start (ENTER when ready)
            if (ks.justPressed(Key.SPACE)) {
                slots[0].isReady = !slots[0].isReady
                refreshAll()
            }
            if (ks.justPressed(Key.RETURN)) {
                if (!slots[0].isReady) { slots[0].isReady = true; refreshAll() }
                else startGame()
            }

            // ── Gamepad input for P2-P4 (not on mobile) ────────────────────
            if (!isMobile) {
                val gp = capturedViews.input.gamepads
                for (gpIdx in 0..2) {
                    val pad = gp[gpIdx]
                    val slotIdx = gpIdx + 1  // gpIdx 0 → slot 1 (P2), etc.
                    val slot = slots[slotIdx]

                    val startNow  = pad.connected && pad.start
                    val southNow  = pad.connected && pad.south
                    val eastNow   = pad.connected && pad.east
                    val leftNow   = pad.connected && pad.left
                    val rightNow  = pad.connected && pad.right
                    val upNow     = pad.connected && pad.up
                    val downNow   = pad.connected && pad.down

                    val startJP  = startNow  && !prevGpStart[gpIdx]
                    val southJP  = southNow  && !prevGpSouth[gpIdx]
                    val eastJP   = eastNow   && !prevGpEast[gpIdx]
                    val leftJP   = leftNow   && !prevGpLeft[gpIdx]
                    val rightJP  = rightNow  && !prevGpRight[gpIdx]
                    val upJP     = upNow     && !prevGpUp[gpIdx]
                    val downJP   = downNow   && !prevGpDown[gpIdx]

                    if (!slot.isJoined) {
                        // JOIN: press START on gamepad
                        if (startJP) {
                            slot.isJoined = true; slot.isReady = false
                            refreshSlot(slotIdx)
                        }
                    } else {
                        // Already joined
                        // Character selection
                        if (leftJP) {
                            slot.characterIndex = (slot.characterIndex - 1 + GameSession.CHARACTER_NAMES.size) % GameSession.CHARACTER_NAMES.size
                            refreshSlot(slotIdx)
                        }
                        if (rightJP) {
                            slot.characterIndex = (slot.characterIndex + 1) % GameSession.CHARACTER_NAMES.size
                            refreshSlot(slotIdx)
                        }
                        // Level selection (any joined player can change)
                        if (upJP) {
                            if (levelSelected < levelsUnlocked) levelSelected++ else levelSelected = 1
                            levelView.text = "$levelSelected"
                        }
                        if (downJP) {
                            if (levelSelected > 1) levelSelected-- else levelSelected = levelsUnlocked
                            levelView.text = "$levelSelected"
                        }
                        // Ready toggle: South (A/Cross)
                        if (southJP) {
                            slot.isReady = !slot.isReady
                            refreshSlot(slotIdx)
                            refreshAll()
                        }
                        // Un-join: East (B/Circle) while not ready; if ready, un-ready first
                        if (eastJP) {
                            if (slot.isReady) { slot.isReady = false; refreshSlot(slotIdx) }
                            else { slot.isJoined = false; refreshSlot(slotIdx) }
                        }
                        // START while ready = start game
                        if (startJP) {
                            if (!slot.isReady) { slot.isReady = true; refreshSlot(slotIdx); refreshAll() }
                            else if (canStartGame()) startGame()
                        }
                    }

                    // Save prev state
                    prevGpStart[gpIdx] = startNow
                    prevGpSouth[gpIdx] = southNow
                    prevGpEast[gpIdx]  = eastNow
                    prevGpLeft[gpIdx]  = leftNow
                    prevGpRight[gpIdx] = rightNow
                    prevGpUp[gpIdx]    = upNow
                    prevGpDown[gpIdx]  = downNow
                }
            }
        }
    }

    private suspend fun tryLoad(path: String) = try {
        resourcesVfs[path].readBitmap()
    } catch (_: Exception) { null }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
