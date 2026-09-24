package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.ui.ZombustersFonts
import korlibs.event.Key
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.image.format.readBitmap
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

private data class MenuEntry(val label: String)

/**
 * Main menu — legacy MenuScreen.cs.
 *
 * Menu entries from legacy MenuScreen.cs:
 *   0 → "NEW GAME"    (WPPlayNewGame → BeginSelectPlayerScreen)
 *   1 → "EXTRAS"      (ExtrasMenuString → DisplayExtrasMenu)
 *   2 → "OPTIONS"     (SettingsMenuString → DisplayOptions)
 *   3 → "QUIT"        (QuitGame → exit)
 *
 * Background: same scrolling parallax as StartScene (BackgroundScreen base).
 * Title: title.png positioned at (115, 65).
 * Font: Poppins Bold (SUBSTITUTED for legacy ArialMenuList SpriteFont).
 * Selection: highlighted yellow, others white. Wraps at top/bottom.
 */
class MenuScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    private val entries = listOf(
        MenuEntry("NEW GAME"),
        MenuEntry("EXTRAS"),
        MenuEntry("OPTIONS"),
        MenuEntry("QUIT")
    )
    private var selectedIndex = 0

    override suspend fun SContainer.sceneInit() {
        ZombustersFonts.loadFrom(fontResourcesPath)

        val assetBase = "$filesResourcesPath/zombusters/menu"
        val bgBitmap    = tryLoad("$assetBase/background_title.png")
        val scroll1Bmp  = tryLoad("$assetBase/bg_scroll_1.png")
        val scroll2Bmp  = tryLoad("$assetBase/bg_scroll_2.png")
        val scroll3Bmp  = tryLoad("$assetBase/bg_scroll_3.png")
        val titleBitmap = tryLoad("$assetBase/title.png")

        if (bgBitmap != null) {
            image(bgBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x11, 0x11, 0x22, 0xFF))
        }
        val scroll2View = if (scroll2Bmp != null) image(scroll2Bmp) { x = -200.0; y = 0.0; zIndex = 1.0; smoothing = false } else null
        if (scroll3Bmp != null) image(scroll3Bmp) { x = 0.0; y = 0.0; zIndex = 2.0; smoothing = false }
        val scroll1View = if (scroll1Bmp != null) image(scroll1Bmp) { x = -200.0; y = 0.0; zIndex = 3.0; smoothing = false } else null

        if (titleBitmap != null) {
            image(titleBitmap) {
                x = 115.0; y = 65.0
                width = 1000.0; height = 200.0
                zIndex = 10.0; smoothing = true
            }
        } else {
            text("ZOMBUSTERS") {
                textSize = 72.0; color = Colors.RED
                x = GAME_WIDTH / 2.0 - 200; y = 80.0; zIndex = 10.0
                font = ZombustersFonts.menuHeader
            }
        }

        // Determine menu area: left side, positioned ~center-left per legacy MenuComponent.CenterInXLeftMenu
        val menuX = 128.0
        val menuStartY = 320.0
        val menuSpacing = 64.0

        val menuTextViews = entries.mapIndexed { i, entry ->
            text(entry.label) {
                textSize = 40.0
                x = menuX; y = menuStartY + i * menuSpacing
                zIndex = 11.0
                font = ZombustersFonts.menuList
            }
        }

        // Context help text (bottom-left, legacy DrawContextMenu)
        text("© 2024 RETROWAX GAMES") {
            textSize = 14.0; color = Colors.WHITE
            x = menuX; y = GAME_HEIGHT - 36.0; zIndex = 11.0
            font = ZombustersFonts.menuInfo
        }

        fun refreshColors() {
            menuTextViews.forEachIndexed { i, v ->
                v.color = if (i == selectedIndex) Colors.YELLOW else Colors.WHITE
            }
        }
        refreshColors()

        var scrollPos1 = -200.0
        var scrollPos2 = -200.0
        var scrollDir = true
        var prevSelected = selectedIndex
        val capturedViews = views
        val sceneScope = this@MenuScene

        addUpdater { dt: Duration ->
            val ks = capturedViews.input.keys
            val dtMs = dt.inWholeMilliseconds.toFloat()

            // Scrolling parallax
            val v1 = 0.0015f * 2f * dtMs
            val v2 = 0.0015f * dtMs
            if (scrollDir) {
                scrollPos1 = (scrollPos1 + v1).coerceAtMost(0.0)
                scrollPos2 = (scrollPos2 + v2).coerceAtMost(0.0)
                if (scrollPos1 >= 0.0) scrollDir = false
            } else {
                scrollPos1 = (scrollPos1 - v1).coerceAtLeast(-200.0)
                scrollPos2 = (scrollPos2 - v2).coerceAtLeast(-200.0)
                if (scrollPos1 <= -200.0) scrollDir = true
            }
            scroll1View?.x = scrollPos1
            scroll2View?.x = scrollPos2

            // Navigation
            if (ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                selectedIndex = (selectedIndex - 1 + entries.size) % entries.size
            }
            if (ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                selectedIndex = (selectedIndex + 1) % entries.size
            }
            if (prevSelected != selectedIndex) {
                refreshColors()
                prevSelected = selectedIndex
            }

            if (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                sceneScope.launch { activateEntry(selectedIndex) }
            }
        }
    }

    private suspend fun activateEntry(index: Int) {
        when (index) {
            0 -> sceneContainer.changeTo {
                SelectPlayerScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
            }
            1 -> sceneContainer.changeTo {
                ExtrasMenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
            }
            2 -> sceneContainer.changeTo {
                OptionsScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
            }
            3 -> exit()
        }
    }

    private suspend fun tryLoad(path: String) = try {
        resourcesVfs[path].readBitmap()
    } catch (_: Exception) { null }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
