package com.steelvectors.app.feature.game.base

import korlibs.korge.internal.KorgeInternal
import korlibs.korge.render.RenderContext
import korlibs.korge.view.Image
import korlibs.math.geom.Point

class ScrollingBackground(
    val image: Image,
    val screenWidth: Float,
    val screenHeight: Float,
    private val scrollVelocity: Int? = SCROLL_VELOCITY
) : UpdatableView(screenWidth, screenHeight) {
    var position = Point(0, screenHeight - image.scaledHeight)
    private var frame = 0

    override fun update(screenWidth: Float, screenHeight: Float) {
        if (currentState == GameplayState.Playing) {
            frame++

            if (frame.rem(scrollVelocity ?: SCROLL_VELOCITY) == 0) {
                position = if (position.y <= 0) {
                    Point(position.x, position.y + 1)
                } else {
                    Point(position.x, screenHeight - image.scaledHeight)
                }
            }
        }
    }

    @OptIn(KorgeInternal::class)
    override fun renderInternal(ctx: RenderContext) {
        val tex = ctx.getTex(image.bitmap)
        ctx.batch.drawQuad(
            tex,
            x = position.x.toFloat(),
            y = position.y.toFloat(),
            m = globalMatrix,
            width = image.scaledWidth.toFloat(),
            height = image.scaledHeight.toFloat()
        )
    }

    companion object {
        const val SCROLL_VELOCITY = 14
    }
}
