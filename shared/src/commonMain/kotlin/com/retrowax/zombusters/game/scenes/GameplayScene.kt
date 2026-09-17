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
import com.retrowax.zombusters.game.model.GunType
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.systems.CombatSystem
import com.retrowax.zombusters.game.systems.DamageSystem
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

// From legacy GamePlayScreen.cs line 1458
private const val PLAYER_RENDER_OFFSET_X = -20.0
private const val PLAYER_RENDER_OFFSET_Y = -55.0

// Jade idle animation from AnimationDef.xml JadeIdleTrunkDef
private const val JADE_IDLE_FRAME_W = 42
private const val JADE_IDLE_FRAME_H = 55
private const val JADE_IDLE_COLS = 11
private const val JADE_IDLE_FPS = 15

// Zombie walk: ZombieDef — 48x55, 8 cols, Speed=10
private const val ZOMBIE_FRAME_W = 48
private const val ZOMBIE_FRAME_H = 55
private const val ZOMBIE_COLS = 8
private const val ZOMBIE_FPS = 10

// Rat: 48x48 per frame — idle 8cols, run 6cols
private const val RAT_FRAME_W = 48
private const val RAT_FRAME_H = 48
private const val RAT_IDLE_COLS = 8
private const val RAT_RUN_COLS = 6
private const val RAT_FPS = 12

// Wolf: 80x48 per frame — idle 8cols, run 6cols
private const val WOLF_FRAME_W = 80
private const val WOLF_FRAME_H = 48
private const val WOLF_IDLE_COLS = 8
private const val WOLF_RUN_COLS = 6
private const val WOLF_FPS = 12

class GameplayScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    private val assetBase get() = "$filesResourcesPath/zombusters"

    override suspend fun SContainer.sceneInit() {
        val levelDef = if (filesResourcesPath.isNotEmpty()) {
            try { LevelParser(filesResourcesPath).loadLevel(1) } catch (_: Exception) { null }
        } else null

        if (levelDef == null) {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x22, 0x22, 0x22, 0xFF))
            text("Failed to load Level 1").also {
                it.textSize = 32.0; it.color = Colors.RED
                it.x = 400.0; it.y = 340.0
            }
            return
        }

        val world = GameplayWorld(levelDef)

        // Load bitmaps
        val mapBitmap = tryLoadBitmap("$assetBase/levels/level01/map.png")
        val jadeIdleBitmap = tryLoadBitmap("$assetBase/characters/jade/idle.png")
        val furnitureBitmaps = loadFurnitureBitmaps()
        val zombieBitmap = tryLoadBitmap("$assetBase/enemies/zombie/walk1.png")
        val ratIdleBitmap = tryLoadBitmap("$assetBase/enemies/rat/idle.png")
        val ratRunBitmap = tryLoadBitmap("$assetBase/enemies/rat/run.png")
        val wolfIdleBitmap = tryLoadBitmap("$assetBase/enemies/wolf/idle.png")
        val wolfRunBitmap = tryLoadBitmap("$assetBase/enemies/wolf/run.png")

        // Build animations
        val zombieAnim = zombieBitmap?.let {
            SpriteAnimation(it, ZOMBIE_FRAME_W, ZOMBIE_FRAME_H, ZOMBIE_COLS, 1)
        }
        val ratIdleAnim = ratIdleBitmap?.let {
            SpriteAnimation(it, RAT_FRAME_W, RAT_FRAME_H, RAT_IDLE_COLS, 1)
        }
        val ratRunAnim = ratRunBitmap?.let {
            SpriteAnimation(it, RAT_FRAME_W, RAT_FRAME_H, RAT_RUN_COLS, 1)
        }
        val wolfIdleAnim = wolfIdleBitmap?.let {
            SpriteAnimation(it, WOLF_FRAME_W, WOLF_FRAME_H, WOLF_IDLE_COLS, 1)
        }
        val wolfRunAnim = wolfRunBitmap?.let {
            SpriteAnimation(it, WOLF_FRAME_W, WOLF_FRAME_H, WOLF_RUN_COLS, 1)
        }

        // Layer 0: Map background
        if (mapBitmap != null) {
            image(mapBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x22, 0x22, 0x22, 0xFF))
        }

        // Layer 1-n: Furniture with depth sorting
        buildFurnitureViews(world.furnitures, furnitureBitmaps)

        // Layer: Player
        val playerContainer = container {
            zIndex = world.player1.position.y
        }
        if (jadeIdleBitmap != null) {
            val idleAnim = SpriteAnimation(jadeIdleBitmap, JADE_IDLE_FRAME_W, JADE_IDLE_FRAME_H, JADE_IDLE_COLS, 1)
            val playerSprite = playerContainer.sprite(idleAnim) { smoothing = false }
            playerSprite.playAnimationLooped(idleAnim, (1.0 / JADE_IDLE_FPS).seconds)
        } else {
            playerContainer.solidRect(20.0, 20.0, Colors.CYAN).also { it.x = -10.0; it.y = -10.0 }
        }

        // Bullet rendering layer — updated each frame
        val bulletGraphics = cpuGraphics { }
        bulletGraphics.zIndex = 900.0

        // HUD
        val hudHealth = text("HP: 100") {
            textSize = 14.0; color = Colors.LIME; x = 8.0; y = 4.0; zIndex = 1000.0
        }
        val hudLives = text("Lives: 3") {
            textSize = 14.0; color = Colors.WHITE; x = 90.0; y = 4.0; zIndex = 1000.0
        }
        val hudScore = text("Score: 0") {
            textSize = 14.0; color = Colors.YELLOW; x = 170.0; y = 4.0; zIndex = 1000.0
        }
        val hudWeapon = text("PISTOL (INF)") {
            textSize = 14.0; color = Colors.ORANGE; x = 270.0; y = 4.0; zIndex = 1000.0
        }
        val hudWave = text("Wave 1") {
            textSize = 14.0; color = Colors.LIGHTGRAY; x = 430.0; y = 4.0; zIndex = 1000.0
        }
        val hudEnemies = text("Enemies: 0") {
            textSize = 14.0; color = Colors.LIGHTGRAY; x = 530.0; y = 4.0; zIndex = 1000.0
        }

        // Pause overlay
        val pauseOverlay = solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0, 0, 0, 160)).apply {
            zIndex = 1100.0; visible = false
        }
        val pauseLabel = text("PAUSED") {
            textSize = 48.0; color = Colors.WHITE
            x = GAME_WIDTH.toDouble() / 2 - 80.0; y = GAME_HEIGHT.toDouble() / 2 - 30.0
            zIndex = 1101.0; visible = false
        }

        // Game over overlay
        val gameOverOverlay = solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0, 0, 0, 200)).apply {
            zIndex = 1200.0; visible = false
        }
        val gameOverLabel = text("GAME OVER") {
            textSize = 64.0; color = Colors.RED
            x = GAME_WIDTH.toDouble() / 2 - 140.0; y = GAME_HEIGHT.toDouble() / 2 - 40.0
            zIndex = 1201.0; visible = false
        }
        val gameOverSub = text("R - Restart Level   ESC - Main Menu") {
            textSize = 20.0; color = Colors.WHITE
            x = GAME_WIDTH.toDouble() / 2 - 180.0; y = GAME_HEIGHT.toDouble() / 2 + 40.0
            zIndex = 1201.0; visible = false
        }

        // Stage cleared overlay
        val stageClearedLabel = text("STAGE CLEARED!") {
            textSize = 56.0; color = Colors.YELLOW
            x = GAME_WIDTH.toDouble() / 2 - 200.0; y = GAME_HEIGHT.toDouble() / 2 - 40.0
            zIndex = 1200.0; visible = false
        }

        // Enemy view pool
        val enemyViews = mutableListOf<Pair<BaseEnemy, korlibs.korge.view.Container>>()

        fun spawnEnemyViews(newEnemies: List<BaseEnemy>) {
            for (enemy in newEnemies) {
                val c = container { zIndex = enemy.entity.position.y.toDouble() }
                when (enemy.type) {
                    EnemyType.ZOMBIE -> {
                        val anim = zombieAnim
                        if (anim != null) {
                            val s = c.sprite(anim) { smoothing = false }
                            s.playAnimationLooped(anim, (1.0 / ZOMBIE_FPS).seconds)
                        } else {
                            c.solidRect(20.0, 20.0, Colors.RED).also { it.x = -10.0; it.y = -10.0 }
                        }
                    }
                    EnemyType.RAT -> {
                        val anim = ratRunAnim ?: ratIdleAnim
                        if (anim != null) {
                            val s = c.sprite(anim) { smoothing = false }
                            s.playAnimationLooped(anim, (1.0 / RAT_FPS).seconds)
                        } else {
                            c.solidRect(16.0, 16.0, Colors.YELLOW).also { it.x = -8.0; it.y = -8.0 }
                        }
                    }
                    EnemyType.WOLF -> {
                        val anim = wolfRunAnim ?: wolfIdleAnim
                        if (anim != null) {
                            val s = c.sprite(anim) { smoothing = false }
                            s.playAnimationLooped(anim, (1.0 / WOLF_FPS).seconds)
                        } else {
                            c.solidRect(24.0, 24.0, Colors.MAGENTA).also { it.x = -12.0; it.y = -12.0 }
                        }
                    }
                    else -> {
                        c.solidRect(20.0, 20.0, Colors.ORANGE).also { it.x = -10.0; it.y = -10.0 }
                    }
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
                    circle(Point(f.obstaclePositionX.toDouble(), f.obstaclePositionY.toDouble()),
                        f.obstacleRadius.toDouble())
                }
            }
            for (z in world.spawnZones) {
                stroke(Colors.GREEN, lineWidth = 1.0) {
                    rect(z.originX.toDouble(), z.originY.toDouble(),
                        (z.endX - z.originX).toDouble(), (z.endY - z.originY).toDouble())
                }
            }
        }
        debugOverlay.zIndex = 999.0
        debugOverlay.visible = false

        // Music
        if (filesResourcesPath.isNotEmpty()) {
            try {
                val music = resourcesVfs["$filesResourcesPath/music/BradSucks_BadAttraction.ogg"].readMusic()
                music.playNoCancelForever()
            } catch (_: Exception) {}
        }

        var totalSeconds = 0f
        var isPaused = false
        var gameOver = false
        // Default facing North; updated whenever the player moves
        var lastFireAngle = FacingDirection.angleFrom(0f, -1f)
        var lastMoveDx = 0f
        var lastMoveDy = -1f

        val capturedViews = views
        val sceneScope = this@GameplayScene

        addUpdater { dt: Duration ->
            val ks = capturedViews.input.keys

            // ESC: pause toggle or exit game-over → main menu
            if (ks.justPressed(Key.ESCAPE)) {
                if (gameOver) {
                    sceneScope.launch {
                        sceneContainer.changeTo {
                            MainMenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                        }
                    }
                    return@addUpdater
                }
                isPaused = !isPaused
                pauseOverlay.visible = isPaused
                pauseLabel.visible = isPaused
            }

            // R: restart Level 1 after game over
            if (gameOver && ks.justPressed(Key.R)) {
                sceneScope.launch {
                    sceneContainer.changeTo {
                        GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                    }
                }
                return@addUpdater
            }

            if (isPaused || gameOver) return@addUpdater

            val dtSec = dt.inWholeMilliseconds / 1000f
            totalSeconds += dtSec

            // Debug + cheat keys
            if (ks.justPressed(Key.F1)) {
                debugMode = !debugMode
                debugOverlay.visible = debugMode
            }
            if (debugMode) {
                if (ks.justPressed(Key.K)) {
                    world.enemySystem.enemies.forEach { if (it.isActive) it.crash(totalSeconds) }
                }
                if (ks.justPressed(Key.G)) {
                    world.player1.ammo[GunType.MACHINEGUN.id] = 50
                    world.player1.ammo[GunType.SHOTGUN.id] = 25
                    world.player1.ammo[GunType.FLAMETHROWER.id] = 25
                    world.player1.ammo[GunType.GRENADE.id] = 5
                }
                if (ks.justPressed(Key.H)) {
                    world.player1.lifecounter -= 20
                    if (world.player1.lifecounter <= 0) {
                        world.player1.lives--
                        world.player1.lifecounter = 100
                        world.player1.destroy(totalSeconds)
                    }
                }
            }

            // Weapon cycle (TAB)
            if (ks.justPressed(Key.TAB)) world.player1.cycleWeapon()

            // Player movement (only when alive/immune)
            val playerCanAct = world.player1.status == ObjectStatus.ACTIVE ||
                               world.player1.status == ObjectStatus.IMMUNE
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
                world.player1.position = CollisionSystem.resolveMovement(
                    world.player1.position, delta, world.walls, world.furnitures
                )
            }

            // Fire (SPACE) — only when ACTIVE, not IMMUNE
            if (world.player1.status == ObjectStatus.ACTIVE && ks[Key.SPACE]) {
                CombatSystem.tryFire(world.player1, world.combatState, totalSeconds, lastFireAngle)
            }

            // Prune out-of-bounds projectiles
            CombatSystem.pruneOutOfBounds(world.combatState, totalSeconds)

            // Bullet-enemy collision + scoring
            val kills = DamageSystem.processBulletCollisions(
                world.combatState, world.enemySystem.enemies, world.player1, totalSeconds
            )

            // Power-up spawn on kills
            repeat(kills) {
                world.powerUpSystem.trySpawnOnKill(
                    world.player1.position.x.toFloat(),
                    world.player1.position.y.toFloat(),
                    totalSeconds
                )
            }

            // Enemy contact damage (only when ACTIVE — IMMUNE players are safe)
            if (world.player1.status == ObjectStatus.ACTIVE) {
                DamageSystem.processEnemyContact(world.enemySystem.enemies, world.player1, totalSeconds)
            }

            // Power-up update + pickup
            world.powerUpSystem.update(totalSeconds, world.player1)

            // Wave / spawn logic
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
                playerSteering.velocity = Vec2(
                    lastMoveDx * AVATAR_PIXELS_PER_SECOND / 60f,
                    lastMoveDy * AVATAR_PIXELS_PER_SECOND / 60f
                )
                world.enemySystem.update(dtSec, playerPos, playerSteering, totalSeconds)

                if (waveSystem.spawned && world.enemySystem.activeCount == 0) {
                    waveSystem.advanceWave()
                }
            }

            // Avatar lifecycle transitions (DYING→IMMUNE→ACTIVE)
            world.player1.update(totalSeconds)

            // Check game over
            if (world.player1.status == ObjectStatus.INACTIVE && world.player1.lives <= 0) {
                gameOver = true
                gameOverOverlay.visible = true
                gameOverLabel.visible = true
                gameOverSub.visible = true
            }

            // Stage cleared
            if (waveSystem.isLevelComplete) {
                stageClearedLabel.visible = true
            }

            // Sync enemy views
            for ((enemy, view) in enemyViews) {
                if (enemy.status == ObjectStatus.INACTIVE) {
                    view.visible = false
                    continue
                }
                if (enemy.status == ObjectStatus.DYING) {
                    // Show death briefly (tint red), then hide once INACTIVE
                    view.visible = true
                    view.colorMul = korlibs.image.color.Colors.RED
                } else {
                    view.colorMul = korlibs.image.color.Colors.WHITE
                }
                view.visible = true
                val ex = enemy.entity.position.x.toDouble()
                val ey = enemy.entity.position.y.toDouble()
                val yOffset = when (enemy.type) {
                    EnemyType.ZOMBIE -> 50.0
                    EnemyType.RAT    -> Rat.Y_OFFSET.toDouble()
                    EnemyType.WOLF   -> Wolf.Y_OFFSET.toDouble()
                    else             -> 50.0
                }
                view.x = ex
                view.y = ey - yOffset
                view.zIndex = ey
                if (enemy.entity.velocity.x != 0f) {
                    view.scaleX = if (enemy.entity.velocity.x > 0) 1.0 else -1.0
                }
            }

            // Player position + IMMUNE blink (10 Hz toggle)
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

            // Bullet rendering
            bulletGraphics.updateShape {
                for (bullet in world.combatState.bullets) {
                    val (bx, by) = bullet.positionAt(totalSeconds)
                    fill(Colors.YELLOW) {
                        circle(Point(bx.toDouble(), by.toDouble()), 3.0)
                    }
                }
                for (shell in world.combatState.shotgunShells) {
                    for (pi in 0..2) {
                        val (px, py) = shell.pelletPositionAt(pi, totalSeconds)
                        fill(Colors.ORANGE) {
                            circle(Point(px.toDouble(), py.toDouble()), 2.5)
                        }
                    }
                }
            }

            // HUD updates
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
                else                           -> Colors.RED
            }
            hudLives.text = "Lives: ${world.player1.lives}"
            hudScore.text = "Score: ${world.player1.score}"
            hudWeapon.text = gunLabel
            hudWave.text = if (waveSystem.isLevelComplete) "CLEARED"
                           else "Wave ${waveSystem.waveNumber}/${waveSystem.totalWaves}"
            hudEnemies.text = "Enemies: ${world.enemySystem.activeCount}"
        }

        // Initial position
        playerContainer.x = world.player1.position.x + PLAYER_RENDER_OFFSET_X
        playerContainer.y = world.player1.position.y + PLAYER_RENDER_OFFSET_Y
    }

    private suspend fun tryLoadBitmap(path: String): Bitmap? = try {
        resourcesVfs[path].readBitmap()
    } catch (_: Exception) { null }

    private suspend fun loadFurnitureBitmaps(): Map<Pair<FurnitureType, FurnitureOrientation?>, Bitmap?> {
        val base = "$assetBase/furniture"
        return mapOf(
            Pair(FurnitureType.ARBOL, null)                         to tryLoadBitmap("$base/arbol.png"),
            Pair(FurnitureType.BASURA, null)                        to tryLoadBitmap("$base/basura.png"),
            Pair(FurnitureType.COCHE_ARDIENDO, null)                to tryLoadBitmap("$base/coche_ardiendo.png"),
            Pair(FurnitureType.PUENTE, null)                        to tryLoadBitmap("$base/puente.png"),
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

    private fun SContainer.buildFurnitureViews(
        furnitures: List<Furniture>,
        bitmaps: Map<Pair<FurnitureType, FurnitureOrientation?>, Bitmap?>
    ) {
        for (f in furnitures) {
            val bmp = bitmaps[Pair(f.type, f.orientation)] ?: bitmaps[Pair(f.type, null)]
            if (bmp != null) {
                image(bmp) {
                    x = f.positionX.toDouble()
                    y = f.positionY.toDouble()
                    zIndex = (f.positionY + bmp.height).toDouble()
                    smoothing = false
                }
            }
        }
    }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
