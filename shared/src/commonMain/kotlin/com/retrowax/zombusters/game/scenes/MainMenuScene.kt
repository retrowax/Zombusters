package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import korlibs.audio.sound.readMusic
import korlibs.event.Key
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.image.format.readBitmap
import korlibs.korge.input.mouse
import korlibs.korge.scene.Scene
import korlibs.korge.view.SContainer
import korlibs.korge.view.Text
import korlibs.korge.view.addUpdater
import korlibs.korge.view.image
import korlibs.korge.view.solidRect
import korlibs.korge.view.text
import korlibs.io.file.std.resourcesVfs
import korlibs.math.geom.Size
import kotlinx.coroutines.launch

class MainMenuScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    private var selectedIndex = 0
    private val menuItems = listOf("START GAME", "EXIT")
    private val assetBase get() = "$filesResourcesPath/zombusters"

    override suspend fun SContainer.sceneInit() {
        // Background: try to load the scrolling menu background
        val bgBitmap = if (filesResourcesPath.isNotEmpty()) {
            try { resourcesVfs["$assetBase/menu/background_title.png"].readBitmap() } catch (_: Exception) { null }
        } else null

        if (bgBitmap != null) {
            image(bgBitmap) {
                x = 0.0; y = 0.0
                // Crop the wider 1480px background to the 1280 game canvas by anchoring at left
                width = GAME_WIDTH.toDouble()
                height = GAME_HEIGHT.toDouble()
                smoothing = false
            }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x11, 0x11, 0x11, 0xFF))
        }

        // Logo: try the extracted title image (1000x323 zombusters title)
        val titleBitmap = if (filesResourcesPath.isNotEmpty()) {
            try { resourcesVfs["$assetBase/menu/title.png"].readBitmap() } catch (_: Exception) { null }
        } else null

        if (titleBitmap != null) {
            val scale = minOf(
                (GAME_WIDTH * 0.75).toDouble() / titleBitmap.width,
                180.0 / titleBitmap.height
            )
            image(titleBitmap) {
                this.scale = scale
                x = (GAME_WIDTH - titleBitmap.width * scale) / 2
                y = 80.0
                smoothing = true
            }
        } else {
            // Fallback: text title
            text("ZOMBUSTERS").also {
                it.textSize = 96.0
                it.color = Colors.RED
                it.x = (GAME_WIDTH / 2 - 270).toDouble()
                it.y = 120.0
            }
        }

        var menuViewsList: List<Text> = emptyList()

        fun refreshColors() {
            menuViewsList.forEachIndexed { i, v ->
                v.color = if (i == selectedIndex) Colors.YELLOW else Colors.WHITE
            }
        }

        val menuViews = menuItems.mapIndexed { i, label ->
            text(label).also { t ->
                t.textSize = 48.0
                t.x = (GAME_WIDTH / 2 - 150).toDouble()
                t.y = 380.0 + i * 80.0
                t.mouse {
                    onOver { selectedIndex = i; refreshColors() }
                    onClick { activateSelected() }
                }
            }
        }
        menuViewsList = menuViews
        refreshColors()

        val capturedViews = views
        addUpdater {
            val prevIndex = selectedIndex
            val ks = capturedViews.input.keys
            if (ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                selectedIndex = (selectedIndex - 1 + menuItems.size) % menuItems.size
            }
            if (ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                selectedIndex = (selectedIndex + 1) % menuItems.size
            }
            if (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                activateSelected()
            }
            if (prevIndex != selectedIndex) refreshColors()
        }

        if (filesResourcesPath.isNotEmpty()) {
            try {
                val music = resourcesVfs["$filesResourcesPath/music/NuitNoire_OpeningThePortal.ogg"].readMusic()
                music.playNoCancelForever()
            } catch (_: Exception) {}
        }
    }

    private fun activateSelected() {
        launch {
            when (selectedIndex) {
                0 -> sceneContainer.changeTo {
                    GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                }
                1 -> exit()
            }
        }
    }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
