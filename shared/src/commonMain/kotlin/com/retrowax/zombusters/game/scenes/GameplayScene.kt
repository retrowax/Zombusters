package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.engine.CollisionSystem
import com.retrowax.zombusters.game.model.AVATAR_PIXELS_PER_SECOND
import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.world.GameplayWorld
import com.retrowax.zombusters.game.world.LevelParser
import korlibs.audio.sound.readMusic
import korlibs.event.Key
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.korge.scene.Scene
import korlibs.korge.view.SContainer
import korlibs.korge.view.addUpdater
import korlibs.korge.view.cpuGraphics
import korlibs.korge.view.solidRect
import korlibs.korge.view.text
import korlibs.io.file.std.resourcesVfs
import korlibs.math.geom.Point
import korlibs.math.geom.Size
import kotlinx.coroutines.launch
import kotlin.math.sqrt
import kotlin.time.Duration

class GameplayScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    override suspend fun SContainer.sceneInit() {
        solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x22, 0x22, 0x22, 0xFF))

        val levelDef = if (filesResourcesPath.isNotEmpty()) {
            try { LevelParser(filesResourcesPath).loadLevel(1) } catch (_: Exception) { null }
        } else null

        if (levelDef == null) {
            text("Failed to load Level 1").also {
                it.textSize = 32.0; it.color = Colors.RED
                it.x = 400.0; it.y = 340.0
            }
            return
        }

        val world = GameplayWorld(levelDef)

        // Debug geometry: walls as white lines, obstacles as orange outlines, spawn zones as green rects
        cpuGraphics {
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

        // Player placeholder: cyan rect + "P1" label at spawn position
        val playerView = solidRect(20.0, 20.0, Colors.CYAN).also {
            it.x = world.player1.position.x - 10.0
            it.y = world.player1.position.y - 10.0
        }
        val playerLabel = text("P1").also {
            it.textSize = 14.0; it.color = Colors.WHITE
            it.x = world.player1.position.x - 8.0
            it.y = world.player1.position.y - 8.0
        }

        text("Level 1  P1:(${levelDef.p1SpawnX.toInt()},${levelDef.p1SpawnY.toInt()})  [ESC=Menu]").also {
            it.textSize = 16.0; it.color = Colors.LIGHTGRAY
            it.x = 10.0; it.y = 10.0
        }

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

            if (capturedViews.input.keys.justPressed(Key.ESCAPE)) {
                sceneScope.launch {
                    sceneContainer.changeTo {
                        MainMenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                    }
                }
                return@addUpdater
            }

            var dx = 0f
            var dy = 0f
            val ks = capturedViews.input.keys
            if (ks[Key.W] || ks[Key.UP]) dy -= 1f
            if (ks[Key.S] || ks[Key.DOWN]) dy += 1f
            if (ks[Key.A] || ks[Key.LEFT]) dx -= 1f
            if (ks[Key.D] || ks[Key.RIGHT]) dx += 1f

            if (dx != 0f || dy != 0f) {
                val len = sqrt(dx * dx + dy * dy)
                dx /= len; dy /= len
                val speed = AVATAR_PIXELS_PER_SECOND * dtSec
                val delta = Point(dx.toDouble() * speed, dy.toDouble() * speed)
                val newPos = CollisionSystem.resolveMovement(
                    world.player1.position, delta, world.walls, world.furnitures
                )
                world.player1.position = newPos
                playerView.x = newPos.x - 10.0
                playerView.y = newPos.y - 10.0
                playerLabel.x = newPos.x - 8.0
                playerLabel.y = newPos.y - 8.0
            }

            world.player1.update(totalSeconds)
        }
    }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
