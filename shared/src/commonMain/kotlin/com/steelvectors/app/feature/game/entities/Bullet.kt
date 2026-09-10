package com.steelvectors.app.feature.game.entities

import com.steelvectors.app.feature.game.base.Collisionable
import com.steelvectors.app.feature.game.base.GameplayState
import com.steelvectors.app.feature.game.base.UpdatableView
import korlibs.image.bitmap.Bitmap
import korlibs.image.bitmap.slice
import korlibs.korge.internal.KorgeInternal
import korlibs.korge.render.RenderContext
import korlibs.math.geom.Point

class Bullet(override var position: Point, image: Bitmap, val direction: BulletDirection) :
    UpdatableView(),
    Collisionable {
    private val imageSlice = image.slice()

    init {
        this.position = Point(position.x + X_OFFSET, position.y + Y_OFFSET)
    }

    override fun update(screenWidth: Float, screenHeight: Float) {
        if (currentState == GameplayState.Playing) {
            if (position != null) {
                if (direction == BulletDirection.UP) {
                    this.position = Point(position.x, position.y - DEFAULT_SPEED)
                } else {
                    this.position = Point(position.x, position.y + DEFAULT_SPEED)
                }
            }
        }
    }

    @OptIn(KorgeInternal::class)
    override fun renderInternal(ctx: RenderContext) {
        val tex = ctx.getTex(imageSlice)
        ctx.batch.drawQuad(
            tex,
            x = position.x.toFloat(),
            y = position.y.toFloat(),
            m = globalMatrix
        )
    }

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

    companion object {
        private const val X_OFFSET = 8
        private const val Y_OFFSET = -18
        private const val DEFAULT_SPEED = 13
    }
}

enum class BulletDirection {
    UP,
    DOWN
}
