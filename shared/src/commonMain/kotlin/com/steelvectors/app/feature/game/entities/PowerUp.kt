package com.steelvectors.app.feature.game.entities

import com.steelvectors.app.feature.game.base.Collisionable
import com.steelvectors.app.feature.game.base.GameplayState
import com.steelvectors.app.feature.game.base.UpdatableView
import korlibs.image.bitmap.Bitmap
import korlibs.image.bitmap.slice
import korlibs.korge.internal.KorgeInternal
import korlibs.korge.render.RenderContext
import korlibs.math.geom.Point
import kotlin.random.Random

class PowerUp(
    override var position: Point,
    powerUpBitmap: List<Bitmap>
) : UpdatableView(), Collisionable {
    val type = Random.nextInt(0, powerUpBitmap.count())
    val imageSlice = powerUpBitmap[type].slice()

    override fun getImageWidth(): Int {
        return imageSlice.width
    }

    override fun getImageHeight(): Int {
        return imageSlice.height
    }

    override fun isCollidingWith(collisionable: Collisionable): Boolean {
        return this.position.x < collisionable.position.x + collisionable.getImageWidth() &&
                this.position.x + this.getImageWidth() > collisionable.position.x &&
                this.position.y < collisionable.position.y + collisionable.getImageHeight() &&
                this.position.y + this.getImageHeight() > collisionable.position.y
    }

    override fun update(screenWidth: Float, screenHeight: Float) {
        if (currentState == GameplayState.Playing) {
            if (position != null) {
                this.position = Point(position.x, position.y + DEFAULT_SPEED)
            }
        }
    }

    @OptIn(KorgeInternal::class)
    override fun renderInternal(ctx: RenderContext) {
        val powerUpTexture = ctx.getTex(imageSlice)
        ctx.batch.drawQuad(
            powerUpTexture,
            x = position.x.toFloat(),
            y = position.y.toFloat(),
            m = globalMatrix
        )
    }

    companion object {
        private const val DEFAULT_SPEED = 1
    }
}
