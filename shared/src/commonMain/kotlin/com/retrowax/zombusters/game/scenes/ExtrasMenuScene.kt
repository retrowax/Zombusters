package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.ui.ZombustersFonts
import com.retrowax.zombusters.localization.getCurrentLocalization
import korlibs.event.Key
import korlibs.event.MouseButton
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
import kotlin.time.Duration

private data class ExtrasEntry(val label: String, val available: Boolean = true)

/**
 * Extras menu — legacy ExtrasMenuScreen.cs.
 *
 * Entries from legacy ExtrasMenuScreen.cs:
 *   0 → HOW TO PLAY  (HowToPlayInGameString → DisplayHowToPlay)
 *   1 → LEADERBOARD  (LeaderboardMenuString → DisplayLeaderBoard) — DEFERRED (online)
 *   2 → CREDITS      (CreditsMenuString → DisplayCredits)
 *
 * ESC → back to MenuScene.
 */
class ExtrasMenuScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = ""
) : Scene() {

    private val _loc = getCurrentLocalization()
    private val entries = listOf(
        ExtrasEntry(_loc.extrasHowToPlay),
        ExtrasEntry(_loc.extrasLeaderboard, available = false),  // DEFERRED: online
        ExtrasEntry(_loc.extrasCredits)
    )
    private var selectedIndex = 0

    override suspend fun SContainer.sceneInit() {
        ZombustersFonts.loadFrom(fontResourcesPath)

        val isMobile = views.input.isTouchDevice
        val menuBase = "$filesResourcesPath/zombusters/menu"
        val bgBitmap    = tryLoad("$menuBase/background_title.png")
        val titleBitmap = tryLoad("$menuBase/title.png")

        if (bgBitmap != null) {
            image(bgBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x11, 0x11, 0x22, 0xFF))
        }
        if (titleBitmap != null) {
            image(titleBitmap) { x = 115.0; y = 20.0; width = 1000.0; height = 180.0; zIndex = 1.0; smoothing = true }
        } else {
            text("ZOMBUSTERS") {
                textSize = 56.0; color = Colors.RED
                x = GAME_WIDTH / 2.0 - 150; y = 30.0; zIndex = 1.0
                font = ZombustersFonts.menuHeader
            }
        }

        text("EXTRAS") {
            textSize = 36.0; color = Colors.WHITE
            x = 128.0; y = 230.0; zIndex = 2.0
            font = ZombustersFonts.menuHeader
        }

        val menuStartY = 300.0
        val menuSpacing = 60.0
        val menuTextViews = entries.mapIndexed { i, entry ->
            text(entry.label) {
                textSize = 34.0
                x = 128.0; y = menuStartY + i * menuSpacing
                zIndex = 2.0
                font = ZombustersFonts.menuList
                color = if (!entry.available) Colors.DARKGRAY else Colors.WHITE
            }
        }

        if (!isMobile) {
            text("ESC — Back") {
                textSize = 20.0; color = Colors.WHITE
                x = 128.0; y = GAME_HEIGHT - 50.0; zIndex = 2.0
                font = ZombustersFonts.menuInfo
            }
        }

        fun refreshColors() {
            entries.forEachIndexed { i, entry ->
                menuTextViews[i].color = when {
                    !entry.available -> Colors.DARKGRAY
                    i == selectedIndex -> Colors.YELLOW
                    else -> Colors.WHITE
                }
            }
        }
        refreshColors()

        var prevSelected = selectedIndex
        var done = false
        val capturedViews = views
        val sceneScope = this@ExtrasMenuScene

        val menuStartY2 = menuStartY
        val menuSpacing2 = menuSpacing

        // Touch tap on menu entries
        touch {
            end { info ->
                if (done) return@end
                val tx = info.local.x; val ty = info.local.y
                for (i in entries.indices) {
                    val ey = menuStartY2 + i * menuSpacing2
                    if (tx >= 50.0 && tx <= 900.0 && ty >= ey - 15 && ty <= ey + 50) {
                        if (!entries[i].available) return@end
                        selectedIndex = i; done = true
                        sceneScope.launch {
                            when (i) {
                                0 -> sceneContainer.changeTo { HowToPlayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                                2 -> sceneContainer.changeTo { CreditsScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, canLeaveImmediately = false) }
                                else -> {}
                            }
                            done = false
                        }
                        break
                    }
                }
                // Back area (bottom left)
                if (ty >= GAME_HEIGHT - 80.0) {
                    done = true
                    sceneScope.launch { sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }; done = false }
                }
            }
        }

        var prevMouseDown = false

        addUpdater { _: Duration ->
            val ks = capturedViews.input.keys

            // Mouse click hit-test
            val mouseDown = capturedViews.input.mouseButtonPressed(MouseButton.LEFT)
            if (!mouseDown && prevMouseDown && !done) {
                val mp = capturedViews.input.mousePos
                for (i in entries.indices) {
                    val ey = menuStartY2 + i * menuSpacing2
                    if (mp.x >= 50.0 && mp.x <= 900.0 && mp.y >= ey - 15 && mp.y <= ey + 50) {
                        if (!entries[i].available) break
                        selectedIndex = i; done = true
                        sceneScope.launch {
                            when (i) {
                                0 -> sceneContainer.changeTo { HowToPlayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                                2 -> sceneContainer.changeTo { CreditsScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, canLeaveImmediately = false) }
                                else -> {}
                            }
                            done = false
                        }
                        break
                    }
                }
            }
            prevMouseDown = mouseDown

            if (ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                selectedIndex = (selectedIndex - 1 + entries.size) % entries.size
            }
            if (ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                selectedIndex = (selectedIndex + 1) % entries.size
            }
            if (prevSelected != selectedIndex) {
                prevSelected = selectedIndex
                refreshColors()
            }

            if (ks.justPressed(Key.ESCAPE)) {
                if (!done) {
                    done = true
                    sceneScope.launch {
                        sceneContainer.changeTo {
                            MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                        }
                    }
                }
            }

            if (!done && (ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE))) {
                val entry = entries[selectedIndex]
                if (!entry.available) return@addUpdater
                done = true
                sceneScope.launch {
                    when (selectedIndex) {
                        0 -> sceneContainer.changeTo {
                            HowToPlayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                        }
                        1 -> { /* DEFERRED — Leaderboard */ done = false }
                        2 -> sceneContainer.changeTo {
                            CreditsScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, canLeaveImmediately = false)
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
