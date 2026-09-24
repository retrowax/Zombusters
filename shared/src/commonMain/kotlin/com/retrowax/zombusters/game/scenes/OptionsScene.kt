package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
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
 * Options screen — legacy OptionsScreen.cs.
 *
 * Options from legacy OptionsScreen.cs:
 *   0 → Sound FX Volume  (slider, 0-10 units)
 *   1 → Music Volume     (slider, 0-10 units)
 *   2 → Language
 *   3 → Full Screen
 *   4 → Save and Exit
 *
 * Step 6 implementation: FX Volume + Music Volume are displayed.
 * Language/Fullscreen stubs shown. Save navigates back to MenuScene.
 * Settings persistence via multiplatform-settings is deferred.
 */
class OptionsScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    private val optionLabels = listOf(
        "SOUND FX VOLUME",
        "MUSIC VOLUME",
        "LANGUAGE",
        "FULLSCREEN",
        "SAVE AND EXIT"
    )
    private val languages = listOf("EN", "DE", "ES", "FR", "IT")
    private var selectedIndex = 0
    private var fxVolume = GameSettings.fxVolume
    private var musicVolume = GameSettings.musicVolume
    private var langIndex = languages.indexOfFirst { it.lowercase() == GameSettings.language }.coerceAtLeast(0)
    private var fullscreen = GameSettings.fullscreen

    override suspend fun SContainer.sceneInit() {
        ZombustersFonts.loadFrom(fontResourcesPath)

        val assetBase = "$filesResourcesPath/zombusters/menu"
        val bgBitmap = tryLoad("$assetBase/background_title.png")
        if (bgBitmap != null) {
            image(bgBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x11, 0x11, 0x22, 0xFF))
        }

        text("OPTIONS") {
            textSize = 40.0; color = Colors.WHITE
            x = 128.0; y = 180.0; zIndex = 2.0
            font = ZombustersFonts.menuHeader
        }

        val menuStartY = 280.0
        val menuSpacing = 60.0

        val labelViews = optionLabels.mapIndexed { i, label ->
            text(label) {
                textSize = 28.0
                x = 128.0; y = menuStartY + i * menuSpacing
                zIndex = 2.0
                font = ZombustersFonts.menuList
            }
        }

        // Value displays for options
        val fxValueView = text("") {
            textSize = 28.0; color = Colors.CYAN
            x = 500.0; y = menuStartY; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }
        val musicValueView = text("") {
            textSize = 28.0; color = Colors.CYAN
            x = 500.0; y = menuStartY + menuSpacing; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }
        val langValueView = text("") {
            textSize = 28.0; color = Colors.CYAN
            x = 500.0; y = menuStartY + 2 * menuSpacing; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }
        val fullscreenValueView = text("") {
            textSize = 28.0; color = Colors.CYAN
            x = 500.0; y = menuStartY + 3 * menuSpacing; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }

        text("← / → to adjust values   ESC — Back") {
            textSize = 18.0; color = Colors.WHITE
            x = 128.0; y = GAME_HEIGHT - 50.0; zIndex = 2.0
            font = ZombustersFonts.menuInfo
        }

        fun renderBars(volume: Int): String = "█".repeat(volume) + "░".repeat(10 - volume)

        fun refresh() {
            labelViews.forEachIndexed { i, v ->
                v.color = if (i == selectedIndex) Colors.YELLOW else Colors.WHITE
            }
            fxValueView.text = renderBars(fxVolume)
            musicValueView.text = renderBars(musicVolume)
            langValueView.text = "< ${languages[langIndex]} >"
            fullscreenValueView.text = if (fullscreen) "ON" else "OFF"
        }
        refresh()

        var prevSelected = selectedIndex
        var done = false
        val capturedViews = views
        val sceneScope = this@OptionsScene

        // Touch regions:
        //   Option rows (x 50-900, y row-15 to row+45): select row; for volume rows x<500=decrease, x>=500=increase
        //   SAVE AND EXIT row (i=4): navigate back
        //   Bottom strip (y >= GAME_HEIGHT - 80): back to menu
        val mSY = menuStartY
        val mSp = menuSpacing
        touch {
            end { info ->
                if (done) return@end
                val tx = info.local.x; val ty = info.local.y
                // Bottom back strip
                if (ty >= GAME_HEIGHT - 80.0) {
                    done = true
                    sceneScope.launch { sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }; done = false }
                    return@end
                }
                for (i in optionLabels.indices) {
                    val ry = mSY + i * mSp
                    if (tx >= 50.0 && tx <= 900.0 && ty >= ry - 15 && ty <= ry + 45) {
                        selectedIndex = i
                        when (i) {
                            0 -> if (tx >= 500.0) fxVolume = (fxVolume + 1).coerceAtMost(10)
                                 else fxVolume = (fxVolume - 1).coerceAtLeast(0)
                            1 -> if (tx >= 500.0) musicVolume = (musicVolume + 1).coerceAtMost(10)
                                 else musicVolume = (musicVolume - 1).coerceAtLeast(0)
                            4 -> { done = true; sceneScope.launch { GameSettings.fxVolume = fxVolume; GameSettings.musicVolume = musicVolume; GameSettings.language = languages[langIndex].lowercase(); GameSettings.fullscreen = fullscreen; sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }; done = false } }
                        }
                        break
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
                if (ty >= GAME_HEIGHT - 80.0) {
                    done = true
                    sceneScope.launch { sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }; done = false }
                } else {
                    for (i in optionLabels.indices) {
                        val ry = menuStartY + i * menuSpacing
                        if (tx >= 50.0 && tx <= 900.0 && ty >= ry - 15 && ty <= ry + 45) {
                            selectedIndex = i
                            when (i) {
                                0 -> if (tx >= 500.0) fxVolume = (fxVolume + 1).coerceAtMost(10)
                                     else fxVolume = (fxVolume - 1).coerceAtLeast(0)
                                1 -> if (tx >= 500.0) musicVolume = (musicVolume + 1).coerceAtMost(10)
                                     else musicVolume = (musicVolume - 1).coerceAtLeast(0)
                                4 -> { done = true; sceneScope.launch { GameSettings.fxVolume = fxVolume; GameSettings.musicVolume = musicVolume; GameSettings.language = languages[langIndex].lowercase(); GameSettings.fullscreen = fullscreen; sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }; done = false } }
                            }
                            break
                        }
                    }
                }
            }
            prevMouseDown = mouseDown

            if (ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                selectedIndex = (selectedIndex - 1 + optionLabels.size) % optionLabels.size
            }
            if (ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                selectedIndex = (selectedIndex + 1) % optionLabels.size
            }

            when (selectedIndex) {
                0 -> {
                    if (ks.justPressed(Key.RIGHT) || ks.justPressed(Key.D)) fxVolume = (fxVolume + 1).coerceAtMost(10)
                    if (ks.justPressed(Key.LEFT)  || ks.justPressed(Key.A)) fxVolume = (fxVolume - 1).coerceAtLeast(0)
                }
                1 -> {
                    if (ks.justPressed(Key.RIGHT) || ks.justPressed(Key.D)) musicVolume = (musicVolume + 1).coerceAtMost(10)
                    if (ks.justPressed(Key.LEFT)  || ks.justPressed(Key.A)) musicVolume = (musicVolume - 1).coerceAtLeast(0)
                }
                2 -> {
                    if (ks.justPressed(Key.RIGHT) || ks.justPressed(Key.D)) langIndex = (langIndex + 1) % languages.size
                    if (ks.justPressed(Key.LEFT)  || ks.justPressed(Key.A)) langIndex = (langIndex - 1 + languages.size) % languages.size
                }
                3 -> {
                    if (ks.justPressed(Key.RIGHT) || ks.justPressed(Key.D) || ks.justPressed(Key.LEFT) || ks.justPressed(Key.A)) fullscreen = !fullscreen
                    if (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) fullscreen = !fullscreen
                }
            }

            if (prevSelected != selectedIndex) {
                prevSelected = selectedIndex
            }
            refresh()

            if (ks.justPressed(Key.ESCAPE)) {
                sceneScope.launch {
                    sceneContainer.changeTo {
                        MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                    }
                }
            }
            if (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                when (selectedIndex) {
                    3 -> fullscreen = !fullscreen
                    4 -> {
                        sceneScope.launch {
                            GameSettings.fxVolume = fxVolume; GameSettings.musicVolume = musicVolume; GameSettings.language = languages[langIndex].lowercase(); GameSettings.fullscreen = fullscreen;
                            sceneContainer.changeTo {
                                MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                        }
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
