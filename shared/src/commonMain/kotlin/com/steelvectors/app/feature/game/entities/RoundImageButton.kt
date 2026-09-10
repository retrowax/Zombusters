package com.steelvectors.app.feature.game.entities

import korlibs.image.bitmap.Bitmap
import korlibs.image.color.Colors
import korlibs.korge.input.onDown
import korlibs.korge.input.onUpAnywhere
import korlibs.korge.view.Container
import korlibs.korge.view.View
import korlibs.korge.view.circle
import korlibs.korge.view.image
import korlibs.korge.view.position
import korlibs.math.geom.Point
import korlibs.math.geom.degrees

private const val BUTTON_BACKGROUND_ALPHA = 0.4
private const val ALPHA_ON_PRESSED = 0.4
private const val ALPHA_NON_PRESSED = 1.0

class RoundImageButton(
    val position: Point,
    val bitmap: Bitmap,
    val onButton: (pressed: Boolean) -> Unit = { _ -> },
    val flipHorizontal: Boolean = false,
    val buttonRadius: Double
) : Container() {
    private val background = circle {
        position(position.x - radius * 3, position.y - radius * 3)
        radius = buttonRadius
        color = Colors.LIGHTGREY
    }.apply {
        alpha = BUTTON_BACKGROUND_ALPHA
    }

    private val imageView = if (flipHorizontal) {
        image(bitmap).position(position.x + 35, position.y + 30)
    } else {
        image(bitmap).position(position.x - 35, position.y - 30)
    }

    init {
        if (flipHorizontal) {
            imageView.rotation = (180).degrees
        }
        addChild(background)
        addChild(imageView)
        decorateButton()
    }

    private fun <T : View> T.decorateButton() = this.apply {
        var pressing = false
        onDown {
            pressing = true
            alpha = ALPHA_ON_PRESSED
            onButton(true)
        }
        onUpAnywhere {
            if (pressing) {
                pressing = false
                alpha = ALPHA_NON_PRESSED
                onButton(false)
            }
        }
    }
}
