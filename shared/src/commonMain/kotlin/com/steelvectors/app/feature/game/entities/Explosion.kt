package com.steelvectors.app.feature.game.entities

import com.steelvectors.app.feature.game.base.AnimationState
import com.steelvectors.app.feature.game.base.GameplayState
import com.steelvectors.app.feature.game.base.UpdatableView
import korlibs.image.bitmap.Bitmap
import korlibs.image.bitmap.BitmapSlice
import korlibs.image.bitmap.slice
import korlibs.korge.internal.KorgeInternal
import korlibs.korge.render.RenderContext
import korlibs.math.geom.Point

class Explosion(var position: Point, bitmapList: List<Bitmap>) : UpdatableView() {
    private val animationSlices = getAnimationSlices(bitmapList)
    var imageSlice = animationSlices[0]
    private var frame = 0
    private var currentAnimationFrame = 0
    var currentAnimationState = AnimationState.PLAYING

    override fun update(screenWidth: Float, screenHeight: Float) {
        if (currentState == GameplayState.Playing) {
            frame++

            if (frame.rem(ANIMATION_SPEED) == 0) {
                if (currentAnimationFrame < animationSlices.count() - 1) {
                    currentAnimationFrame++
                    imageSlice = animationSlices[currentAnimationFrame]
                } else {
                    currentAnimationState = AnimationState.ENDED
                }
            }
        }
    }

    @OptIn(KorgeInternal::class)
    override fun renderInternal(ctx: RenderContext) {
        if (currentAnimationState == AnimationState.PLAYING) {
            val tex = ctx.getTex(imageSlice)
            ctx.batch.drawQuad(
                tex,
                x = position.x.toFloat(),
                y = position.y.toFloat(),
                m = globalMatrix
            )
        }
    }

    private fun getAnimationSlices(bitmapList: List<Bitmap>): List<BitmapSlice<Bitmap>> {
        val bitmapSlices = mutableListOf<BitmapSlice<Bitmap>>()
        for (bitmap in bitmapList) {
            bitmapSlices.add(bitmap.slice())
        }
        return bitmapSlices
    }

    companion object {
        const val ANIMATION_SPEED = 2
    }
}
