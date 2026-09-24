package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.combat.FacingDirection
import com.retrowax.zombusters.game.enemy.BaseEnemy
import com.retrowax.zombusters.game.enemy.Rat
import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.enemy.Wolf
import com.retrowax.zombusters.game.engine.CollisionSystem
import com.retrowax.zombusters.game.model.AVATAR_PIXELS_PER_SECOND
import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.model.FurnitureOrientation
import com.retrowax.zombusters.game.model.FurnitureType
import com.retrowax.zombusters.game.model.EnemyType
import com.retrowax.zombusters.game.model.GameSession
import com.retrowax.zombusters.game.model.GunType
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
import korlibs.korge.scene.Scene
import korlibs.korge.view.SContainer
import korlibs.korge.view.container
import korlibs.korge.view.SpriteAnimation
import korlibs.korge.view.addUpdater
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

// Debug level-jump shortcut range
private const val DEBUG_MIN_JUMP_LEVEL = 1
private const val DEBUG_MAX_JUMP_LEVEL = 10

/**
 * Main gameplay scene — driven by [GameSession].
 *
 * Step 6 additions vs Step 5:
 *   - Accepts [GameSession] with currentLevel and characterIndex
 *   - Loads level dynamically from session.currentLevel (not hardcoded Level 1)
 *   - Uses session.characterSpritePath for player sprite selection
 *   - Stage-clear advances session to next level via [GameplayScene] with updated session
 *   - Game-over shows [GameOverMenuScene]
 *   - Debug level-jump: F2+1..0 selects level 1-10 in debug mode
 *   - Pause menu improved per legacy GamePlayMenu.cs entries
 *   - HUD carries session score (accumulated across levels)
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

        val world = GameplayWorld(levelDef)
        // Carry session score and lives into the new world's player
        world.player1.score = session.score
        world.player1.lives = session.lives

        // Load bitmaps
        val levelPad = levelNumber.toString().padStart(2, '0')
        val mapBitmap       = tryLoadBitmap("$assetBase/levels/level$levelPad/map.png")
        val charPath        = session.characterSpritePath
        val playerIdleBmp   = tryLoadBitmap("$assetBase/characters/$charPath/idle.png")
        val furnitureBitmaps = loadFurnitureBitmaps()
        val zombieBitmap    = tryLoadBitmap("$assetBase/enemies/zombie/walk1.png")
        val ratIdleBitmap   = tryLoadBitmap("$assetBase/enemies/rat/idle.png")
        val ratRunBitmap    = tryLoadBitmap("$assetBase/enemies/rat/run.png")
        val wolfIdleBitmap  = tryLoadBitmap("$assetBase/enemies/wolf/idle.png")
        val wolfRunBitmap   = tryLoadBitmap("$assetBase/enemies/wolf/run.png")
        val minotaurBitmap  = tryLoadBitmap("$assetBase/enemies/minotaur/walk.png")

        val zombieAnim   = zombieBitmap?.let { SpriteAnimation(it, ZOMBIE_FRAME_W, ZOMBIE_FRAME_H, ZOMBIE_COLS, 1) }
        val ratIdleAnim  = ratIdleBitmap?.let { SpriteAnimation(it, RAT_FRAME_W, RAT_FRAME_H, RAT_IDLE_COLS, 1) }
        val ratRunAnim   = ratRunBitmap?.let { SpriteAnimation(it, RAT_FRAME_W, RAT_FRAME_H, RAT_RUN_COLS, 1) }
        val wolfIdleAnim = wolfIdleBitmap?.let { SpriteAnimation(it, WOLF_FRAME_W, WOLF_FRAME_H, WOLF_IDLE_COLS, 1) }
        val wolfRunAnim  = wolfRunBitmap?.let { SpriteAnimation(it, WOLF_FRAME_W, WOLF_FRAME_H, WOLF_RUN_COLS, 1) }

        // Map background
        if (mapBitmap != null) {
            image(mapBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            // Placeholder for levels whose map hasn't been extracted yet
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x22, 0x33, 0x22, 0xFF))
            text("Level $levelNumber — map not extracted") {
                textSize = 20.0; color = Colors.ORANGE; x = 20.0; y = 20.0; zIndex = 0.1
                font = ZombustersFonts.menuInfo
            }
        }

        buildFurnitureViews(world.furnitures, furnitureBitmaps)

        // Player container
        val playerContainer = container { zIndex = world.player1.position.y }
        if (playerIdleBmp != null) {
            val idleAnim = SpriteAnimation(playerIdleBmp, JADE_IDLE_FRAME_W, JADE_IDLE_FRAME_H, JADE_IDLE_COLS, 1)
            val playerSprite = playerContainer.sprite(idleAnim) { smoothing = false }
            playerSprite.playAnimationLooped(idleAnim, (1.0 / JADE_IDLE_FPS).seconds)
        } else {
            playerContainer.solidRect(20.0, 20.0, Colors.CYAN).also { it.x = -10.0; it.y = -10.0 }
        }

        val bulletGraphics = cpuGraphics { }
        bulletGraphics.zIndex = 900.0

        // HUD
        val hudHealth = text("HP: 100") { textSize = 14.0; color = Colors.LIME; x = 8.0; y = 4.0; zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudLives  = text("Lives: ${session.lives}") { textSize = 14.0; color = Colors.WHITE; x = 90.0; y = 4.0; zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudScore  = text("Score: ${session.score}") { textSize = 14.0; color = Colors.YELLOW; x = 170.0; y = 4.0; zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudWeapon = text("PISTOL (INF)") { textSize = 14.0; color = Colors.ORANGE; x = 270.0; y = 4.0; zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudWave   = text("Wave 1") { textSize = 14.0; color = Colors.LIGHTGRAY; x = 430.0; y = 4.0; zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudLevel  = text("LEVEL $levelNumber") { textSize = 14.0; color = Colors.WHITE; x = 580.0; y = 4.0; zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudEnemies = text("Enemies: 0") { textSize = 14.0; color = Colors.LIGHTGRAY; x = 680.0; y = 4.0; zIndex = 1000.0; font = ZombustersFonts.menuInfo }

        // Pause overlay (legacy GamePlayMenu entries)
        val pauseOverlay = solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0, 0, 0, 160)).apply { zIndex = 1100.0; visible = false }
        val pauseLabel = text("PAUSED") {
            textSize = 48.0; color = Colors.WHITE
            x = GAME_WIDTH / 2.0 - 80.0; y = GAME_HEIGHT / 2.0 - 80.0; zIndex = 1101.0; visible = false
            font = ZombustersFonts.menuHeader
        }
        // Legacy pause menu entries
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

        // Game-over overlay (legacy GameOverMenu — shows gameover.png + menu)
        val gameOverBmp = tryLoadBitmap("$assetBase/hud/gameover.png")
        val gameOverOverlay = solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0, 0, 0, 200)).apply { zIndex = 1200.0; visible = false }
        val gameOverImgView = if (gameOverBmp != null) {
            image(gameOverBmp) {
                x = GAME_WIDTH / 2.0 - gameOverBmp.width / 2.0
                y = GAME_HEIGHT * 0.1
                zIndex = 1201.0; visible = false; smoothing = true
            }
        } else null
        // Legacy GameOverMenu entries
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

        // Music: random from pool (MusicComponent plays random in legacy)
        if (filesResourcesPath.isNotEmpty()) {
            try {
                val music = resourcesVfs["$filesResourcesPath/music/BradSucks_BadAttraction.ogg"].readMusic()
                music.playNoCancelForever()
            } catch (_: Exception) {}
        }

        var totalSeconds = 0f
        var isPaused = false
        var isGameOver = false
        var isStageClear = false
        var stageClearTimer = 0f
        var debugF2 = false
        var lastFireAngle = FacingDirection.angleFrom(0f, -1f)
        var lastMoveDx = 0f
        var lastMoveDy = -1f

        val capturedViews = views
        val sceneScope = this@GameplayScene

        addUpdater { dt: Duration ->
            val ks = capturedViews.input.keys

            // Debug level-jump (F2 + digit)
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

            // Game-over menu navigation
            if (isGameOver) {
                if (ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                    gameOverMenuIndex = (gameOverMenuIndex - 1 + gameOverEntries.size) % gameOverEntries.size
                    refreshGameOverColors()
                }
                if (ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                    gameOverMenuIndex = (gameOverMenuIndex + 1) % gameOverEntries.size
                    refreshGameOverColors()
                }
                if (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                    sceneScope.launch {
                        when (gameOverMenuIndex) {
                            0 -> sceneContainer.changeTo {   // Restart this wave
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session)
                            }
                            1 -> sceneContainer.changeTo {   // Restart from beginning
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath,
                                    GameSession.newGame(session.characterIndex))
                            }
                            2 -> sceneContainer.changeTo {   // Return to main menu
                                MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                        }
                    }
                }
                return@addUpdater
            }

            // Pause menu navigation (legacy GamePlayMenu entries)
            if (isPaused) {
                if (ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                    pauseMenuIndex = (pauseMenuIndex - 1 + pauseEntries.size) % pauseEntries.size
                    refreshPauseColors()
                }
                if (ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                    pauseMenuIndex = (pauseMenuIndex + 1) % pauseEntries.size
                    refreshPauseColors()
                }
                if (ks.justPressed(Key.ESCAPE)) {
                    isPaused = false
                    pauseOverlay.visible = false; pauseLabel.visible = false
                    pauseEntryViews.forEach { it.visible = false }
                }
                if (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                    sceneScope.launch {
                        when (pauseMenuIndex) {
                            0 -> {   // Resume
                                isPaused = false
                                pauseOverlay.visible = false; pauseLabel.visible = false
                                pauseEntryViews.forEach { it.visible = false }
                            }
                            1 -> sceneContainer.changeTo {   // How To Play
                                HowToPlayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                            2 -> sceneContainer.changeTo {   // Options
                                OptionsScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                            3 -> sceneContainer.changeTo {   // Restart Level
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session)
                            }
                            4 -> sceneContainer.changeTo {   // Quit to Main Menu
                                MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                        }
                    }
                }
                return@addUpdater
            }

            // ESC → pause
            if (ks.justPressed(Key.ESCAPE)) {
                isPaused = true
                pauseMenuIndex = 0
                pauseOverlay.visible = true; pauseLabel.visible = true
                pauseEntryViews.forEach { it.visible = true }
                refreshPauseColors()
                return@addUpdater
            }

            if (isPaused || isGameOver) return@addUpdater

            // Stage-clear countdown before advancing
            if (isStageClear) {
                stageClearTimer += dt.inWholeMilliseconds / 1000f
                if (stageClearTimer >= 2.5f || ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                    val updatedSession = session.copy(
                        currentLevel = levelNumber,
                        score = world.player1.score,
                        lives = world.player1.lives
                    ).withNextLevel()
                    sceneScope.launch {
                        if (updatedSession.isCampaignComplete()) {
                            // TODO Phase AH: proper ending screen
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

            if (ks.justPressed(Key.TAB)) world.player1.cycleWeapon()

            val playerCanAct = world.player1.status == ObjectStatus.ACTIVE || world.player1.status == ObjectStatus.IMMUNE
            var dx = 0f; var dy = 0f
            if (ks[Key.W] || ks[Key.UP]) dy -= 1f
            if (ks[Key.S] || ks[Key.DOWN]) dy += 1f
            if (ks[Key.A] || ks[Key.LEFT]) dx -= 1f
            if (ks[Key.D] || ks[Key.RIGHT]) dx += 1f

            if (playerCanAct && (dx != 0f || dy != 0f)) {
                val len = sqrt(dx * dx + dy * dy)
                dx /= len; dy /= len
                lastMoveDx = dx; lastMoveDy = dy
                lastFireAngle = FacingDirection.angleFrom(dx, dy)
                val speed = world.player1.pixelsPerSecond * dtSec
                val delta = Point(dx.toDouble() * speed, dy.toDouble() * speed)
                world.player1.position = CollisionSystem.resolveMovement(world.player1.position, delta, world.walls, world.furnitures)
            }

            if (world.player1.status == ObjectStatus.ACTIVE && ks[Key.SPACE]) {
                CombatSystem.tryFire(world.player1, world.combatState, totalSeconds, lastFireAngle)
            }

            CombatSystem.pruneOutOfBounds(world.combatState, totalSeconds)
            val kills = DamageSystem.processBulletCollisions(world.combatState, world.enemySystem.enemies, world.player1, totalSeconds)
            repeat(kills) {
                world.powerUpSystem.trySpawnOnKill(world.player1.position.x.toFloat(), world.player1.position.y.toFloat(), totalSeconds)
            }

            if (world.player1.status == ObjectStatus.ACTIVE) {
                DamageSystem.processEnemyContact(world.enemySystem.enemies, world.player1, totalSeconds)
            }

            world.powerUpSystem.update(totalSeconds, world.player1)

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

                val playerPos = Vec2(world.player1.position.x.toFloat(), world.player1.position.y.toFloat())
                val playerSteering = SteeringEntity(playerPos)
                playerSteering.velocity = Vec2(lastMoveDx * AVATAR_PIXELS_PER_SECOND / 60f, lastMoveDy * AVATAR_PIXELS_PER_SECOND / 60f)
                world.enemySystem.update(dtSec, playerPos, playerSteering, totalSeconds)

                if (waveSystem.spawned && world.enemySystem.activeCount == 0) {
                    waveSystem.advanceWave()
                }
            }

            world.player1.update(totalSeconds)

            // Check game over
            if (world.player1.status == ObjectStatus.INACTIVE && world.player1.lives <= 0) {
                isGameOver = true
                gameOverMenuIndex = 0
                gameOverOverlay.visible = true
                gameOverImgView?.visible = true
                gameOverEntryViews.forEach { it.visible = true }
                refreshGameOverColors()
            }

            // Stage cleared
            if (waveSystem.isLevelComplete && !isStageClear) {
                isStageClear = true
                stageClearedLabel.visible = true
            }

            // Sync enemy views
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

            // Player
            val blinkVisible = (totalSeconds * 10).toInt() % 2 == 0
            playerContainer.visible = when (world.player1.status) {
                ObjectStatus.IMMUNE   -> blinkVisible
                ObjectStatus.DYING    -> false
                ObjectStatus.INACTIVE -> false
                else                  -> true
            }
            playerContainer.x = world.player1.position.x + PLAYER_RENDER_OFFSET_X
            playerContainer.y = world.player1.position.y + PLAYER_RENDER_OFFSET_Y
            playerContainer.zIndex = world.player1.position.y

            // Bullets
            bulletGraphics.updateShape {
                for (bullet in world.combatState.bullets) {
                    val (bx, by) = bullet.positionAt(totalSeconds)
                    fill(Colors.YELLOW) { circle(Point(bx.toDouble(), by.toDouble()), 3.0) }
                }
                for (shell in world.combatState.shotgunShells) {
                    for (pi in 0..2) {
                        val (px, py) = shell.pelletPositionAt(pi, totalSeconds)
                        fill(Colors.ORANGE) { circle(Point(px.toDouble(), py.toDouble()), 2.5) }
                    }
                }
            }

            // HUD
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
            hudLives.text  = "Lives: ${world.player1.lives}"
            hudScore.text  = "Score: ${world.player1.score}"
            hudWeapon.text = gunLabel
            hudWave.text   = if (waveSystem.isLevelComplete) "CLEARED" else "Wave ${waveSystem.waveNumber}/${waveSystem.totalWaves}"
            hudLevel.text  = "LEVEL $levelNumber"
            hudEnemies.text = "Enemies: ${world.enemySystem.activeCount}"
        }

        playerContainer.x = world.player1.position.x + PLAYER_RENDER_OFFSET_X
        playerContainer.y = world.player1.position.y + PLAYER_RENDER_OFFSET_Y
    }

    private suspend fun tryLoadBitmap(path: String): Bitmap? = try { resourcesVfs[path].readBitmap() } catch (_: Exception) { null }

    private suspend fun loadFurnitureBitmaps(): Map<Pair<FurnitureType, FurnitureOrientation?>, Bitmap?> {
        val base = "$assetBase/furniture"
        return mapOf(
            Pair(FurnitureType.ARBOL, null)                            to tryLoadBitmap("$base/arbol.png"),
            Pair(FurnitureType.BASURA, null)                           to tryLoadBitmap("$base/basura.png"),
            Pair(FurnitureType.COCHE_ARDIENDO, null)                   to tryLoadBitmap("$base/coche_ardiendo.png"),
            Pair(FurnitureType.PUENTE, null)                           to tryLoadBitmap("$base/puente.png"),
            Pair(FurnitureType.BANCO, FurnitureOrientation.SOUTH_EAST) to tryLoadBitmap("$base/banco_se.png"),
            Pair(FurnitureType.BANCO, FurnitureOrientation.SOUTH_WEST) to tryLoadBitmap("$base/banco_sw.png"),
            Pair(FurnitureType.BANCO, FurnitureOrientation.NORTH_EAST) to tryLoadBitmap("$base/banco_se.png"),
            Pair(FurnitureType.BANCO, FurnitureOrientation.NORTH_WEST) to tryLoadBitmap("$base/banco_sw.png"),
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
