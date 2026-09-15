package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.engine.CollisionSystem
import com.retrowax.zombusters.game.model.AVATAR_PIXELS_PER_SECOND
import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.model.FurnitureOrientation
import com.retrowax.zombusters.game.model.FurnitureType
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

// From legacy GamePlayScreen.cs: Vector2 offsetPosition = new Vector2(-20, -55)
private const val PLAYER_RENDER_OFFSET_X = -20.0
private const val PLAYER_RENDER_OFFSET_Y = -55.0

// Jade idle animation metadata from AnimationDef.xml
private const val JADE_IDLE_FRAME_W = 42
private const val JADE_IDLE_FRAME_H = 55
private const val JADE_IDLE_COLS = 11
private const val JADE_IDLE_FPS = 15

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

        // Load all needed bitmaps at scene start
        val mapBitmap = tryLoadBitmap("$assetBase/levels/level01/map.png")
        val jadeIdleBitmap = tryLoadBitmap("$assetBase/characters/jade/idle.png")
        val furnitureBitmaps = loadFurnitureBitmaps()

        // Layer 0: Map background
        if (mapBitmap != null) {
            image(mapBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x22, 0x22, 0x22, 0xFF))
        }

        // Layer 1-n: Furniture with depth sorting
        buildFurnitureViews(world.furnitures, furnitureBitmaps)

        // Layer: Player (zIndex updated each frame based on position.Y)
        val playerContainer = container {
            zIndex = world.player1.position.y
        }

        if (jadeIdleBitmap != null) {
            val idleAnim = SpriteAnimation(
                spriteMap = jadeIdleBitmap,
                spriteWidth = JADE_IDLE_FRAME_W,
                spriteHeight = JADE_IDLE_FRAME_H,
                columns = JADE_IDLE_COLS,
                rows = 1
            )
            val playerSprite = playerContainer.sprite(idleAnim) { smoothing = false }
            playerSprite.playAnimationLooped(idleAnim, (1.0 / JADE_IDLE_FPS).seconds)
        } else {
            // Debug placeholder when sprite unavailable
            playerContainer.solidRect(20.0, 20.0, Colors.CYAN).also {
                it.x = -10.0; it.y = -10.0
            }
            playerContainer.text("P1") {
                textSize = 14.0; color = Colors.WHITE; x = -8.0; y = -8.0
            }
        }

        // HUD overlay (always on top)
        val hudText = text("Level 1  [ESC=Menu  F1=Debug]") {
            textSize = 14.0; color = Colors.LIGHTGRAY; x = 8.0; y = 8.0; zIndex = 1000.0
        }

        // Debug overlay (F1 toggle — off by default)
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
            }

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

            // Keep player container at gameplay position + render offset
            playerContainer.x = world.player1.position.x + PLAYER_RENDER_OFFSET_X
            playerContainer.y = world.player1.position.y + PLAYER_RENDER_OFFSET_Y
            playerContainer.zIndex = world.player1.position.y

            if (debugMode) {
                hudText.text = "Level 1  [ESC=Menu  F1=Debug OFF]  P:(${world.player1.position.x.toInt()},${world.player1.position.y.toInt()})"
            } else {
                hudText.text = "Level 1  [ESC=Menu  F1=Debug]"
            }

            world.player1.update(totalSeconds)
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
            val bmp = furnitureBitmapFor(f, bitmaps)
            if (bmp != null) {
                image(bmp) {
                    x = f.positionX.toDouble()
                    y = f.positionY.toDouble()
                    zIndex = (f.positionY + bmp.height).toDouble()
                    smoothing = false
                }
            }
            // Items without recovered assets render nothing in normal mode;
            // they still appear in debug overlay via the obstacle circles.
        }
    }

    private fun furnitureBitmapFor(
        f: Furniture,
        bitmaps: Map<Pair<FurnitureType, FurnitureOrientation?>, Bitmap?>
    ): Bitmap? =
        bitmaps[Pair(f.type, f.orientation)] ?: bitmaps[Pair(f.type, null)]

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
