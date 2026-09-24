package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
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
 * How To Play screen — legacy HowToPlayScreen.cs.
 *
 * Legacy displayed a controller diagram with labelled controls.
 * This port shows keyboard/mouse controls for current desktop target.
 * Controller references adapted to keyboard scheme.
 *
 * Controls described:
 *   Move:         WASD / Arrow keys
 *   Shoot:        SPACE (fires in last movement direction)
 *   Change weapon: TAB
 *   Pause:        ESC
 *   Debug overlay: F1
 */
class HowToPlayScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    override suspend fun SContainer.sceneInit() {
        ZombustersFonts.loadFrom(fontResourcesPath)

        val menuBase = "$filesResourcesPath/zombusters/menu"
        val bgBitmap    = tryLoad("$menuBase/background_title.png")
        val titleBitmap = tryLoad("$menuBase/title.png")

        if (bgBitmap != null) {
            image(bgBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x11, 0x11, 0x22, 0xFF))
        }
        if (titleBitmap != null) {
            image(titleBitmap) { x = 115.0; y = 20.0; width = 1000.0; height = 140.0; zIndex = 1.0; smoothing = true }
        }

        text("HOW TO PLAY") {
            textSize = 36.0; color = Colors.WHITE
            x = 128.0; y = 190.0; zIndex = 2.0
            font = ZombustersFonts.menuHeader
        }

        // Separator
        solidRect(GAME_WIDTH - 256.0, 2.0, Colors.WHITE).apply {
            x = 128.0; y = 240.0; zIndex = 1.9; alpha = 0.5
        }

        val controls = listOf(
            Pair("MOVE",          "WASD  /  Arrow keys"),
            Pair("SHOOT",         "SPACE  (fires in movement direction)"),
            Pair("CHANGE WEAPON", "TAB"),
            Pair("PAUSE",         "ESC"),
            Pair("DEBUG OVERLAY", "F1"),
            Pair("GIVE AMMO",     "G  (debug only)"),
            Pair("TAKE DAMAGE",   "H  (debug only)"),
            Pair("KILL WAVE",     "K  (debug only, requires F1)")
        )

        val startY = 265.0
        val spacing = 44.0
        controls.forEachIndexed { i, (action, desc) ->
            text(action) {
                textSize = 22.0; color = Colors.YELLOW
                x = 128.0; y = startY + i * spacing; zIndex = 2.0
                font = ZombustersFonts.menuList
            }
            text(desc) {
                textSize = 20.0; color = Colors.WHITE
                x = 380.0; y = startY + i * spacing; zIndex = 2.0
                font = ZombustersFonts.menuInfo
            }
        }

        text("ESC — Back") {
            textSize = 20.0; color = Colors.WHITE
            x = 128.0; y = GAME_HEIGHT - 50.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }

        var done = false
        val capturedViews = views
        val sceneScope = this@HowToPlayScene

        fun goBack() {
            if (!done) {
                done = true
                sceneScope.launch {
                    sceneContainer.changeTo {
                        ExtrasMenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                    }
                }
            }
        }

        touch { end { goBack() } }

        var prevMouseDown = false

        addUpdater { _: Duration ->
            val ks = capturedViews.input.keys
            if (!done && (ks.justPressed(Key.ESCAPE) || ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE))) {
                goBack()
            }
            val mouseDown = capturedViews.input.mouseButtonPressed(MouseButton.LEFT)
            if (!mouseDown && prevMouseDown) goBack()
            prevMouseDown = mouseDown
        }
    }

    private suspend fun tryLoad(path: String) = try {
        resourcesVfs[path].readBitmap()
    } catch (_: Exception) { null }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
