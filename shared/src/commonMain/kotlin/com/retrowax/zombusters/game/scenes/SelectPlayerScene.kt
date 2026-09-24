package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.model.GameSession
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
 * Character names from legacy SelectPlayerScreen.cs avatarNameList:
 *   0 → Tracy   (sprites available: jade/ folder)
 *   1 → Charles (sprites not yet extracted — shows NOT AVAILABLE)
 *   2 → Ryan    (sprites not yet extracted — shows NOT AVAILABLE)
 *   3 → Peter   (sprites not yet extracted — shows NOT AVAILABLE)
 *
 * Controls:
 *   ← / → (or A/D): cycle character selection
 *   ↑ / ↓ (or W/S): change starting level (1 to levelsUnlocked)
 *   Enter / Space: confirm and start game
 *   Escape: back to MenuScene
 *
 * Single-player only in Step 6 — only Player 1 slot active.
 */
class SelectPlayerScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = "",
    private val levelsUnlocked: Int = 1
) : Scene() {

    private var charIndex = 0
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
            image(titleBmp) { x = 115.0; y = 20.0; width = 1000.0; height = 150.0; zIndex = 1.0; smoothing = true }
        }

        // Character select heading
        text("SELECT CHARACTER") {
            textSize = 36.0; color = Colors.WHITE
            x = 128.0; y = 200.0; zIndex = 2.0
            font = ZombustersFonts.menuHeader
        }

        // Character name display
        val charNameView = text(GameSession.CHARACTER_NAMES[charIndex].uppercase()) {
            textSize = 32.0; color = Colors.YELLOW
            x = 128.0; y = 270.0; zIndex = 2.0
            font = ZombustersFonts.menuList
        }

        // Navigation arrows hint
        text("< / >  to change character") {
            textSize = 18.0; color = Colors.WHITE
            x = 128.0; y = 320.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }

        // Availability note
        val availabilityView = text("") {
            textSize = 18.0; color = Colors.ORANGE
            x = 128.0; y = 350.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }

        // Level selection (right side of screen, legacy DrawLevelSelectionMenu)
        text("START LEVEL") {
            textSize = 24.0; color = Colors.WHITE
            x = GAME_WIDTH - 280.0; y = 200.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }
        val levelView = text("$levelSelected") {
            textSize = 72.0; color = Colors.SALMON
            x = GAME_WIDTH - 200.0; y = 230.0; zIndex = 2.0
            font = ZombustersFonts.digitBig
        }
        text("↑ / ↓ to change level") {
            textSize = 18.0; color = Colors.WHITE
            x = GAME_WIDTH - 280.0; y = 320.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }

        // Bottom controls hint
        text("ENTER — Confirm   ESC — Back") {
            textSize = 20.0; color = Colors.WHITE
            x = 128.0; y = GAME_HEIGHT - 60.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }

        // Thin separator line approximation
        solidRect(GAME_WIDTH.toDouble(), 2.0, Colors.WHITE).apply {
            x = 0.0; y = GAME_HEIGHT - 80.0; zIndex = 1.9; alpha = 0.4
        }

        fun refreshView() {
            charNameView.text = GameSession.CHARACTER_NAMES[charIndex].uppercase()
            charNameView.color = if (GameSession.AVAILABLE_CHARACTER_INDICES.contains(charIndex)) Colors.YELLOW else Colors.DIMGRAY
            availabilityView.text = if (GameSession.AVAILABLE_CHARACTER_INDICES.contains(charIndex)) "" else "NOT AVAILABLE (sprites not yet extracted)"
            levelView.text = "$levelSelected"
        }
        refreshView()

        var done = false
        val capturedViews = views
        val sceneScope = this@SelectPlayerScene

        // Touch regions:
        //   Left half (x < 640), y 180-400  → cycle character
        //   Right half, y 180-290            → level up
        //   Right half, y 290-400            → level down
        //   Bottom left (y >= 620, x < 640)  → confirm
        //   Bottom right (y >= 620, x >= 640) → back
        touch {
            end { info ->
                if (done) return@end
                val tx = info.local.x
                val ty = info.local.y
                when {
                    tx < GAME_WIDTH / 2.0 && ty in 180.0..400.0 -> {
                        charIndex = (charIndex + 1) % GameSession.CHARACTER_NAMES.size
                        refreshView()
                    }
                    tx >= GAME_WIDTH / 2.0 && ty in 180.0..290.0 -> {
                        if (levelSelected < levelsUnlocked) levelSelected++ else levelSelected = 1
                        refreshView()
                    }
                    tx >= GAME_WIDTH / 2.0 && ty in 290.0..400.0 -> {
                        if (levelSelected > 1) levelSelected-- else levelSelected = levelsUnlocked
                        refreshView()
                    }
                    ty >= GAME_HEIGHT - 100.0 && tx < GAME_WIDTH / 2.0 -> {
                        done = true
                        val effectiveCharIndex = if (GameSession.AVAILABLE_CHARACTER_INDICES.contains(charIndex)) charIndex else 0
                        val sess = GameSession.newGame(effectiveCharIndex).copy(
                            currentLevel = levelSelected, levelsUnlocked = levelsUnlocked
                        )
                        sceneScope.launch {
                            sceneContainer.changeTo { GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session = sess) }
                            done = false
                        }
                    }
                    ty >= GAME_HEIGHT - 100.0 && tx >= GAME_WIDTH / 2.0 -> {
                        done = true
                        sceneScope.launch {
                            sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                            done = false
                        }
                    }
                }
            }
        }

        var prevMouseDown = false

        addUpdater { _: Duration ->
            val ks = capturedViews.input.keys

            // Mouse click: same hit regions as touch
            val mouseDown = capturedViews.input.mouseButtonPressed(MouseButton.LEFT)
            if (!mouseDown && prevMouseDown && !done) {
                val mp = capturedViews.input.mousePos
                val tx = mp.x; val ty = mp.y
                when {
                    tx < GAME_WIDTH / 2.0 && ty in 180.0..400.0 -> {
                        charIndex = (charIndex + 1) % GameSession.CHARACTER_NAMES.size
                        refreshView()
                    }
                    tx >= GAME_WIDTH / 2.0 && ty in 180.0..290.0 -> {
                        if (levelSelected < levelsUnlocked) levelSelected++ else levelSelected = 1
                        refreshView()
                    }
                    tx >= GAME_WIDTH / 2.0 && ty in 290.0..400.0 -> {
                        if (levelSelected > 1) levelSelected-- else levelSelected = levelsUnlocked
                        refreshView()
                    }
                    ty >= GAME_HEIGHT - 100.0 && tx < GAME_WIDTH / 2.0 -> {
                        done = true
                        val effectiveCharIndex = if (GameSession.AVAILABLE_CHARACTER_INDICES.contains(charIndex)) charIndex else 0
                        val sess = GameSession.newGame(effectiveCharIndex).copy(
                            currentLevel = levelSelected, levelsUnlocked = levelsUnlocked
                        )
                        sceneScope.launch {
                            sceneContainer.changeTo { GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session = sess) }
                            done = false
                        }
                    }
                    ty >= GAME_HEIGHT - 100.0 && tx >= GAME_WIDTH / 2.0 -> {
                        done = true
                        sceneScope.launch {
                            sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                            done = false
                        }
                    }
                }
            }
            prevMouseDown = mouseDown

            if (ks.justPressed(Key.LEFT) || ks.justPressed(Key.A)) {
                charIndex = (charIndex - 1 + GameSession.CHARACTER_NAMES.size) % GameSession.CHARACTER_NAMES.size
                refreshView()
            }
            if (ks.justPressed(Key.RIGHT) || ks.justPressed(Key.D)) {
                charIndex = (charIndex + 1) % GameSession.CHARACTER_NAMES.size
                refreshView()
            }
            if (ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                if (levelSelected < levelsUnlocked) levelSelected++
                else levelSelected = 1
                refreshView()
            }
            if (ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                if (levelSelected > 1) levelSelected--
                else levelSelected = levelsUnlocked
                refreshView()
            }
            if (ks.justPressed(Key.ESCAPE)) {
                sceneScope.launch {
                    sceneContainer.changeTo {
                        MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                    }
                }
            }
            if (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                val effectiveCharIndex = if (GameSession.AVAILABLE_CHARACTER_INDICES.contains(charIndex)) charIndex else 0
                val session = GameSession.newGame(effectiveCharIndex).copy(
                    currentLevel = levelSelected,
                    levelsUnlocked = levelsUnlocked
                )
                sceneScope.launch {
                    sceneContainer.changeTo {
                        GameplayScene(
                            exit = exit,
                            drawableResourcesPath = drawableResourcesPath,
                            fontResourcesPath = fontResourcesPath,
                            filesResourcesPath = filesResourcesPath,
                            session = session
                        )
                    }
                }
            }
        }
    }

    private suspend fun tryLoad(path: String) = try {
        resourcesVfs[path].readBitmap()
    } catch (_: Exception) { null }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
