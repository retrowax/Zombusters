package com.retrowax.zombusters.game.scenes

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
import com.retrowax.zombusters.game.model.ObjectStatus
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

        // HUD overlay
        val hudText = text("Level 1  [ESC=Menu  F1=Debug  K=Kill wave]") {
            textSize = 14.0; color = Colors.LIGHTGRAY; x = 8.0; y = 8.0; zIndex = 1000.0
        }

        // Enemy view pool — container per enemy, populated as enemies are spawned
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

        // Debug overlay (F1 — off by default)
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

        // AI debug overlay (rebuilt each frame when active)
        var aiDebugMode = false

        // Music
        if (filesResourcesPath.isNotEmpty()) {
            try {
                val music = resourcesVfs["$filesResourcesPath/music/BradSucks_BadAttraction.ogg"].readMusic()
                music.playNoCancelForever()
            } catch (_: Exception) {}
        }

        var totalSeconds = 0f
        val capturedViews = views
        val sceneScope = this@GameplayScene

        addUpdater { dt: Duration ->
            val dtSec = dt.inWholeMilliseconds / 1000f
            totalSeconds += dtSec

            val ks = capturedViews.input.keys

            if (ks.justPressed(Key.ESCAPE)) {
                sceneScope.launch {
                    sceneContainer.changeTo {
                        MainMenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                    }
                }
                return@addUpdater
            }

            if (ks.justPressed(Key.F1)) {
                debugMode = !debugMode
                debugOverlay.visible = debugMode
                aiDebugMode = debugMode
            }

            // K = kill all active enemies (skip wave for testing)
            if (ks.justPressed(Key.K)) {
                world.enemySystem.enemies.forEach { if (it.isActive) it.status = ObjectStatus.INACTIVE }
            }

            // Player movement
            var dx = 0f; var dy = 0f
            if (ks[Key.W] || ks[Key.UP]) dy -= 1f
            if (ks[Key.S] || ks[Key.DOWN]) dy += 1f
            if (ks[Key.A] || ks[Key.LEFT]) dx -= 1f
            if (ks[Key.D] || ks[Key.RIGHT]) dx += 1f

            if (dx != 0f || dy != 0f) {
                val len = sqrt(dx * dx + dy * dy)
                dx /= len; dy /= len
                val speed = AVATAR_PIXELS_PER_SECOND * dtSec
                val delta = Point(dx.toDouble() * speed, dy.toDouble() * speed)
                world.player1.position = CollisionSystem.resolveMovement(
                    world.player1.position, delta, world.walls, world.furnitures
                )
            }

            // Player container position + zIndex
            playerContainer.x = world.player1.position.x + PLAYER_RENDER_OFFSET_X
            playerContainer.y = world.player1.position.y + PLAYER_RENDER_OFFSET_Y
            playerContainer.zIndex = world.player1.position.y

            // Wave/spawn logic
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

                // Build player SteeringEntity snapshot for enemy AI
                val playerPos = Vec2(world.player1.position.x.toFloat(), world.player1.position.y.toFloat())
                val playerSteering = SteeringEntity(playerPos)
                // approximate player velocity from last frame movement
                playerSteering.velocity = Vec2(
                    (dx * AVATAR_PIXELS_PER_SECOND / 60f),
                    (dy * AVATAR_PIXELS_PER_SECOND / 60f)
                )

                world.enemySystem.update(dtSec, playerPos, playerSteering)

                // Advance wave when all active enemies gone
                if (waveSystem.spawned && world.enemySystem.activeCount == 0) {
                    waveSystem.advanceWave()
                }
            }

            // Sync enemy views to domain positions
            for ((enemy, view) in enemyViews) {
                if (enemy.status != ObjectStatus.ACTIVE) {
                    view.visible = false
                    continue
                }
                view.visible = true
                val ex = enemy.entity.position.x.toDouble()
                val ey = enemy.entity.position.y.toDouble()
                val yOffset = when (enemy.type) {
                    EnemyType.ZOMBIE -> 50.0
                    EnemyType.RAT -> Rat.Y_OFFSET.toDouble()
                    EnemyType.WOLF -> Wolf.Y_OFFSET.toDouble()
                    else -> 50.0
                }
                view.x = ex
                view.y = ey - yOffset
                view.zIndex = ey

                // Flip sprite based on horizontal velocity
                if (enemy.entity.velocity.x != 0f) {
                    view.scaleX = if (enemy.entity.velocity.x > 0) 1.0 else -1.0
                }
            }

            // HUD
            val contactCount = world.enemySystem.enemiesInContactRange(
                Vec2(world.player1.position.x.toFloat(), world.player1.position.y.toFloat())
            ).size
            val waveInfo = if (waveSystem.isLevelComplete) "CLEARED" else "Wave ${waveSystem.waveNumber}/${waveSystem.totalWaves}"
            if (debugMode) {
                hudText.text = "$waveInfo  Enemies:${world.enemySystem.activeCount}  Contact:$contactCount  " +
                    "P:(${world.player1.position.x.toInt()},${world.player1.position.y.toInt()})  [F1=Debug OFF  K=Kill]"
            } else {
                hudText.text = "$waveInfo  Enemies:${world.enemySystem.activeCount}  [ESC=Menu  F1=Debug  K=Kill]"
            }

            world.player1.update(totalSeconds)
        }

        // Initial player position
        playerContainer.x = world.player1.position.x + PLAYER_RENDER_OFFSET_X
        playerContainer.y = world.player1.position.y + PLAYER_RENDER_OFFSET_Y
    }

    private suspend fun tryLoadBitmap(path: String): Bitmap? = try {
        resourcesVfs[path].readBitmap()
    } catch (_: Exception) { null }

    private suspend fun loadFurnitureBitmaps(): Map<Pair<FurnitureType, FurnitureOrientation?>, Bitmap?> {
        val base = "$assetBase/furniture"
        return mapOf(
            Pair(FurnitureType.ARBOL, null) to tryLoadBitmap("$base/arbol.png"),
            Pair(FurnitureType.BASURA, null) to tryLoadBitmap("$base/basura.png"),
            Pair(FurnitureType.COCHE_ARDIENDO, null) to tryLoadBitmap("$base/coche_ardiendo.png"),
            Pair(FurnitureType.PUENTE, null) to tryLoadBitmap("$base/puente.png"),
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
