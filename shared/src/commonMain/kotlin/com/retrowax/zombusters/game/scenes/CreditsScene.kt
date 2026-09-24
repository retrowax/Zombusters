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
import korlibs.korge.view.container
import korlibs.korge.view.image
import korlibs.korge.view.solidRect
import korlibs.korge.view.text
import korlibs.io.file.std.resourcesVfs
import korlibs.math.geom.Size
import kotlinx.coroutines.launch
import kotlin.time.Duration

/**
 * Credits screen — legacy CreditsScreen.cs.
 *
 * Original behavior:
 *   - Displays title.png at top (Rectangle 115,65 1000×323)
 *   - Reads Credits.txt (legacy LevelXMLs/Credits.txt) and scrolls it upward
 *   - ESC available once scrolling reaches end of text (canLeaveScreen)
 *   - Font: ArialMenuInfo (SUBSTITUTED → Poppins Regular)
 *
 * The Credits.txt content is embedded directly to avoid runtime file dependency on legacy folder.
 * Scroll speed: ~30px/sec.
 */
class CreditsScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = "",
    private val canLeaveImmediately: Boolean = false
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

        // Title header (Rectangle 115, 65, 1000×160)
        if (titleBitmap != null) {
            image(titleBitmap) {
                x = 115.0; y = 20.0; width = 1000.0; height = 160.0
                zIndex = 1.0; smoothing = true
            }
        } else {
            text("ZOMBUSTERS") {
                textSize = 56.0; color = Colors.RED
                x = GAME_WIDTH / 2.0 - 150; y = 30.0; zIndex = 1.0
                font = ZombustersFonts.menuHeader
            }
        }

        // Scrolling text clip area: Rectangle(0, 280, 1280, 440) from legacy mText
        val clipY = 200.0
        val clipH = 440.0

        // Clip mask for scrolling region
        solidRect(GAME_WIDTH.toDouble(), clipH, Colors.BLACK).apply {
            x = 0.0; y = clipY; zIndex = 0.5; alpha = 0.6
        }

        val scrollContainer = container { zIndex = 2.0 }

        var lineY = clipY + clipH  // start below visible area
        val lineHeight = 26.0
        val creditsLines = CREDITS_TEXT.lines()

        creditsLines.forEach { line ->
            scrollContainer.text(line) {
                textSize = 18.0; color = Colors.WHITE
                x = GAME_WIDTH / 2.0 - 250
                y = lineY
                font = ZombustersFonts.menuInfo
            }
            lineY += lineHeight
        }
        val totalTextHeight = lineY - (clipY + clipH)

        var scrollY = 0.0
        var canLeave = canLeaveImmediately
        var done = false

        val hintText = text(if (canLeave) "TAP / ESC — Back" else "") {
            textSize = 20.0; color = Colors.WHITE
            x = 128.0; y = GAME_HEIGHT - 50.0; zIndex = 3.0
            font = ZombustersFonts.menuInfo
        }

        val capturedViews = views
        val sceneScope = this@CreditsScene

        fun tryLeave() {
            if (canLeave && !done) {
                done = true
                sceneScope.launch {
                    sceneContainer.changeTo {
                        ExtrasMenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                    }
                }
            } else if (!canLeave) {
                // Skip to end
                scrollY = totalTextHeight + clipH + 1.0
                canLeave = true
                hintText.text = "TAP / ESC — Back"
            }
        }

        touch { end { tryLeave() } }

        var prevMouseDown = false

        addUpdater { dt: Duration ->
            val dtSec = dt.inWholeMilliseconds / 1000.0
            scrollY += 30.0 * dtSec
            scrollContainer.y = -scrollY

            scrollContainer.children.forEachIndexed { i, child ->
                val worldY = (clipY + clipH) + i * lineHeight - scrollY
                child.visible = worldY > clipY - lineHeight && worldY < clipY + clipH + lineHeight
            }

            if (scrollY >= totalTextHeight + clipH && !canLeave) {
                canLeave = true
                hintText.text = "TAP / ESC — Back"
            }

            val mouseDown = capturedViews.input.mouseButtonPressed(MouseButton.LEFT)
            if (!mouseDown && prevMouseDown) tryLeave()
            prevMouseDown = mouseDown

            val ks = capturedViews.input.keys
            if (ks.justPressed(Key.ESCAPE) || ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE)) {
                tryLeave()
            }
        }
    }

    private suspend fun tryLoad(path: String) = try {
        resourcesVfs[path].readBitmap()
    } catch (_: Exception) { null }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)

    companion object {
        // Content from legacy/ZombustersWindows/LevelXMLs/Credits.txt — preserved verbatim
        val CREDITS_TEXT = """
DEVELOPED BY RETROWAX GAMES



PROGRAMMING & GAME DESIGN

Ferran Pons



LEAD ARTIST

Paco Illescas



PIXEL ART & ANIMATIONS

Ferran Pons



PRODUCTION ASSISTANT

Vanesa Rodriguez



MUSIC

'High Ground'
Performed by London To Tokyo.
Written by Simon Steadman. (ascap)
Used by Permission. All Rights Reserved.

'Dancing on a Dime'
Performed by Bare Wires. (Live at WFMU on the Evan Funk Davies show)
Used by Permission. All Rights Reserved.

'High Dive'
Performed by Black Math. (Phantom Power)
Used by Permission. All Rights Reserved.

'Suck City'
Performed by Black Math. (Phantom Power)
Used by Permission. All Rights Reserved.

'Bad Attraction'
Performed by Brad Sucks. (I Don't Know What I'm Doing)
Used by Permission. All Rights Reserved.

'Understood by your Dad'
Performed by Brad Sucks. (Out of It)
Used by Permission. All Rights Reserved.

'Wolfram'
Performed by Kraus. (I Could Destroy You with a Single Thought)
Used by Permission. All Rights Reserved.

'Opening The Portal'
Performed by Nuit Noire. (split 7'' with His Electro Blue Voice)
Used by Permission. All Rights Reserved.
Used Courtesy of AVANT! Records.

'I Don't Like You'
Performed by The Black Bug. (I Don't Like You 7'')
Used by Permission. All Rights Reserved.
Used Courtesy of AVANT! Records.

'In The Hall Of The Mountain King'
Performed by The Itchy Creeps. (The Itchy Creeps)
Used by Permission. All Rights Reserved.
Used Courtesy of Happy Puppy Records.

'As You Know'
Performed by THIS CO. (THIS CO.)
Used by Permission. All Rights Reserved.

'Take It Away'
Performed by THIS CO. (THIS CO.)
Used by Permission. All Rights Reserved.



MUSIC EQ

Juanfran Rodriguez



SOUND FX

Ferran Pons
Juanfran Rodriguez



QA (Playtest)

Ivan Oleart
Jose Aranda
Peter Schraut (XNA Community)
Krazy Insane (XNA Community)
Ion Vapor Studios (XNA Community)
DavidParker (XNA Community)



LOCALIZATION

Ferran Pons


TRANSLATIONS

Carles Prats
Peter Schraut
Paolo Bernagozzi
William David



SPECIAL THANKS

Vanesa Rodriguez
Paco Illescas
JuanFran Rodriguez
Ivan Oleart
Carles Prats
Jose Aranda
Paolo Bernagozzi
Simon Steadman (London To Tokyo)
Carlos Vergara (THIS CO.)
Andrea (AVANT! Records)
Jimmy (Black Math)
Brad (Brad Sucks)
Lee (Happy Puppy Records)
Matthew Melton (Bare Wires)
Kraus
Peter Schraut
Swing Swing Submarine
Jimmy K. Oak



THIS GAME IS DEDICATED TO MY FAMILY
SPECIALLY TO MY PARENTS RAMON AND JUANA
WITHOUT THEM THIS DREAM COULD NOT BE A REALITY

ALSO I WANT TO THANK TO ALL THE PEOPLE INVOLVED
IN THIS PROJECT, YOU ARE AWESOME!!

A SPECIAL EXTRA THANKS TO VANESA RODRIGUEZ
FOR HER PATIENCE, UNDERSTANDING AND LOVE!!

AN SPECIAL EXTRA-EXTRA DEDICATION
TO MY SON PAU AND MY DAUGHTER ELIA!!
(born Feb. 2013 and Oct. 2015)



"If you can dream it, you can do it." (Walt Disney)
        """.trimIndent()
    }
}
