package com.steelvectors.app.feature.game.entities

import korlibs.image.color.Colors
import korlibs.image.font.BitmapFont
import korlibs.korge.input.onDown
import korlibs.korge.input.onUpAnywhere
import korlibs.korge.view.Container
import korlibs.korge.view.View
import korlibs.korge.view.graphics
import korlibs.korge.view.position
import korlibs.korge.view.text
import korlibs.math.geom.Point

private const val BUTTON_HEIGHT = 28
private const val BUTTON_BACKGROUND_ALPHA = 0.4
private const val ALPHA_ON_PRESSED = 0.8
private const val ALPHA_NON_PRESSED = 1.0
private const val RECT_CHAR_OFFSET = 15

class Button(
    val position: Point,
    val text: String,
    val onButton: (pressed: Boolean) -> Unit = { _ -> },
    //val size: Pair<Double, Double>? = null,
    bitmapFont: BitmapFont? = null
) : Container() {
    private val background = graphics {
        /*position(position).fill(Colors.WHITE) {
          if (size != null) {
            rect(0, 0, size.first + 4, size.second + 10)
          } else {
            rect(0, 0, RECT_CHAR_OFFSET * text.length, BUTTON_HEIGHT)
          }
          alpha = BUTTON_BACKGROUND_ALPHA
        }*/
    }
    val textView = if (bitmapFont != null) {
        text(text = text, color = Colors.DARKGREEN, font = bitmapFont)
            .position(position.x + 1, position.y + 4)
            .also { view ->
                /*view.filtering = false
                size?.let {
                  view.setSize(size.first, size.second)
                }*/
            }
    } else {
        text(text = text, color = Colors.DARKGREEN)
            .position(position.x + 1, position.y + 8)
            .also { view ->
                /*view.filtering = false
                size?.let {
                  view.setSize(size.first, size.second)
                }*/
            }
    }

    init {
        addChild(background)
        addChild(textView)
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
