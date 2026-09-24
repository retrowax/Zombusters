package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.ui.ZombustersFonts
import com.retrowax.zombusters.localization.getCurrentLocalization
import korlibs.audio.sound.readMusic
import korlibs.event.Key
import korlibs.korge.input.touch
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
import kotlin.math.cos
import kotlin.time.Duration

/**
 * Title screen — "press any key" screen before the main menu.
 *
 * Legacy: StartScreen.cs
 *   - Scrolling background (BackgroundScreen)
 *   - Title image at Rectangle(115, 65, 1000, 323)
 *   - Pulsing "PRESS ANY KEY" (cosine wave at 2 rad/s)
 *   - Copyright string at bottom
 *   - On any input → MenuScene (simplified: skip legacy LoadInScreen + MessageBox)
 *   - Music: NuitNoire_OpeningThePortal.ogg (StartPlayingMusic(6) → index 6 in song list)
 *
 * Background draw order (BackgroundScreen.cs):
 *   1. background_title.png  (static)
 *   2. bg_scroll_2.png       (slow parallax, oscillates ±100px)
 *   3. bg_scroll_3.png       (static overlay)
 *   4. bg_scroll_1.png       (fast parallax, oscillates ±100px × 2)
 *   5. background_shadow     (not yet extracted — omitted)
 */
class StartScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    override suspend fun SContainer.sceneInit() {
        ZombustersFonts.loadFrom(fontResourcesPath)

        val assetBase = "$filesResourcesPath/zombusters/menu"

        val bgBitmap     = tryLoad("$assetBase/background_title.png")
        val scroll1Bmp   = tryLoad("$assetBase/bg_scroll_1.png")
        val scroll2Bmp   = tryLoad("$assetBase/bg_scroll_2.png")
        val scroll3Bmp   = tryLoad("$assetBase/bg_scroll_3.png")
        val titleBitmap  = tryLoad("$assetBase/title.png")

        // Layer 1: static base
        if (bgBitmap != null) {
            image(bgBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x11, 0x11, 0x22, 0xFF))
        }

        // Layer 2: slow parallax
        val scroll2View = if (scroll2Bmp != null) {
            image(scroll2Bmp) { x = -200.0; y = 0.0; zIndex = 1.0; smoothing = false }
        } else null

        // Layer 3: static overlay
        if (scroll3Bmp != null) {
            image(scroll3Bmp) { x = 0.0; y = 0.0; zIndex = 2.0; smoothing = false }
        }

        // Layer 4: fast parallax
        val scroll1View = if (scroll1Bmp != null) {
            image(scroll1Bmp) { x = -200.0; y = 0.0; zIndex = 3.0; smoothing = false }
        } else null

        // Title image: legacy places it at Rectangle(115, 65, 1000, 323)
        if (titleBitmap != null) {
            image(titleBitmap) {
                x = 115.0; y = 65.0
                width = 1000.0; height = 323.0
                zIndex = 10.0; smoothing = true
            }
        } else {
            text("ZOMBUSTERS") {
                textSize = 96.0; color = Colors.RED
                x = GAME_WIDTH / 2.0 - 270; y = 120.0; zIndex = 10.0
                font = ZombustersFonts.menuHeader
            }
        }

        // "PRESS ANY KEY" pulsing text — positioned like legacy (75% down uiBounds)
        val pressKeyText = text(getCurrentLocalization().pressAnyKey) {
            textSize = 32.0; color = Colors.WHITE
            x = GAME_WIDTH / 2.0 - 140; y = GAME_HEIGHT * 0.75
            zIndex = 11.0
            font = ZombustersFonts.menuList
        }

        // Copyright
        text("© 2024 RETROWAX GAMES") {
            textSize = 16.0; color = Colors.WHITE
            x = GAME_WIDTH / 2.0 - 120; y = GAME_HEIGHT - 36.0
            zIndex = 11.0
            font = ZombustersFonts.menuInfo
        }

        // Start music: NuitNoire_OpeningThePortal.ogg (index 7 in MusicComponent songsList)
        if (filesResourcesPath.isNotEmpty()) {
            try {
                val music = resourcesVfs["$filesResourcesPath/music/NuitNoire_OpeningThePortal.ogg"].readMusic()
                music.playNoCancelForever()
            } catch (_: Exception) {}
        }

        var totalSec = 0f
        // Scrolling state: oscillate between -200 and 0 (from BackgroundScreen.cs)
        var scrollPos1 = -200.0   // fast layer, range [-200, 0]
        var scrollPos2 = -200.0   // slow layer
        var scrollDir = true      // true = moving right
        var done = false
        val capturedViews = views
        val sceneScope = this@StartScene

        fun advance() {
            if (!done) {
                done = true
                sceneScope.launch {
                    sceneContainer.changeTo {
                        MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                    }
                }
            }
        }

        // Touch tap anywhere = advance (legacy: any input advances StartScreen)
        touch { end { advance() } }

        addUpdater { dt: Duration ->
            val ks = capturedViews.input.keys
            // Any key or mouse click advances
            if (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE) ||
                ks.justPressed(Key.ESCAPE) || ks.justPressed(Key.UP) ||
                ks.justPressed(Key.DOWN) || ks.justPressed(Key.W) ||
                ks.justPressed(Key.S) || ks.justPressed(Key.A) ||
                ks.justPressed(Key.D) || capturedViews.input.mouseButtonPressed(korlibs.event.MouseButton.LEFT)) {
                advance()
                return@addUpdater
            }

            val dtMs = dt.inWholeMilliseconds.toFloat()
            totalSec += dtMs / 1000f

            // Scrolling oscillation (BackgroundScreen.cs VELOCITY=0.0015 per ms)
            val v1 = 0.0015f * 2f * dtMs   // fast layer
            val v2 = 0.0015f * dtMs         // slow layer
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

            // Pulsing alpha: cos wave at 2 rad/s (from StartScreen.cs)
            val value = (cos(totalSec * 2.0f) + 1f) / 2f
            pressKeyText.alpha = value.toDouble()
        }
    }

    private suspend fun tryLoad(path: String) = try {
        resourcesVfs[path].readBitmap()
    } catch (_: Exception) { null }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
