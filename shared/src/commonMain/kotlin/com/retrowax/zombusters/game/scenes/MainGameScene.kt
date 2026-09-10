package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.world.LevelParser
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.korge.scene.Scene
import korlibs.korge.view.SContainer
import korlibs.korge.view.solidRect
import korlibs.korge.view.text
import korlibs.math.geom.Size

class MainGameScene(
    private val buttonBackgroundColor: RGBA = Colors.DARKRED,
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    override suspend fun SContainer.sceneInit() {
        solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), Colors.BLACK)

        text("ZOMBUSTERS").also {
            it.textSize = 72.0
            it.color = Colors.RED
            it.x = (GAME_WIDTH / 2 - 220).toDouble()
            it.y = (GAME_HEIGHT / 2 - 50).toDouble()
        }

        text("[ Migration in progress — Phase 1 ]").also {
            it.textSize = 28.0
            it.color = Colors.LIGHTGRAY
            it.x = (GAME_WIDTH / 2 - 220).toDouble()
            it.y = (GAME_HEIGHT / 2 + 50).toDouble()
        }

        // Pre-load level data to verify parser works
        if (filesResourcesPath.isNotEmpty()) {
            try {
                val parser = LevelParser(filesResourcesPath)
                val level1 = parser.loadLevel(1)
                level1?.let {
                    text("Level 1 loaded: P1 spawn (${it.p1SpawnX.toInt()},${it.p1SpawnY.toInt()})").also { t ->
                        t.textSize = 18.0
                        t.color = Colors.GREEN
                        t.x = (GAME_WIDTH / 2 - 220).toDouble()
                        t.y = (GAME_HEIGHT / 2 + 120).toDouble()
                    }
                }
            } catch (e: Exception) {
                text("Level load: ${e.message?.take(60)}").also { t ->
                    t.textSize = 16.0
                    t.color = Colors.YELLOW
                    t.x = 10.0
                    t.y = (GAME_HEIGHT / 2 + 120).toDouble()
                }
            }
        }
    }

    override fun onSizeChanged(size: Size) {
        super.onSizeChanged(size)
    }
}
