package com.steelvectors.app.feature.game.entities.enemies

import com.steelvectors.app.feature.game.base.Collisionable
import com.steelvectors.app.feature.game.base.GameplayState
import com.steelvectors.app.feature.game.entities.Bullet
import com.steelvectors.app.feature.game.entities.BulletDirection
import com.steelvectors.app.feature.game.entities.Enemy
import com.steelvectors.app.feature.game.entities.EnemyStatus
import com.steelvectors.app.feature.game.entities.EnemyType
import korlibs.image.bitmap.Bitmap
import korlibs.image.bitmap.slice
import korlibs.korge.internal.KorgeInternal
import korlibs.korge.render.RenderContext
import korlibs.korge.view.Container
import korlibs.math.geom.Point

class Boss(
    private val bossType: BossType,
    override var position: Point,
    image: Bitmap,
    private val bulletBitmap: Bitmap,
    private val bullets: MutableList<Bullet>,
    private val container: Container,
    //private val soundPool: SoundPool?,
    private val phaserSound: Int?,
    screenWidth: Float,
    screenHeight: Float
) : Enemy(screenWidth, screenHeight) {
    private val imageSlice = image.slice()
    override var status = EnemyStatus.ALIVE
    override val type = EnemyType.BOSS
    override var strength =
        if (bossType.ordinal > 0) DEFAULT_STRENGTH * bossType.ordinal else DEFAULT_STRENGTH
    private var frame = 0
    private var direction = Direction.RIGHT

    override fun update(screenWidth: Float, screenHeight: Float) {
        if (currentState == GameplayState.Playing) {
            frame++

            if (bossType != null) {
                when (bossType) {
                    BossType.A -> updateBossA()
                    BossType.B -> updateBossB()
                    BossType.C -> updateBossC()
                    BossType.D -> updateBossD()
                    BossType.E -> updateBossE()
                    BossType.F -> updateBossF()
                    BossType.G -> updateBossG()
                }
            }
        }
    }

    private fun updateBossA() {
        if (direction == Direction.RIGHT) {
            this.position = Point(this.position.x + SPEED, this.position.y)
        } else {
            this.position = Point(this.position.x - SPEED, this.position.y)
        }

        if (position.x > SCREEN_RIGHT_OFFSET) {
            direction = Direction.LEFT
        }
        if (position.x < SCREEN_LEFT_OFFSET) {
            direction = Direction.RIGHT
        }

        if (frame.rem(120) == 0) {
            val bullet =
                Bullet(Point(position.x, position.y + 110), bulletBitmap, BulletDirection.DOWN)
            bullets.add(bullet)
            container.addChild(bullet)
            //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
        }
    }

    private fun updateBossB() {
        if (direction == Direction.RIGHT) {
            this.position = Point(this.position.x + SPEED * 2, this.position.y)
        } else {
            this.position = Point(this.position.x - SPEED, this.position.y)
        }

        if (position.x > SCREEN_RIGHT_OFFSET) {
            direction = Direction.LEFT
        }
        if (position.x < SCREEN_LEFT_OFFSET) {
            direction = Direction.RIGHT
        }

        if (frame.rem(100) == 0) {
            val bullet1 =
                Bullet(Point(position.x - 10, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            val bullet2 =
                Bullet(Point(position.x + 10, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            bullets.add(bullet1)
            bullets.add(bullet2)
            container.addChild(bullet1)
            container.addChild(bullet2)
            //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
        }
    }

    private fun updateBossC() {
        if (direction == Direction.RIGHT) {
            this.position = Point(this.position.x + SPEED, this.position.y)
        } else {
            this.position = Point(this.position.x - SPEED * 2, this.position.y)
        }

        if (position.x > SCREEN_RIGHT_OFFSET_LARGE_SHIP) {
            direction = Direction.LEFT
        }
        if (position.x < SCREEN_LEFT_OFFSET) {
            direction = Direction.RIGHT
        }

        if (frame.rem(100) == 0) {
            val bullet1 =
                Bullet(Point(position.x - 20, position.y + 80), bulletBitmap, BulletDirection.DOWN)
            val bullet2 =
                Bullet(Point(position.x, position.y + 80), bulletBitmap, BulletDirection.DOWN)
            val bullet3 =
                Bullet(Point(position.x + 20, position.y + 80), bulletBitmap, BulletDirection.DOWN)
            bullets.add(bullet1)
            bullets.add(bullet2)
            bullets.add(bullet3)
            container.addChild(bullet1)
            container.addChild(bullet2)
            container.addChild(bullet3)
            //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
        }
    }

    private fun updateBossD() {
        if (direction == Direction.RIGHT) {
            this.position = Point(this.position.x + SPEED * 2, this.position.y)
        } else {
            this.position = Point(this.position.x - SPEED * 2, this.position.y)
        }

        if (position.x > SCREEN_RIGHT_OFFSET_LARGE_SHIP) {
            direction = Direction.LEFT
        }
        if (position.x < SCREEN_LEFT_OFFSET) {
            direction = Direction.RIGHT
        }

        if (frame.rem(80) == 0) {
            val bullet1 =
                Bullet(Point(position.x + 20, position.y + 80), bulletBitmap, BulletDirection.DOWN)
            val bullet2 =
                Bullet(Point(position.x + 40, position.y + 80), bulletBitmap, BulletDirection.DOWN)
            bullets.add(bullet1)
            bullets.add(bullet2)
            container.addChild(bullet1)
            container.addChild(bullet2)
            //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
        }
    }

    private fun updateBossE() {
        if (direction == Direction.RIGHT) {
            this.position = Point(this.position.x + SPEED, this.position.y)
        } else {
            this.position = Point(this.position.x - SPEED, this.position.y)
        }

        if (position.x > SCREEN_RIGHT_OFFSET_LARGE_SHIP) {
            direction = Direction.LEFT
        }
        if (position.x < SCREEN_LEFT_OFFSET) {
            direction = Direction.RIGHT
        }

        if (frame.rem(80) == 0) {
            val bullet1 =
                Bullet(Point(position.x - 20, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            val bullet2 =
                Bullet(Point(position.x, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            val bullet3 =
                Bullet(Point(position.x + 20, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            bullets.add(bullet1)
            bullets.add(bullet2)
            bullets.add(bullet3)
            container.addChild(bullet1)
            container.addChild(bullet2)
            container.addChild(bullet3)
            //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
        }
    }

    private fun updateBossF() {
        if (direction == Direction.RIGHT) {
            this.position = Point(this.position.x + SPEED * 2, this.position.y)
        } else {
            this.position = Point(this.position.x - SPEED, this.position.y)
        }

        if (position.x > SCREEN_RIGHT_OFFSET) {
            direction = Direction.LEFT
        }
        if (position.x < SCREEN_LEFT_OFFSET) {
            direction = Direction.RIGHT
        }

        if (frame.rem(80) == 0) {
            val bullet1 =
                Bullet(Point(position.x - 20, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            val bullet2 =
                Bullet(Point(position.x, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            val bullet3 =
                Bullet(Point(position.x + 20, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            bullets.add(bullet1)
            bullets.add(bullet2)
            bullets.add(bullet3)
            container.addChild(bullet1)
            container.addChild(bullet2)
            container.addChild(bullet3)
            //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
        }
    }

    private fun updateBossG() {
        if (direction == Direction.RIGHT) {
            this.position = Point(this.position.x + SPEED * 2, this.position.y)
        } else {
            this.position = Point(this.position.x - SPEED * 2, this.position.y)
        }

        if (position.x > SCREEN_RIGHT_OFFSET_LARGE_SHIP) {
            direction = Direction.LEFT
        }
        if (position.x < SCREEN_LEFT_OFFSET) {
            direction = Direction.RIGHT
        }

        if (frame.rem(60) == 0) {
            val bullet1 =
                Bullet(Point(position.x - 20, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            val bullet2 =
                Bullet(Point(position.x, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            val bullet3 =
                Bullet(Point(position.x + 20, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            val bullet4 =
                Bullet(Point(position.x + 40, position.y + 100), bulletBitmap, BulletDirection.DOWN)
            bullets.add(bullet1)
            bullets.add(bullet2)
            bullets.add(bullet3)
            bullets.add(bullet4)
            container.addChild(bullet1)
            container.addChild(bullet2)
            container.addChild(bullet3)
            container.addChild(bullet4)
            //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
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
        const val SPEED = 1
        const val DEFAULT_STRENGTH = 20
        const val SCREEN_RIGHT_OFFSET = 270
        const val SCREEN_RIGHT_OFFSET_LARGE_SHIP = 220
        const val SCREEN_LEFT_OFFSET = 0

        enum class Direction {
            LEFT,
            RIGHT
        }
    }
}

enum class BossType {
    A,
    B,
    C,
    D,
    E,
    F,
    G
}
