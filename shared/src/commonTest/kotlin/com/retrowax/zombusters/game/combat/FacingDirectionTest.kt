package com.retrowax.zombusters.game.combat

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.math.PI

class FacingDirectionTest {

    @Test
    fun angle_zero_classifiesAsNorth() {
        assertEquals(FacingDirection.N, FacingDirection.classify(0f))
    }

    @Test
    fun angle_piOver4_classifiesAsNorthEast() {
        assertEquals(FacingDirection.NE, FacingDirection.classify((PI / 4).toFloat()))
    }

    @Test
    fun angle_piOver2_classifiesAsEast() {
        assertEquals(FacingDirection.E, FacingDirection.classify((PI / 2).toFloat()))
    }

    @Test
    fun angle_3piOver4_classifiesAsSouthEast() {
        assertEquals(FacingDirection.SE, FacingDirection.classify((3 * PI / 4).toFloat()))
    }

    @Test
    fun angle_pi_classifiesAsSouth() {
        assertEquals(FacingDirection.S, FacingDirection.classify(PI.toFloat()))
    }

    @Test
    fun angle_negativePi_classifiesAsSouth() {
        assertEquals(FacingDirection.S, FacingDirection.classify((-PI).toFloat()))
    }

    @Test
    fun angle_negative3piOver4_classifiesAsSouthWest() {
        assertEquals(FacingDirection.SW, FacingDirection.classify((-3 * PI / 4).toFloat()))
    }

    @Test
    fun angle_negativepiOver2_classifiesAsWest() {
        assertEquals(FacingDirection.W, FacingDirection.classify((-PI / 2).toFloat()))
    }

    @Test
    fun angle_negativepiOver4_classifiesAsNorthWest() {
        assertEquals(FacingDirection.NW, FacingDirection.classify((-PI / 4).toFloat()))
    }

    @Test
    fun angle_justAboveNorthBoundary_classifiesAsNorthEast() {
        // North boundary is (-0.3925, 0.3925); 0.4 is just above it
        assertEquals(FacingDirection.NE, FacingDirection.classify(0.4f))
    }

    @Test
    fun angleFrom_straightNorth_givesNorthAngle() {
        val angle = FacingDirection.angleFrom(0f, -1f)
        assertEquals(FacingDirection.N, FacingDirection.classify(angle))
    }
}
