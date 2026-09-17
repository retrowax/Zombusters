package com.retrowax.zombusters.game.combat

import com.retrowax.zombusters.game.model.BULLET_SPEED
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

private const val DELTA = 0.01f

class ProjectilePositionTest {

    @Test
    fun northBullet_movesUpward_afterOneSecond() {
        val p = Projectile(100f, 200f, 0f, 0f, FacingDirection.N, BULLET_SPEED)
        val (x, y) = p.positionAt(1f)
        assertEquals(100f, x, DELTA)
        assertEquals(200f - BULLET_SPEED, y, DELTA)
    }

    @Test
    fun southBullet_movesDownward_afterOneSecond() {
        val angle = FacingDirection.angleFrom(0f, 1f)
        val p = Projectile(100f, 200f, 0f, angle, FacingDirection.S, BULLET_SPEED)
        val (x, y) = p.positionAt(1f)
        assertEquals(100f, x, DELTA)
        assertEquals(200f + BULLET_SPEED, y, DELTA)
    }

    @Test
    fun eastBullet_movesRight_afterOneSecond() {
        val angle = FacingDirection.angleFrom(1f, 0f)
        val p = Projectile(100f, 200f, 0f, angle, FacingDirection.E, BULLET_SPEED)
        val (x, y) = p.positionAt(1f)
        assertEquals(100f + BULLET_SPEED, x, DELTA)
    }

    @Test
    fun westBullet_movesLeft_afterOneSecond() {
        val angle = FacingDirection.angleFrom(-1f, 0f)
        val p = Projectile(100f, 200f, 0f, angle, FacingDirection.W, BULLET_SPEED)
        val (x, y) = p.positionAt(1f)
        assertEquals(100f - BULLET_SPEED, x, DELTA)
    }

    @Test
    fun atFiredTime_positionEqualsStart_forNorth() {
        val p = Projectile(300f, 400f, 5f, 0f, FacingDirection.N, BULLET_SPEED)
        val (x, y) = p.positionAt(5f)
        assertEquals(300f, x, DELTA)
        assertEquals(400f, y, DELTA)
    }

    @Test
    fun bulletOutOfBounds_leftEdge() {
        val p = Projectile(0f, 360f, 0f, 0f, FacingDirection.W, BULLET_SPEED)
        assertTrue(p.isOutOfBounds(-20f, 360f))
    }

    @Test
    fun bulletNotOutOfBounds_midScreen() {
        val p = Projectile(640f, 360f, 0f, 0f, FacingDirection.N, BULLET_SPEED)
        assertFalse(p.isOutOfBounds(640f, 360f))
    }

    @Test
    fun isHitting_bulletAtEnemyPos_returnsTrue() {
        val p = Projectile(500f, 300f, 0f, 0f, FacingDirection.N, BULLET_SPEED)
        assertTrue(p.isHitting(500f, 300f, 500f, 300f))
    }

    @Test
    fun isHitting_bulletFarFromEnemy_returnsFalse() {
        val p = Projectile(500f, 300f, 0f, 0f, FacingDirection.N, BULLET_SPEED)
        assertFalse(p.isHitting(500f, 300f, 600f, 400f))
    }
}
