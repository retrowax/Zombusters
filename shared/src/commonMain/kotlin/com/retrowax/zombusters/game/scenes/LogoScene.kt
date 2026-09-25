package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.ui.ZombustersFonts
import korlibs.event.Key
import korlibs.image.color.Colors
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

private const val FADE_DURATION_S = 1.2f
private const val HOLD_DURATION_S = 1.0f

/**
 * Logo/intro screen — first scene the user sees.
 *
 * Legacy: LogoScreen.cs fades in retrowax-logo-splash, holds ~1s, fades out.
 * Port: uses zombusters_logo.png (retrowax-logo-splash not yet extracted).
 * Any key skips the full sequence and advances to StartScene.
 */
class LogoScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    override suspend fun SContainer.sceneInit() {
        ZombustersFonts.loadFrom(fontResourcesPath)

        val assetBase = "$filesResourcesPath/zombusters"

        solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), Colors.BLACK)

        val logoBitmap = if (filesResourcesPath.isNotEmpty()) {
            try { resourcesVfs["$assetBase/menu/retrowax_logo.png"].readBitmap() }
            catch (_: Exception) { null }
        } else null

        val logoView = if (logoBitmap != null) {
            val scale = minOf(
                GAME_WIDTH * 0.75 / logoBitmap.width,
                GAME_HEIGHT * 0.6 / logoBitmap.height
            )
            image(logoBitmap) {
                this.scale = scale
                x = (GAME_WIDTH - logoBitmap.width * scale) / 2
                y = (GAME_HEIGHT - logoBitmap.height * scale) / 2
                alpha = 0.0
                smoothing = true
            }
        } else {
            text("RETROWAX GAMES") {
                textSize = 48.0; color = Colors.WHITE; alpha = 0.0
                x = GAME_WIDTH / 2.0 - 190; y = GAME_HEIGHT / 2.0 - 30
            }
        }

        var elapsed = 0f
        var phase = 0
        var done = false
        val capturedViews = views
        val sceneScope = this@LogoScene

        fun advance() {
            if (!done) {
                done = true
                sceneScope.launch {
                    sceneContainer.changeTo {
                        StartScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                    }
                }
            }
        }

        addUpdater { dt: Duration ->
            val ks = capturedViews.input.keys
            if (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE) || ks.justPressed(Key.ESCAPE)) {
                advance()
                return@addUpdater
            }

            elapsed += dt.inWholeMilliseconds / 1000f
            when (phase) {
                0 -> {
                    logoView.alpha = (elapsed / FADE_DURATION_S).toDouble().coerceIn(0.0, 1.0)
                    if (elapsed >= FADE_DURATION_S) { phase = 1; elapsed = 0f }
                }
                1 -> {
                    logoView.alpha = 1.0
                    if (elapsed >= HOLD_DURATION_S) { phase = 2; elapsed = 0f }
                }
                2 -> {
                    logoView.alpha = (1.0f - elapsed / FADE_DURATION_S).toDouble().coerceIn(0.0, 1.0)
                    if (elapsed >= FADE_DURATION_S) { phase = 3; advance() }
                }
            }
        }
    }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
