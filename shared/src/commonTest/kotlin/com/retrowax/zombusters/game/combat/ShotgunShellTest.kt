package com.retrowax.zombusters.game.combat

import com.retrowax.zombusters.game.model.PELLET_SPEED
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

private const val DELTA = 0.1f

class ShotgunShellTest {

    @Test
    fun north_centerPellet_movesStraightUp() {
        val shell = ShotgunShell(100f, 200f, 0f, 0f, FacingDirection.N)
        val (x, y) = shell.pelletPositionAt(1, 1f)
        assertEquals(100f, x, DELTA)
        assertEquals(200f - PELLET_SPEED, y, DELTA)
    }

    @Test
    fun north_leftPellet_hasNegativeXSpread() {
        val shell = ShotgunShell(100f, 200f, 0f, 0f, FacingDirection.N)
        val (centerX, _) = shell.pelletPositionAt(1, 1f)
        val (leftX, _) = shell.pelletPositionAt(0, 1f)
        assertTrue(leftX < centerX)
    }

    @Test
    fun east_centerPellet_movesStraightRight() {
        val angle = FacingDirection.angleFrom(1f, 0f)
        val shell = ShotgunShell(100f, 200f, 0f, angle, FacingDirection.E)
        val (x, y) = shell.pelletPositionAt(1, 1f)
        assertEquals(100f + PELLET_SPEED, x, DELTA)
        assertEquals(200f, y, DELTA)
    }

    @Test
    fun south_centerPellet_movesStraightDown() {
        val angle = FacingDirection.angleFrom(0f, 1f)
        val shell = ShotgunShell(100f, 200f, 0f, angle, FacingDirection.S)
        val (x, y) = shell.pelletPositionAt(1, 1f)
        assertEquals(100f, x, DELTA)
        assertEquals(200f + PELLET_SPEED, y, DELTA)
    }

    @Test
    fun isPelletHitting_atEnemyPos_returnsTrue() {
        val shell = ShotgunShell(500f, 300f, 0f, 0f, FacingDirection.N)
        // At t=firedAt, center pellet is at startX, startY (d=0)
        assertTrue(shell.isPelletHitting(500f, 300f, 500f, 300f))
    }
}
