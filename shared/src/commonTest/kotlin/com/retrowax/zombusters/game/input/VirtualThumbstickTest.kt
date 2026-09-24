package com.retrowax.zombusters.game.input

import com.retrowax.zombusters.game.enemy.Vec2
import kotlin.math.abs
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class VirtualThumbstickTest {

    private val leftRegion   = Vec2(200f, 360f)   // left half of 1280×720
    private val rightRegion  = Vec2(900f, 360f)   // right half

    // ── Basic lifecycle ──────────────────────────────────────────────────────

    @Test
    fun initialState_inactive() {
        val s = VirtualThumbstickState()
        assertFalse(s.active)
        assertEquals(-1, s.touchId)
        assertEquals(Vec2.ZERO, s.vector)
    }

    @Test
    fun touchDown_inCorrectHalf_activates() {
        val s = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, leftRegion, isLeftStick = true)
        assertTrue(s.active)
        assertEquals(0, s.touchId)
        assertEquals(leftRegion, s.center)
        assertEquals(Vec2.ZERO, s.vector)  // no displacement yet
    }

    @Test
    fun touchDown_inWrongHalf_doesNotActivate() {
        // Right-region touch should NOT claim left stick
        val s = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, rightRegion, isLeftStick = true)
        assertFalse(s.active)
    }

    @Test
    fun touchDown_whenAlreadyActive_ignored() {
        val existing = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, leftRegion, isLeftStick = true)
        val second = VirtualThumbstickLogic.onTouchDown(existing, 1, Vec2(100f, 360f), isLeftStick = true)
        assertEquals(0, second.touchId)   // first touch still owns it
    }

    @Test
    fun touchUp_matchingId_resets() {
        val active = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, leftRegion, isLeftStick = true)
        val released = VirtualThumbstickLogic.onTouchUp(active, 0)
        assertFalse(released.active)
        assertEquals(Vec2.ZERO, released.vector)
    }

    @Test
    fun touchUp_nonMatchingId_noChange() {
        val active = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, leftRegion, isLeftStick = true)
        val unchanged = VirtualThumbstickLogic.onTouchUp(active, 99)
        assertTrue(unchanged.active)
        assertEquals(0, unchanged.touchId)
    }

    // ── Vector math ──────────────────────────────────────────────────────────

    @Test
    fun displacement_within_deadZone_isZero() {
        var s = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, leftRegion, isLeftStick = true)
        val tinyDisplace = leftRegion + Vec2(VirtualThumbstickLogic.DEAD_ZONE * 0.5f, 0f)
        s = VirtualThumbstickLogic.onTouchMove(s, 0, tinyDisplace)
        assertEquals(Vec2.ZERO, s.vector)
    }

    @Test
    fun displacement_at_max_radius_produces_magnitude_one() {
        val center = leftRegion
        var s = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, center, isLeftStick = true)
        val atMax = center + Vec2(VirtualThumbstickLogic.maxRadius, 0f)
        s = VirtualThumbstickLogic.onTouchMove(s, 0, atMax)
        val m = s.vector.length()
        assertTrue(abs(m - 1f) < 0.01f, "expected magnitude ~1.0 at max radius, got $m")
    }

    @Test
    fun displacement_beyond_max_radius_clamped_to_one() {
        val center = leftRegion
        var s = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, center, isLeftStick = true)
        val beyond = center + Vec2(VirtualThumbstickLogic.maxRadius * 3f, 0f)
        s = VirtualThumbstickLogic.onTouchMove(s, 0, beyond)
        val m = s.vector.length()
        assertTrue(m <= 1.01f, "expected clamped magnitude ≤ 1.0, got $m")
    }

    @Test
    fun proportional_displacement_at_half_radius() {
        val center = leftRegion
        var s = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, center, isLeftStick = true)
        val halfMax = center + Vec2(VirtualThumbstickLogic.maxRadius / 2f, 0f)
        s = VirtualThumbstickLogic.onTouchMove(s, 0, halfMax)
        val v = s.vector
        assertTrue(abs(v.x - 0.5f) < 0.02f, "expected x ≈ 0.5, got ${v.x}")
        assertTrue(abs(v.y) < 0.01f, "expected y ≈ 0, got ${v.y}")
    }

    @Test
    fun upward_displacement_produces_negative_y_vector() {
        val center = leftRegion
        var s = VirtualThumbstickLogic.onTouchDown(VirtualThumbstickState(), 0, center, isLeftStick = true)
        // Moving up on screen = negative Y
        val up = center + Vec2(0f, -VirtualThumbstickLogic.maxRadius)
        s = VirtualThumbstickLogic.onTouchMove(s, 0, up)
        assertTrue(s.vector.y < -0.9f, "expected negative y for upward touch")
    }

    // ── Independent sticks ────────────────────────────────────────────────────

    @Test
    fun releasingLeftDoesNotAffectRight() {
        val dual = DualVirtualStickState()
        dual.onTouchDown(0, leftRegion)
        dual.onTouchDown(1, rightRegion)

        dual.onTouchMove(1, rightRegion + Vec2(VirtualThumbstickLogic.maxRadius, 0f))
        dual.onTouchUp(0)   // release left

        assertTrue(dual.right.active, "right stick should still be active")
        assertTrue(dual.right.vector.length() > 0.5f, "right vector should still be non-zero")
    }

    @Test
    fun releasingRightDoesNotAffectLeft() {
        val dual = DualVirtualStickState()
        dual.onTouchDown(0, leftRegion)
        dual.onTouchDown(1, rightRegion)

        dual.onTouchMove(0, leftRegion + Vec2(0f, VirtualThumbstickLogic.maxRadius))
        dual.onTouchUp(1)   // release right

        assertTrue(dual.left.active, "left stick should still be active")
        assertTrue(dual.left.vector.length() > 0.5f, "left vector should still be non-zero")
    }

    @Test
    fun simultaneousTwoFingerInteraction() {
        val dual = DualVirtualStickState()
        dual.onTouchDown(0, leftRegion)
        dual.onTouchDown(1, rightRegion)

        dual.onTouchMove(0, leftRegion + Vec2(VirtualThumbstickLogic.maxRadius, 0f))
        dual.onTouchMove(1, rightRegion + Vec2(-VirtualThumbstickLogic.maxRadius, 0f))

        assertTrue(dual.left.vector.x > 0.9f, "left should point right")
        assertTrue(dual.right.vector.x < -0.9f, "right should point left")
        assertTrue(dual.isFiring, "right stick beyond dead zone = firing")
    }

    @Test
    fun resetAll_clears_both_sticks() {
        val dual = DualVirtualStickState()
        dual.onTouchDown(0, leftRegion)
        dual.onTouchDown(1, rightRegion)
        dual.resetAll()

        assertFalse(dual.left.active)
        assertFalse(dual.right.active)
        assertEquals(Vec2.ZERO, dual.movementVector)
        assertEquals(Vec2.ZERO, dual.aimVector)
    }

    @Test
    fun rightStick_below_deadZone_notFiring() {
        val dual = DualVirtualStickState()
        dual.onTouchDown(0, rightRegion)
        // Tiny movement within dead zone
        dual.onTouchMove(0, rightRegion + Vec2(VirtualThumbstickLogic.DEAD_ZONE * 0.5f, 0f))
        assertFalse(dual.isFiring, "tiny stick within dead zone should not fire")
    }

    @Test
    fun touchInWrongHalf_notCapturedByWrongStick() {
        val dual = DualVirtualStickState()
        // Left-region touch should not activate right stick
        dual.onTouchDown(0, leftRegion)
        assertFalse(dual.right.active, "left-region touch must not activate right stick")
        assertTrue(dual.left.active, "left-region touch must activate left stick")
    }

    @Test
    fun pointerCancellation_releasesStick() {
        val dual = DualVirtualStickState()
        dual.onTouchDown(0, leftRegion)
        dual.onTouchMove(0, leftRegion + Vec2(50f, 0f))
        dual.onTouchUp(0)  // simulates cancel
        assertFalse(dual.left.active)
        assertEquals(Vec2.ZERO, dual.movementVector)
    }
}

