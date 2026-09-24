package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.combat.FacingDirection
import com.retrowax.zombusters.game.debug.GameDebugConfig
import com.retrowax.zombusters.game.enemy.BaseEnemy
import com.retrowax.zombusters.game.enemy.Rat
import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.enemy.Wolf
import com.retrowax.zombusters.game.engine.CollisionSystem
import com.retrowax.zombusters.game.input.DualVirtualStickState
import com.retrowax.zombusters.game.input.GameInput
import com.retrowax.zombusters.game.input.InputMode
import com.retrowax.zombusters.game.input.VirtualThumbstickLogic
import com.retrowax.zombusters.game.model.AVATAR_PIXELS_PER_SECOND
import com.retrowax.zombusters.game.model.EnemyType
import com.retrowax.zombusters.game.model.FurnitureOrientation
import com.retrowax.zombusters.game.model.FurnitureType
import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.model.GameSession
import com.retrowax.zombusters.game.model.GunType
import com.retrowax.zombusters.game.persistence.GameSettings
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.systems.CombatSystem
import com.retrowax.zombusters.game.systems.DamageSystem
import com.retrowax.zombusters.game.ui.ZombustersFonts
import com.retrowax.zombusters.game.world.Furniture
import com.retrowax.zombusters.game.world.GameplayWorld
import com.retrowax.zombusters.game.world.LevelParser
import korlibs.audio.sound.readMusic
import korlibs.event.Key
import korlibs.image.bitmap.Bitmap
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.image.format.readBitmap
import korlibs.korge.input.touch
import korlibs.korge.scene.Scene
import korlibs.korge.view.SContainer
import korlibs.korge.view.SpriteAnimation
import korlibs.korge.view.addUpdater
import korlibs.korge.view.container
import korlibs.korge.view.cpuGraphics
import korlibs.korge.view.image
import korlibs.korge.view.solidRect
import korlibs.korge.view.sprite
import korlibs.korge.view.text
import korlibs.io.file.std.resourcesVfs
import korlibs.math.geom.Point
import korlibs.math.geom.Size
import kotlinx.coroutines.launch
import kotlin.math.sqrt
import kotlin.time.Duration
import kotlin.time.Duration.Companion.seconds

// Player sprite offsets from legacy GamePlayScreen.cs line 1458
private const val PLAYER_RENDER_OFFSET_X = -20.0
private const val PLAYER_RENDER_OFFSET_Y = -55.0

// Jade idle animation from AnimationDef.xml JadeIdleTrunkDef
private const val JADE_IDLE_FRAME_W = 42
private const val JADE_IDLE_FRAME_H = 55
private const val JADE_IDLE_COLS = 11
private const val JADE_IDLE_FPS = 15

// Zombie walk: ZombieDef — 48×55, 8 cols, Speed=10
private const val ZOMBIE_FRAME_W = 48
private const val ZOMBIE_FRAME_H = 55
private const val ZOMBIE_COLS = 8
private const val ZOMBIE_FPS = 10

// Rat: 48×48 per frame
private const val RAT_FRAME_W = 48
private const val RAT_FRAME_H = 48
private const val RAT_IDLE_COLS = 8
private const val RAT_RUN_COLS = 6
private const val RAT_FPS = 12

// Wolf: 80×48 per frame
private const val WOLF_FRAME_W = 80
private const val WOLF_FRAME_H = 48
private const val WOLF_IDLE_COLS = 8
private const val WOLF_RUN_COLS = 6
private const val WOLF_FPS = 12

// Pause icon hit region from legacy GamePlayScreen.cs (1280×720 logical coords)
private const val PAUSE_ICON_X1 = 1088f
private const val PAUSE_ICON_Y1 = 39f
private const val PAUSE_ICON_X2 = 1135f
private const val PAUSE_ICON_Y2 = 83f

/**
 * Main gameplay scene — driven by [GameSession].
 *
 * Step 7: Input refactored.
 *   - All production movement/aim/fire uses [GameInput] — no direct keyboard in gameplay logic.
 *   - Touch: dual virtual thumbsticks via [DualVirtualStickState] (legacy VirtualThumbsticks.cs behavior).
 *   - Keyboard: WASD/arrows + SPACE mapped to [GameInput] each frame.
 *   - Debug keys (F1/F2/K/G/H/digits) remain as direct keyboard checks (developer tooling only).
 *   - Virtual stick visuals rendered as circle placeholders when InputMode.TOUCH.
 *   - Pause icon hit region (legacy coords 1088-1135, 39-83) for touch pause.
 */
class GameplayScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = "",
    private val session: GameSession = GameSession()
) : Scene() {

    private val assetBase get() = "$filesResourcesPath/zombusters"
    private val levelNumber get() = session.currentLevel.coerceIn(1, 10)

    override suspend fun SContainer.sceneInit() {
        ZombustersFonts.loadFrom(fontResourcesPath)

        val levelDef = if (filesResourcesPath.isNotEmpty()) {
            try { LevelParser(filesResourcesPath).loadLevel(levelNumber) } catch (_: Exception) { null }
        } else null

        if (levelDef == null) {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x22, 0x22, 0x22, 0xFF))
            text("Failed to load Level $levelNumber") {
                textSize = 32.0; color = Colors.RED
                x = 400.0; y = 340.0
            }
            return
        }

        val world = GameplayWorld(levelDef, session.numPlayers)
        world.player1.score = session.score
        world.player1.lives = session.lives

        // Load bitmaps
        val levelPad = levelNumber.toString().padStart(2, '0')
        val mapBitmap        = tryLoadBitmap("$assetBase/levels/level$levelPad/map.png")
        val charPath         = session.characterSpritePath
        val playerIdleBmp    = tryLoadBitmap("$assetBase/characters/$charPath/idle.png")
        val furnitureBitmaps = loadFurnitureBitmaps()
        val zombieBitmap     = tryLoadBitmap("$assetBase/enemies/zombie/walk1.png")
        val ratIdleBitmap    = tryLoadBitmap("$assetBase/enemies/rat/idle.png")
        val ratRunBitmap     = tryLoadBitmap("$assetBase/enemies/rat/run.png")
        val wolfIdleBitmap   = tryLoadBitmap("$assetBase/enemies/wolf/idle.png")
        val wolfRunBitmap    = tryLoadBitmap("$assetBase/enemies/wolf/run.png")
        val minotaurBitmap   = tryLoadBitmap("$assetBase/enemies/minotaur/walk.png")

        val zombieAnim   = zombieBitmap?.let { SpriteAnimation(it, ZOMBIE_FRAME_W, ZOMBIE_FRAME_H, ZOMBIE_COLS, 1) }
        val ratIdleAnim  = ratIdleBitmap?.let { SpriteAnimation(it, RAT_FRAME_W, RAT_FRAME_H, RAT_IDLE_COLS, 1) }
        val ratRunAnim   = ratRunBitmap?.let { SpriteAnimation(it, RAT_FRAME_W, RAT_FRAME_H, RAT_RUN_COLS, 1) }
        val wolfIdleAnim = wolfIdleBitmap?.let { SpriteAnimation(it, WOLF_FRAME_W, WOLF_FRAME_H, WOLF_IDLE_COLS, 1) }
        val wolfRunAnim  = wolfRunBitmap?.let { SpriteAnimation(it, WOLF_FRAME_W, WOLF_FRAME_H, WOLF_RUN_COLS, 1) }

        // Map background
        if (mapBitmap != null) {
            image(mapBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x22, 0x33, 0x22, 0xFF))
            text("Level $levelNumber — map not extracted") {
                textSize = 20.0; color = Colors.ORANGE; x = 20.0; y = 20.0; zIndex = 0.1
                font = ZombustersFonts.menuInfo
            }
        }

        buildFurnitureViews(world.furnitures, furnitureBitmaps)

        // Player containers — one per player
        val playerContainers = List(session.numPlayers) { playerIdx ->
            val c = container { zIndex = world.players[playerIdx].position.y }
            if (playerIdleBmp != null) {
                val idleAnim = SpriteAnimation(playerIdleBmp, JADE_IDLE_FRAME_W, JADE_IDLE_FRAME_H, JADE_IDLE_COLS, 1)
                val s = c.sprite(idleAnim) { smoothing = false }
                s.playAnimationLooped(idleAnim, (1.0 / JADE_IDLE_FPS).seconds)
            } else {
                val col = listOf(Colors.CYAN, Colors.LIME, Colors.YELLOW, Colors.ORANGE).getOrElse(playerIdx) { Colors.WHITE }
                c.solidRect(20.0, 20.0, col).also { it.x = -10.0; it.y = -10.0 }
            }
            c
        }
        val playerContainer = playerContainers[0]  // backward-compat alias for single-player HUD/refs

        val bulletGraphics = cpuGraphics { }
        bulletGraphics.zIndex = 900.0

        // Touch virtual stick overlay — redrawn each frame
        val touchOverlayGraphics = cpuGraphics { }
        touchOverlayGraphics.zIndex = 1050.0

        // HUD
        val hudHealth  = text("HP: 100")           { textSize = 14.0; color = Colors.LIME;      x = 8.0;   y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudLives   = text("Lives: ${session.lives}") { textSize = 14.0; color = Colors.WHITE;     x = 90.0;  y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudScore   = text("Score: ${session.score}") { textSize = 14.0; color = Colors.YELLOW;    x = 170.0; y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudWeapon  = text("PISTOL (INF)")       { textSize = 14.0; color = Colors.ORANGE;    x = 270.0; y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudWave    = text("Wave 1")             { textSize = 14.0; color = Colors.LIGHTGRAY; x = 430.0; y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudLevel   = text("LEVEL $levelNumber") { textSize = 14.0; color = Colors.WHITE;     x = 580.0; y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudEnemies = text("Enemies: 0")         { textSize = 14.0; color = Colors.LIGHTGRAY; x = 680.0; y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }

        // Pause overlay (legacy GamePlayMenu entries)
        val pauseOverlay = solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0, 0, 0, 160)).apply { zIndex = 1100.0; visible = false }
        val pauseLabel = text("PAUSED") {
            textSize = 48.0; color = Colors.WHITE
            x = GAME_WIDTH / 2.0 - 80.0; y = GAME_HEIGHT / 2.0 - 80.0; zIndex = 1101.0; visible = false
            font = ZombustersFonts.menuHeader
        }
        val pauseEntries = listOf("RESUME", "HOW TO PLAY", "OPTIONS", "RESTART LEVEL", "QUIT TO MAIN MENU")
        var pauseMenuIndex = 0
        val pauseEntryViews = pauseEntries.mapIndexed { i, label ->
            text(label) {
                textSize = 30.0
                x = GAME_WIDTH / 2.0 - 160.0; y = GAME_HEIGHT / 2.0 - 20.0 + i * 46.0
                zIndex = 1101.0; visible = false
                font = ZombustersFonts.menuList
            }
        }

        // Game-over overlay (legacy GameOverMenu)
        val gameOverBmp = tryLoadBitmap("$assetBase/hud/gameover.png")
        val gameOverOverlay = solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0, 0, 0, 200)).apply { zIndex = 1200.0; visible = false }
        val gameOverImgView = if (gameOverBmp != null) {
            image(gameOverBmp) {
                x = GAME_WIDTH / 2.0 - gameOverBmp.width / 2.0
                y = GAME_HEIGHT * 0.1
                zIndex = 1201.0; visible = false; smoothing = true
            }
        } else null
        val gameOverEntries = listOf("RESTART THIS WAVE", "RESTART FROM BEGINNING", "RETURN TO MAIN MENU")
        var gameOverMenuIndex = 0
        val gameOverEntryViews = gameOverEntries.mapIndexed { i, label ->
            text(label) {
                textSize = 30.0
                x = GAME_WIDTH / 2.0 - 200.0
                y = GAME_HEIGHT * 0.45 + i * 52.0
                zIndex = 1201.0; visible = false
                font = ZombustersFonts.menuList
            }
        }

        // Stage cleared overlay
        val stageClearedLabel = text("STAGE CLEARED!") {
            textSize = 56.0; color = Colors.YELLOW
            x = GAME_WIDTH / 2.0 - 200.0; y = GAME_HEIGHT / 2.0 - 40.0
            zIndex = 1200.0; visible = false
            font = ZombustersFonts.menuHeader
        }

        fun refreshPauseColors() {
            pauseEntryViews.forEachIndexed { i, v -> v.color = if (i == pauseMenuIndex) Colors.YELLOW else Colors.WHITE }
        }
        fun refreshGameOverColors() {
            gameOverEntryViews.forEachIndexed { i, v -> v.color = if (i == gameOverMenuIndex) Colors.YELLOW else Colors.WHITE }
        }

        // Enemy view pool
        val enemyViews = mutableListOf<Pair<BaseEnemy, korlibs.korge.view.Container>>()

        fun spawnEnemyViews(newEnemies: List<BaseEnemy>) {
            for (enemy in newEnemies) {
                val c = container { zIndex = enemy.entity.position.y.toDouble() }
                when (enemy.type) {
                    EnemyType.ZOMBIE -> {
                        val anim = zombieAnim
                        if (anim != null) { val s = c.sprite(anim) { smoothing = false }; s.playAnimationLooped(anim, (1.0 / ZOMBIE_FPS).seconds) }
                        else c.solidRect(20.0, 20.0, Colors.RED).also { it.x = -10.0; it.y = -10.0 }
                    }
                    EnemyType.RAT -> {
                        val anim = ratRunAnim ?: ratIdleAnim
                        if (anim != null) { val s = c.sprite(anim) { smoothing = false }; s.playAnimationLooped(anim, (1.0 / RAT_FPS).seconds) }
                        else c.solidRect(16.0, 16.0, Colors.YELLOW).also { it.x = -8.0; it.y = -8.0 }
                    }
                    EnemyType.WOLF -> {
                        val anim = wolfRunAnim ?: wolfIdleAnim
                        if (anim != null) { val s = c.sprite(anim) { smoothing = false }; s.playAnimationLooped(anim, (1.0 / WOLF_FPS).seconds) }
                        else c.solidRect(24.0, 24.0, Colors.MAGENTA).also { it.x = -12.0; it.y = -12.0 }
                    }
                    EnemyType.MINOTAUR -> {
                        if (minotaurBitmap != null) {
                            val anim = SpriteAnimation(minotaurBitmap, 80, 80, 6, 1)
                            val s = c.sprite(anim) { smoothing = false }; s.playAnimationLooped(anim, (1.0 / 10).seconds)
                        } else c.solidRect(30.0, 30.0, Colors.BROWN).also { it.x = -15.0; it.y = -15.0 }
                    }
                    else -> c.solidRect(20.0, 20.0, Colors.ORANGE).also { it.x = -10.0; it.y = -10.0 }
                }
                enemyViews.add(Pair(enemy, c))
            }
        }

        // Debug overlay (F1)
        var debugMode = false
        val debugOverlay = cpuGraphics {
            for (wall in world.walls) {
                stroke(Colors.WHITE, lineWidth = 2.0) {
                    moveTo(Point(wall.fromX.toDouble(), wall.fromY.toDouble()))
                    lineTo(Point(wall.toX.toDouble(), wall.toY.toDouble()))
                }
            }
            for (f in world.furnitures) {
                stroke(Colors.ORANGE, lineWidth = 1.5) {
                    circle(Point(f.obstaclePositionX.toDouble(), f.obstaclePositionY.toDouble()), f.obstacleRadius.toDouble())
                }
            }
            for (z in world.spawnZones) {
                stroke(Colors.GREEN, lineWidth = 1.0) {
                    rect(z.originX.toDouble(), z.originY.toDouble(), (z.endX - z.originX).toDouble(), (z.endY - z.originY).toDouble())
                }
            }
        }
        debugOverlay.zIndex = 999.0
        debugOverlay.visible = false

        if (filesResourcesPath.isNotEmpty()) {
            try {
                val music = resourcesVfs["$filesResourcesPath/music/BradSucks_BadAttraction.ogg"].readMusic()
                music.playNoCancelForever()
            } catch (_: Exception) {}
        }

        // ── Input state ──────────────────────────────────────────────────────
        val dualStick = DualVirtualStickState()
        var pauseIconTouched = false  // set by touch handler, consumed by addUpdater

        // Register touch event handlers (event-based, fires on the KorGE main thread)
        touch {
            start { info ->
                val pos = Vec2(info.local.x.toFloat(), info.local.y.toFloat())
                // Pause icon hit (legacy: 1088-1135, 39-83 in 1280×720 logical)
                if (pos.x in PAUSE_ICON_X1..PAUSE_ICON_X2 && pos.y in PAUSE_ICON_Y1..PAUSE_ICON_Y2) {
                    pauseIconTouched = true
                    return@start
                }
                dualStick.onTouchDown(info.id, pos)
            }
            move { info ->
                dualStick.onTouchMove(info.id, Vec2(info.local.x.toFloat(), info.local.y.toFloat()))
            }
            end { info ->
                dualStick.onTouchUp(info.id)
            }
        }

        var totalSeconds = 0f
        var isPaused = false
        var isGameOver = false
        var isStageClear = false
        var stageClearTimer = 0f
        var debugF2 = false
        // Per-player direction tracking
        val lastFireAngles = MutableList(session.numPlayers) { FacingDirection.angleFrom(0f, -1f) }
        val lastMoveDxList = MutableList(session.numPlayers) { 0f }
        val lastMoveDyList = MutableList(session.numPlayers) { -1f }
        // Gamepad just-pressed prev state for P2-P4 (indices 0-2 = gamepads[0-2])
        val prevGpStart = BooleanArray(3)
        val prevGpR1    = BooleanArray(3)
        var currentInputMode = InputMode.KEYBOARD_MOUSE

        val capturedViews = views
        val sceneScope = this@GameplayScene

        addUpdater { dt: Duration ->
            val ks = capturedViews.input.keys

            // ── Determine input mode ─────────────────────────────────────────
            currentInputMode = when {
                capturedViews.input.isTouchDevice || dualStick.left.active || dualStick.right.active -> InputMode.TOUCH
                else -> InputMode.KEYBOARD_MOUSE
            }

            // ── Build semantic GameInput ─────────────────────────────────────
            val gameInput: GameInput = when (currentInputMode) {
                InputMode.TOUCH -> {
                    val consumed = pauseIconTouched
                    pauseIconTouched = false
                    GameInput(
                        movement = dualStick.movementVector,
                        aim = dualStick.aimVector,
                        firing = dualStick.isFiring,
                        pauseJustPressed = consumed,
                        confirmJustPressed = false,
                        cancelJustPressed = false,
                        weaponNextJustPressed = false,
                    )
                }
                else -> {
                    pauseIconTouched = false
                    var dx = 0f; var dy = 0f
                    if (ks[Key.W] || ks[Key.UP])    dy -= 1f
                    if (ks[Key.S] || ks[Key.DOWN])   dy += 1f
                    if (ks[Key.A] || ks[Key.LEFT])   dx -= 1f
                    if (ks[Key.D] || ks[Key.RIGHT])  dx += 1f
                    val len = sqrt(dx * dx + dy * dy)
                    if (len > 0f) { dx /= len; dy /= len }
                    val movVec = Vec2(dx, dy)
                    val firing = ks[Key.SPACE]
                    val aimVec = if (firing && len > 0f) movVec else Vec2(0f, 0f)
                    GameInput(
                        movement = movVec,
                        aim = aimVec,
                        firing = firing,
                        pauseJustPressed = ks.justPressed(Key.ESCAPE),
                        confirmJustPressed = ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE),
                        cancelJustPressed = ks.justPressed(Key.ESCAPE),
                        weaponNextJustPressed = ks.justPressed(Key.TAB),
                    )
                }
            }

            // ── Debug keys — direct keyboard only (developer tooling) ────────
            if (GameDebugConfig.ENABLED) {
                if (ks.justPressed(Key.F2)) debugF2 = !debugF2
                if (debugMode && debugF2) {
                    val jumpLevel = when {
                        ks.justPressed(Key.N1) || ks.justPressed(Key.NUMPAD1) -> 1
                        ks.justPressed(Key.N2) || ks.justPressed(Key.NUMPAD2) -> 2
                        ks.justPressed(Key.N3) || ks.justPressed(Key.NUMPAD3) -> 3
                        ks.justPressed(Key.N4) || ks.justPressed(Key.NUMPAD4) -> 4
                        ks.justPressed(Key.N5) || ks.justPressed(Key.NUMPAD5) -> 5
                        ks.justPressed(Key.N6) || ks.justPressed(Key.NUMPAD6) -> 6
                        ks.justPressed(Key.N7) || ks.justPressed(Key.NUMPAD7) -> 7
                        ks.justPressed(Key.N8) || ks.justPressed(Key.NUMPAD8) -> 8
                        ks.justPressed(Key.N9) || ks.justPressed(Key.NUMPAD9) -> 9
                        ks.justPressed(Key.N0) || ks.justPressed(Key.NUMPAD0) -> 10
                        else -> -1
                    }
                    if (jumpLevel in 1..10) {
                        val jumpSession = session.copy(currentLevel = jumpLevel, debugLevelJump = true)
                        sceneScope.launch {
                            sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, jumpSession)
                            }
                        }
                        return@addUpdater
                    }
                }
            }

            // ── Game-over menu ───────────────────────────────────────────────
            if (isGameOver) {
                if (gameInput.movement.y < -0.5f || ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                    gameOverMenuIndex = (gameOverMenuIndex - 1 + gameOverEntries.size) % gameOverEntries.size
                    refreshGameOverColors()
                }
                if (gameInput.movement.y > 0.5f || ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                    gameOverMenuIndex = (gameOverMenuIndex + 1) % gameOverEntries.size
                    refreshGameOverColors()
                }
                if (gameInput.confirmJustPressed || ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                    sceneScope.launch {
                        when (gameOverMenuIndex) {
                            0 -> sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session)
                            }
                            1 -> sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath,
                                    GameSession.newGame(session.characterIndex))
                            }
                            2 -> sceneContainer.changeTo {
                                MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                        }
                    }
                }
                return@addUpdater
            }

            // ── Pause menu ───────────────────────────────────────────────────
            if (isPaused) {
                if (gameInput.movement.y < -0.5f || ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                    pauseMenuIndex = (pauseMenuIndex - 1 + pauseEntries.size) % pauseEntries.size
                    refreshPauseColors()
                }
                if (gameInput.movement.y > 0.5f || ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                    pauseMenuIndex = (pauseMenuIndex + 1) % pauseEntries.size
                    refreshPauseColors()
                }
                val resumeTouch = gameInput.pauseJustPressed || gameInput.cancelJustPressed || ks.justPressed(Key.ESCAPE)
                if (resumeTouch) {
                    isPaused = false
                    dualStick.resetAll()
                    pauseOverlay.visible = false; pauseLabel.visible = false
                    pauseEntryViews.forEach { it.visible = false }
                }
                if (gameInput.confirmJustPressed || ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                    sceneScope.launch {
                        when (pauseMenuIndex) {
                            0 -> {
                                isPaused = false
                                dualStick.resetAll()
                                pauseOverlay.visible = false; pauseLabel.visible = false
                                pauseEntryViews.forEach { it.visible = false }
                            }
                            1 -> sceneContainer.changeTo {
                                HowToPlayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                            2 -> sceneContainer.changeTo {
                                OptionsScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                            3 -> sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session)
                            }
                            4 -> sceneContainer.changeTo {
                                MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                        }
                    }
                }
                return@addUpdater
            }

            // ── Enter pause (P1 or any connected gamepad START) ──────────────
            val anyGpPauseJP = (0 until minOf(session.numPlayers - 1, 3)).any { gpIdx ->
                val gp = capturedViews.input.gamepads[gpIdx]
                gp.connected && gp.start && !prevGpStart[gpIdx]
            }
            if (gameInput.pauseJustPressed || anyGpPauseJP) {
                isPaused = true
                pauseMenuIndex = 0
                dualStick.resetAll()   // clear sticks so no movement/fire resumes on unpause
                pauseOverlay.visible = true; pauseLabel.visible = true
                pauseEntryViews.forEach { it.visible = true }
                refreshPauseColors()
                return@addUpdater
            }

            if (isPaused || isGameOver) return@addUpdater

            // ── Stage-clear advance ──────────────────────────────────────────
            if (isStageClear) {
                stageClearTimer += dt.inWholeMilliseconds / 1000f
                if (stageClearTimer >= 2.5f || gameInput.confirmJustPressed) {
                    val updatedSession = session.copy(
                        currentLevel = levelNumber,
                        score = world.player1.score,
                        lives = world.player1.lives
                    ).withNextLevel()
                    if (updatedSession.currentLevel > GameSettings.levelsUnlocked) {
                        GameSettings.levelsUnlocked = updatedSession.currentLevel.coerceAtMost(GameSession.MAX_CAMPAIGN_LEVELS)
                    }
                    sceneScope.launch {
                        if (updatedSession.isCampaignComplete()) {
                            sceneContainer.changeTo {
                                MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                        } else {
                            sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, updatedSession)
                            }
                        }
                    }
                }
                return@addUpdater
            }

            val dtSec = dt.inWholeMilliseconds / 1000f
            totalSeconds += dtSec

            // ── Debug overlays ───────────────────────────────────────────────
            if (GameDebugConfig.ENABLED) {
                if (ks.justPressed(Key.F1)) { debugMode = !debugMode; debugOverlay.visible = debugMode }
                if (debugMode) {
                    if (ks.justPressed(Key.K)) world.enemySystem.enemies.forEach { if (it.isActive) it.crash(totalSeconds) }
                    if (ks.justPressed(Key.G)) {
                        world.player1.ammo[GunType.MACHINEGUN.id] = 50
                        world.player1.ammo[GunType.SHOTGUN.id] = 25
                        world.player1.ammo[GunType.FLAMETHROWER.id] = 25
                        world.player1.ammo[GunType.GRENADE.id] = 5
                    }
                    if (ks.justPressed(Key.H)) {
                        world.player1.lifecounter -= 20
                        if (world.player1.lifecounter <= 0) {
                            world.player1.lives--; world.player1.lifecounter = 100; world.player1.destroy(totalSeconds)
                        }
                    }
                }
            }

            // ── Per-player weapon / movement / fire ──────────────────────────
            for ((idx, player) in world.players.withIndex()) {
                // Build GameInput for this player
                val pInput: GameInput = if (idx == 0) {
                    gameInput  // P1: keyboard / touch (built above)
                } else {
                    val gpIdx = idx - 1  // P2 = gamepads[0], P3 = gamepads[1], P4 = gamepads[2]
                    val gp = capturedViews.input.gamepads[gpIdx]
                    if (!gp.connected) GameInput.EMPTY
                    else {
                        val lx = gp.lx
                        val ly = gp.ly   // ly: -1=down, +1=up (XNA convention); negate for screen Y+down
                        val rx = gp.rx
                        val ry = gp.ry
                        val movLen = sqrt(lx * lx + ly * ly)
                        val movNormX = if (movLen > 0.1f) lx / movLen else 0f
                        val movNormY = if (movLen > 0.1f) -ly / movLen else 0f
                        val aimLen = sqrt(rx * rx + ry * ry)
                        val firing = aimLen > 0.3f
                        val startJP = gp.start && !prevGpStart[gpIdx]
                        val r1JP    = gp.r1    && !prevGpR1[gpIdx]
                        GameInput(
                            movement = Vec2(movNormX, movNormY),
                            aim = Vec2(rx, if (firing) -ry else 0f),
                            firing = firing,
                            pauseJustPressed = startJP,
                            confirmJustPressed = gp.south,
                            cancelJustPressed = gp.east,
                            weaponNextJustPressed = r1JP,
                        )
                    }
                }

                // Weapon cycle
                if (pInput.weaponNextJustPressed) player.cycleWeapon()

                // Movement
                val playerCanAct = player.status == ObjectStatus.ACTIVE || player.status == ObjectStatus.IMMUNE
                val mvx = pInput.movement.x
                val mvy = pInput.movement.y
                if (playerCanAct && (mvx != 0f || mvy != 0f)) {
                    lastMoveDxList[idx] = mvx; lastMoveDyList[idx] = mvy
                    lastFireAngles[idx] = FacingDirection.angleFrom(mvx, mvy)
                    val speed = player.pixelsPerSecond * dtSec
                    val delta = Point(mvx.toDouble() * speed, mvy.toDouble() * speed)
                    player.position = CollisionSystem.resolveMovement(player.position, delta, world.walls, world.furnitures)
                }

                // Fire
                if (player.status == ObjectStatus.ACTIVE && pInput.firing) {
                    if (pInput.aim.length() > VirtualThumbstickLogic.DEAD_ZONE / VirtualThumbstickLogic.maxRadius) {
                        lastFireAngles[idx] = FacingDirection.angleFrom(pInput.aim.x, pInput.aim.y)
                    }
                    CombatSystem.tryFire(player, world.combatStates[idx], totalSeconds, lastFireAngles[idx], shooterIndex = idx)
                }
            }

            // Update gamepad prev state (after consuming above)
            for (gpIdx in 0 until minOf(session.numPlayers - 1, 3)) {
                val gp = capturedViews.input.gamepads[gpIdx]
                prevGpStart[gpIdx] = gp.connected && gp.start
                prevGpR1[gpIdx]    = gp.connected && gp.r1
            }

            // Prune out-of-bounds projectiles for all players
            for (cs in world.combatStates) { CombatSystem.pruneOutOfBounds(cs, totalSeconds) }

            // Multi-player bullet/contact damage
            val kills = DamageSystem.processBulletCollisionsMulti(world.combatStates, world.enemySystem.enemies, world.players, totalSeconds)
            repeat(kills) {
                world.powerUpSystem.trySpawnOnKill(world.player1.position.x.toFloat(), world.player1.position.y.toFloat(), totalSeconds)
            }

            DamageSystem.processEnemyContactMulti(world.enemySystem.enemies, world.players, totalSeconds)
            world.powerUpSystem.update(totalSeconds, world.players)

            val waveSystem = world.waveSystem
            if (!waveSystem.isLevelComplete) {
                if (!waveSystem.spawned) {
                    val wave = waveSystem.currentWave
                    if (wave != null) {
                        val newEnemies = world.spawnSystem.spawnWave(
                            enemiesCount = wave.enemiesCount,
                            spawnZones = world.spawnZones,
                            furnitures = world.furnitures,
                            subLevelIndex = waveSystem.waveNumber - 1
                        )
                        world.enemySystem.addEnemies(newEnemies)
                        spawnEnemyViews(newEnemies)
                        waveSystem.markSpawned()
                    }
                }

                // Update per-player steering from current position/velocity
                for ((idx, se) in world.playerSteeringEntities.withIndex()) {
                    val p = world.players[idx]
                    se.position = Vec2(p.position.x.toFloat(), p.position.y.toFloat())
                    se.velocity = Vec2(
                        lastMoveDxList[idx] * AVATAR_PIXELS_PER_SECOND / 60f,
                        lastMoveDyList[idx] * AVATAR_PIXELS_PER_SECOND / 60f
                    )
                }
                world.enemySystem.update(dtSec, world.players, world.playerSteeringEntities, totalSeconds)

                if (waveSystem.spawned && world.enemySystem.activeCount == 0) {
                    waveSystem.advanceWave()
                }
            }

            world.players.forEach { it.update(totalSeconds) }

            // ── Game over check (all players must be eliminated) ──────────────
            val allDead = world.players.all { it.status == ObjectStatus.INACTIVE && it.lives <= 0 }
            if (allDead && !isGameOver) {
                isGameOver = true
                gameOverMenuIndex = 0
                gameOverOverlay.visible = true
                gameOverImgView?.visible = true
                gameOverEntryViews.forEach { it.visible = true }
                refreshGameOverColors()
            }

            // ── Stage cleared ─────────────────────────────────────────────────
            if (waveSystem.isLevelComplete && !isStageClear) {
                isStageClear = true
                stageClearedLabel.visible = true
            }

            // ── Enemy views sync ──────────────────────────────────────────────
            for ((enemy, view) in enemyViews) {
                if (enemy.status == ObjectStatus.INACTIVE) { view.visible = false; continue }
                if (enemy.status == ObjectStatus.DYING) {
                    view.visible = true; view.colorMul = Colors.RED
                } else {
                    view.colorMul = Colors.WHITE; view.visible = true
                }
                val ex = enemy.entity.position.x.toDouble()
                val ey = enemy.entity.position.y.toDouble()
                val yOffset = when (enemy.type) {
                    EnemyType.ZOMBIE -> 50.0
                    EnemyType.RAT    -> Rat.Y_OFFSET.toDouble()
                    EnemyType.WOLF   -> Wolf.Y_OFFSET.toDouble()
                    else             -> 50.0
                }
                view.x = ex; view.y = ey - yOffset; view.zIndex = ey
                if (enemy.entity.velocity.x != 0f) { view.scaleX = if (enemy.entity.velocity.x > 0) 1.0 else -1.0 }
            }

            // ── Player view sync (all players) ────────────────────────────────
            val blinkVisible = (totalSeconds * 10).toInt() % 2 == 0
            for ((idx, pc) in playerContainers.withIndex()) {
                val p = world.players[idx]
                pc.visible = when (p.status) {
                    ObjectStatus.IMMUNE   -> blinkVisible
                    ObjectStatus.DYING    -> false
                    ObjectStatus.INACTIVE -> false
                    else                  -> true
                }
                pc.x = p.position.x + PLAYER_RENDER_OFFSET_X
                pc.y = p.position.y + PLAYER_RENDER_OFFSET_Y
                pc.zIndex = p.position.y
            }

            // ── Bullet rendering (all players' projectiles) ───────────────────
            bulletGraphics.updateShape {
                for (cs in world.combatStates) {
                    for (bullet in cs.bullets) {
                        val (bx, by) = bullet.positionAt(totalSeconds)
                        fill(Colors.YELLOW) { circle(Point(bx.toDouble(), by.toDouble()), 3.0) }
                    }
                    for (shell in cs.shotgunShells) {
                        for (pi in 0..2) {
                            val (px, py) = shell.pelletPositionAt(pi, totalSeconds)
                            fill(Colors.ORANGE) { circle(Point(px.toDouble(), py.toDouble()), 2.5) }
                        }
                    }
                }
            }

            // ── Touch overlay (virtual sticks + pause icon) ───────────────────
            touchOverlayGraphics.updateShape {
                if (currentInputMode == InputMode.TOUCH && !isPaused && !isGameOver) {
                    val maxR = VirtualThumbstickLogic.maxRadius.toDouble()
                    val knobR = 18.0

                    // Left stick
                    if (dualStick.left.active) {
                        val cx = dualStick.left.center.x.toDouble()
                        val cy = dualStick.left.center.y.toDouble()
                        stroke(RGBA(200, 200, 200, 120), lineWidth = 2.0) { circle(Point(cx, cy), maxR) }
                        val disp = dualStick.left.clampedDisplacement
                        fill(RGBA(255, 255, 255, 180)) { circle(Point(cx + disp.x, cy + disp.y), knobR) }
                    }

                    // Right stick
                    if (dualStick.right.active) {
                        val cx = dualStick.right.center.x.toDouble()
                        val cy = dualStick.right.center.y.toDouble()
                        stroke(RGBA(200, 200, 200, 120), lineWidth = 2.0) { circle(Point(cx, cy), maxR) }
                        val disp = dualStick.right.clampedDisplacement
                        val knobColor = if (dualStick.isFiring) RGBA(255, 80, 80, 200) else RGBA(255, 255, 255, 180)
                        fill(knobColor) { circle(Point(cx + disp.x, cy + disp.y), knobR) }
                    }

                    // Pause icon placeholder (legacy: UI/pause_iconWP, 1088-1135, 39-83)
                    fill(RGBA(80, 80, 80, 160)) {
                        rect(PAUSE_ICON_X1.toDouble(), PAUSE_ICON_Y1.toDouble(),
                            (PAUSE_ICON_X2 - PAUSE_ICON_X1).toDouble(),
                            (PAUSE_ICON_Y2 - PAUSE_ICON_Y1).toDouble())
                    }
                    // Two pause bars
                    fill(RGBA(240, 240, 240, 220)) {
                        rect(1096.0, 46.0, 10.0, 31.0)
                        rect(1114.0, 46.0, 10.0, 31.0)
                    }
                }
            }

            // ── HUD update ────────────────────────────────────────────────────
            val gunLabel = when (world.player1.currentGun) {
                GunType.PISTOL       -> "PISTOL (INF)"
                GunType.SHOTGUN      -> "SHOTGUN (${world.player1.ammo[GunType.SHOTGUN.id]})"
                GunType.MACHINEGUN   -> "MG (${world.player1.ammo[GunType.MACHINEGUN.id]})"
                GunType.FLAMETHROWER -> "FLAME (${world.player1.ammo[GunType.FLAMETHROWER.id]})"
                GunType.GRENADE      -> "GRENADE (${world.player1.ammo[GunType.GRENADE.id]})"
            }
            hudHealth.text = "HP: ${world.player1.lifecounter}"
            hudHealth.color = when {
                world.player1.lifecounter > 60 -> Colors.LIME
                world.player1.lifecounter > 30 -> Colors.YELLOW
                else -> Colors.RED
            }
            hudLives.text   = "Lives: ${world.player1.lives}"
            hudScore.text   = "Score: ${world.player1.score}"
            hudWeapon.text  = gunLabel
            hudWave.text    = if (waveSystem.isLevelComplete) "CLEARED" else "Wave ${waveSystem.waveNumber}/${waveSystem.totalWaves}"
            hudLevel.text   = "LEVEL $levelNumber"
            hudEnemies.text = "Enemies: ${world.enemySystem.activeCount}"
        }

        for ((idx, pc) in playerContainers.withIndex()) {
            pc.x = world.players[idx].position.x + PLAYER_RENDER_OFFSET_X
            pc.y = world.players[idx].position.y + PLAYER_RENDER_OFFSET_Y
        }
    }

    private suspend fun tryLoadBitmap(path: String): Bitmap? = try { resourcesVfs[path].readBitmap() } catch (_: Exception) { null }

    private suspend fun loadFurnitureBitmaps(): Map<Pair<FurnitureType, FurnitureOrientation?>, Bitmap?> {
        val base = "$assetBase/furniture"
        return mapOf(
            Pair(FurnitureType.ARBOL, null)                             to tryLoadBitmap("$base/arbol.png"),
            Pair(FurnitureType.BASURA, null)                            to tryLoadBitmap("$base/basura.png"),
            Pair(FurnitureType.COCHE_ARDIENDO, null)                    to tryLoadBitmap("$base/coche_ardiendo.png"),
            Pair(FurnitureType.PUENTE, null)                            to tryLoadBitmap("$base/puente.png"),
            Pair(FurnitureType.BANCO, FurnitureOrientation.SOUTH_EAST)  to tryLoadBitmap("$base/banco_se.png"),
            Pair(FurnitureType.BANCO, FurnitureOrientation.SOUTH_WEST)  to tryLoadBitmap("$base/banco_sw.png"),
            Pair(FurnitureType.BANCO, FurnitureOrientation.NORTH_EAST)  to tryLoadBitmap("$base/banco_se.png"),
            Pair(FurnitureType.BANCO, FurnitureOrientation.NORTH_WEST)  to tryLoadBitmap("$base/banco_sw.png"),
            Pair(FurnitureType.FAROLA, FurnitureOrientation.NORTH_EAST) to tryLoadBitmap("$base/farola_ne.png"),
            Pair(FurnitureType.FAROLA, FurnitureOrientation.NORTH_WEST) to tryLoadBitmap("$base/farola_nw.png"),
            Pair(FurnitureType.FAROLA, FurnitureOrientation.SOUTH_EAST) to tryLoadBitmap("$base/farola_se.png"),
            Pair(FurnitureType.FAROLA, FurnitureOrientation.SOUTH_WEST) to tryLoadBitmap("$base/farola_sw.png"),
        )
    }

    private fun SContainer.buildFurnitureViews(furnitures: List<Furniture>, bitmaps: Map<Pair<FurnitureType, FurnitureOrientation?>, Bitmap?>) {
        for (f in furnitures) {
            val bmp = bitmaps[Pair(f.type, f.orientation)] ?: bitmaps[Pair(f.type, null)]
            if (bmp != null) {
                image(bmp) {
                    x = f.positionX.toDouble(); y = f.positionY.toDouble()
                    zIndex = (f.positionY + bmp.height).toDouble()
                    smoothing = false
                }
            }
        }
    }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
