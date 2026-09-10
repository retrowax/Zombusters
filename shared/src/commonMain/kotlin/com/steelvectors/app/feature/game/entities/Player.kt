package com.steelvectors.app.feature.game.entities

import com.steelvectors.app.feature.game.base.Collisionable
import com.steelvectors.app.feature.game.base.GameplayState
import com.steelvectors.app.feature.game.base.UpdatableView
import korlibs.image.bitmap.Bitmap
import korlibs.image.bitmap.BitmapSlice
import korlibs.image.bitmap.slice
import korlibs.korge.render.RenderContext
import korlibs.korge.view.SpriteAnimation
import korlibs.math.geom.Point

const val DEFAULT_LIVES = 3
private const val DEFAULT_SPEED = 4
private const val RIGHT_OFFSET = 270
private const val LEFT_OFFSET = 0
private const val POWER_UP_COUNTDOWN_IN_MILLIS: Long = 25000
private const val PLAYER_SPRITE_WIDTH = 48.0

class Player(
    override var position: Point,
    planeBitmap: Bitmap,
    screenWidth: Float,
    screenHeight: Float,
) : UpdatableView(screenWidth, screenHeight), Collisionable {

    var livesCount = DEFAULT_LIVES
    private var moveDirection = Direction.NONE
    private var frame = 0
    private var currentAnimationFrame = 0
    var shotType = ShotType.SINGLE
    private val spitfireSprite = SpriteAnimation(
        spriteMap = planeBitmap,
        spriteWidth = 128,
        spriteHeight = 128,
        marginTop = 0,
        marginLeft = 0,
        columns = 3,
        rows = 4,
        offsetBetweenColumns = 0,
        offsetBetweenRows = 0,
    )
    private val spitfireImage = spitfireSprite.sprites[11]
    private val propellerAnimation: SpriteAnimation = SpriteAnimation(
        spriteMap = planeBitmap,
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

    override fun update(screenWidth: Float, screenHeight: Float) {
        if (currentState == GameplayState.Playing) {
            frame++

            if (moveDirection != null) {
                if (moveDirection == Direction.RIGHT && position.x + DEFAULT_SPEED < screenWidth - PLAYER_SPRITE_WIDTH) {
                    this.position = Point(position.x + DEFAULT_SPEED, position.y)
                }
                if (moveDirection == Direction.LEFT && position.x - DEFAULT_SPEED > LEFT_OFFSET) {
                    this.position = Point(position.x - DEFAULT_SPEED, position.y)
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

    override fun renderInternal(ctx: RenderContext) {
        if (currentState != GameplayState.GameOver) {
            val shipTexture = ctx.getTex(spitfireImage)
            val thrusterTexture = ctx.getTex(thrusterSlice)

            ctx.useBatcher {
                it.drawQuad(
                    shipTexture,
                    x = position.x.toFloat(),
                    y = position.y.toFloat(),
                    width = PLAYER_SPRITE_WIDTH.toFloat(),
                    height = PLAYER_SPRITE_WIDTH.toFloat(),
                    m = globalMatrix
                )

                it.drawQuad(
                    thrusterTexture,
                    x = position.x.toFloat(),
                    y = position.y.toFloat(),
                    width = PLAYER_SPRITE_WIDTH.toFloat(),
                    height = PLAYER_SPRITE_WIDTH.toFloat(),
                    m = globalMatrix
                )
            }
        }
    }

    fun moveRight() {
        moveDirection = Direction.RIGHT
    }

    fun moveLeft() {
        moveDirection = Direction.LEFT
    }

    fun stop() {
        moveDirection = Direction.NONE
    }

    override fun getImageWidth(): Int {
        return PLAYER_SPRITE_WIDTH.toInt()
    }

    override fun getImageHeight(): Int {
        return PLAYER_SPRITE_WIDTH.toInt()
    }

    override fun isCollidingWith(collisionable: Collisionable): Boolean {
        return this.position.x < collisionable.position.x + collisionable.getImageWidth() &&
                this.position.x + this.getImageWidth() > collisionable.position.x &&
                this.position.y < collisionable.position.y + collisionable.getImageHeight() &&
                this.position.y + this.getImageHeight() > collisionable.position.y
    }

    fun hit() {
        if (livesCount - 1 >= 0) {
            livesCount -= 1
        }
    }

    fun restart() {
        livesCount = DEFAULT_LIVES
    }

    private fun getAnimationSlices(bitmapList: List<Bitmap>): List<BitmapSlice<Bitmap>> {
        val bitmapSlices = mutableListOf<BitmapSlice<Bitmap>>()
        for (bitmap in bitmapList) {
            bitmapSlices.add(bitmap.slice())
        }
        return bitmapSlices
    }

    fun activateShotPowerUp() {
        when (shotType) {
            ShotType.SINGLE -> shotType = ShotType.DOUBLE
            ShotType.DOUBLE -> shotType = ShotType.TRIPLE
            else -> {
            }
        }
        powerUpCoolDown()
    }

    private fun powerUpCoolDown() {
        /*Timer("PowerUpCoolDown", false).schedule(POWER_UP_COUNTDOWN_IN_MILLIS) {
          shotType = ShotType.SINGLE
        }*/
    }

    fun addExtraLive() {
        if (livesCount < DEFAULT_LIVES) {
            livesCount++
        }
    }

    companion object {
        const val ANIMATION_SPEED = 2
        const val THRUSTERS_OFFSET_X = 17
        const val THRUSTERS_OFFSET_Y = 38

        enum class Direction {
            LEFT,
            RIGHT,
            NONE
        }
    }
}

enum class ShotType {
    SINGLE,
    DOUBLE,
    TRIPLE
}
