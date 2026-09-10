package com.steelvectors.app.feature.game.entities.enemies

import com.steelvectors.app.feature.game.base.Collisionable
import com.steelvectors.app.feature.game.base.GameplayState
import com.steelvectors.app.feature.game.entities.Enemy
import com.steelvectors.app.feature.game.entities.EnemyStatus
import com.steelvectors.app.feature.game.entities.EnemyType
import com.steelvectors.app.feature.game.entities.Player.Companion.ANIMATION_SPEED
import korlibs.image.bitmap.Bitmap
import korlibs.korge.internal.KorgeInternal
import korlibs.korge.render.RenderContext
import korlibs.korge.view.SpriteAnimation
import korlibs.math.geom.Point

class Gunship(
    override var position: Point,
    spriteBitmap: Bitmap,
    screenWidth: Float,
    screenHeight: Float
) : Enemy(screenWidth, screenHeight) {
    private val planeSprite = SpriteAnimation(
        spriteMap = spriteBitmap,
        spriteWidth = 128,
        spriteHeight = 128,
        marginTop = 0,
        marginLeft = 0,
        columns = 3,
        rows = 4,
        offsetBetweenColumns = 0,
        offsetBetweenRows = 0,
    )
    private val planeBitmap = planeSprite.sprites[11]
    private val propellerAnimation: SpriteAnimation = SpriteAnimation(
        spriteMap = spriteBitmap,
        spriteWidth = 128,
        spriteHeight = 128,
        marginTop = 0,
        marginLeft = 0,
        columns = 4,
        rows = 1,
        offsetBetweenColumns = 0,
        offsetBetweenRows = 0,
    )
    private var thrusterSlice = propellerAnimation.sprites[0]
    override var status = EnemyStatus.ALIVE
    override val type = EnemyType.GUNSHIP
    private var frame = 0
    private var currentAnimationFrame = 0

    override fun update(screenWidth: Float, screenHeight: Float) {
        if (currentState == GameplayState.Playing) {
            frame++

            if (position != null) {
                this.position = Point(this.position.x, this.position.y + DEFAULT_SPEED)
                if (position.y > screenHeight) {
                    position = position.copy(y = -5.0)
                }
            }

            if (frame.rem(ANIMATION_SPEED) == 0) {
                if (currentAnimationFrame < propellerAnimation.sprites.count() - 1) {
                    currentAnimationFrame++
                    thrusterSlice = propellerAnimation.sprites[currentAnimationFrame]
                } else {
                    currentAnimationFrame = 0
                }
            }
        }
    }

    @OptIn(KorgeInternal::class)
    override fun renderInternal(ctx: RenderContext) {
        val tex = ctx.getTex(planeBitmap)
        ctx.batch.drawQuad(
            tex = tex.flippedY(),
            x = position.x.toFloat(),
            y = position.y.toFloat(),
            width = SPRITE_WIDTH.toFloat(),
            height = SPRITE_WIDTH.toFloat(),
            m = globalMatrix
        )

        val thrusterTexture = ctx.getTex(thrusterSlice)
        ctx.batch.drawQuad(
            tex = thrusterTexture.flippedY(),
            x = position.x.toFloat(),
            y = position.y.toFloat(),
            width = SPRITE_WIDTH.toFloat(),
            height = SPRITE_WIDTH.toFloat(),
            m = globalMatrix
        )
    }

    override fun getImageWidth(): Int {
        return SPRITE_WIDTH.toInt()
    }

    override fun getImageHeight(): Int {
        return SPRITE_WIDTH.toInt()
    }

    override fun isCollidingWith(collisionable: Collisionable): Boolean {
        return this.position.x < collisionable.position.x + collisionable.getImageWidth() &&
                this.position.x + this.getImageWidth() > collisionable.position.x &&
                this.position.y < collisionable.position.y + collisionable.getImageHeight() &&
                this.position.y + this.getImageHeight() > collisionable.position.y
    }

    companion object {
        const val SPRITE_WIDTH = 48.0
    }
}
