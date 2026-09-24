package com.retrowax.zombusters.game.input

import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import kotlin.math.min

/**
 * Virtual thumbstick state — pure immutable snapshot, no KorGE.
 *
 * Designed to be testable without any rendering dependency.
 */
data class VirtualThumbstickState(
    /** Whether a touch is currently active on this stick. */
    val active: Boolean = false,
    /** ID of the captured touch pointer. -1 = none. */
    val touchId: Int = -1,
    /** Dynamic center established at first touch, in logical coords. */
    val center: Vec2 = Vec2.ZERO,
    /** Current touch position, in logical coords. */
    val current: Vec2 = Vec2.ZERO,
) {
    /**
     * Normalized output vector [-1,1] × [-1,1].
     * This is the value fed into movement/aim systems.
     */
    val vector: Vec2
        get() = if (!active) Vec2.ZERO else {
            val displacement = current - center
            val length = displacement.length()
            if (length <= VirtualThumbstickLogic.DEAD_ZONE) {
                Vec2.ZERO
            } else {
                val clamped = displacement.truncate(VirtualThumbstickLogic.maxRadius)
                clamped * (1f / VirtualThumbstickLogic.maxRadius)
            }
        }

    /** Raw pixel displacement from center, clamped to maxRadius. */
    val clampedDisplacement: Vec2
        get() = if (!active) Vec2.ZERO else (current - center).truncate(VirtualThumbstickLogic.maxRadius)
}

/**
 * Virtual thumbstick logic — pure functions to update stick state.
 *
 * Legacy VirtualThumbsticks.cs (Windows Phone):
 *   maxThumbstickDistance = 60f  (in 800×480 logical space)
 *
 * Adapted for 1280×720 logical space:
 *   Scale factor: min(1280/800, 720/480) = min(1.6, 1.5) = 1.5
 *   Adapted radius = 60 * 1.5 = 90f → rounded to 100f for comfortable thumb travel.
 *   Dead zone = 8f (same relative proportion, ~0.08% of radius).
 *
 * Left stick: touch that starts in x < GAME_WIDTH/2 → movement.
 * Right stick: touch that starts in x >= GAME_WIDTH/2 → aim/fire.
 *
 * Y axis: NO inversion applied.
 * Legacy VirtualThumbsticks.cs did `l.Y = -l.Y` because the XNA game world had Y+ = up
 * (math convention). Our KorGE port uses screen coordinates (Y+ = down) throughout,
 * matching the touch screen, so displacement direction is already correct.
 * W key / stick-up → negative dy → player moves up on screen. ✓
 */
object VirtualThumbstickLogic {

    /**
     * Maximum displacement radius in logical pixels.
     * Legacy: 60f on 800×480. Scaled to 1280×720: ≈ 90-100f.
     */
    const val maxRadius: Float = 100f

    /**
     * Dead zone radius — touches closer than this to center produce zero output.
     * Prevents jitter and accidental firing from minor movement.
     */
    const val DEAD_ZONE: Float = 8f

    /**
     * X boundary in logical pixels: touches left of this belong to left stick,
     * touches right of this belong to right stick.
     */
    val splitX: Float get() = GAME_WIDTH / 2f

    /**
     * Process a new touch-down event. Returns updated stick state.
     * Only claims the touch if:
     *   - stick is not already active, AND
     *   - touch position is in the expected screen half.
     */
    fun onTouchDown(
        state: VirtualThumbstickState,
        touchId: Int,
        position: Vec2,
        isLeftStick: Boolean,
    ): VirtualThumbstickState {
        if (state.active) return state   // already captured
        val isInCorrectHalf = if (isLeftStick) position.x < splitX else position.x >= splitX
        if (!isInCorrectHalf) return state
        return VirtualThumbstickState(
            active = true,
            touchId = touchId,
            center = position,
            current = position,
        )
    }

    /**
     * Process touch-move event. Only updates if touch ID matches captured pointer.
     */
    fun onTouchMove(
        state: VirtualThumbstickState,
        touchId: Int,
        position: Vec2,
    ): VirtualThumbstickState {
        if (!state.active || state.touchId != touchId) return state
        return state.copy(current = position)
    }

    /**
     * Process touch-up/cancel event. Resets stick if touch ID matches.
     */
    fun onTouchUp(
        state: VirtualThumbstickState,
        touchId: Int,
    ): VirtualThumbstickState {
        if (!state.active || state.touchId != touchId) return state
        return VirtualThumbstickState()
    }

    /** Reset stick unconditionally (e.g., on pause or app background). */
    fun reset(): VirtualThumbstickState = VirtualThumbstickState()
}

/**
 * Mutable holder for both virtual sticks that scenes can hold.
 * Mutated by touch event handlers; read each frame in addUpdater.
 */
class DualVirtualStickState {
    var left: VirtualThumbstickState = VirtualThumbstickState()
    var right: VirtualThumbstickState = VirtualThumbstickState()

    val movementVector: Vec2 get() = left.vector
    val aimVector: Vec2 get() = right.vector

    /** True when right stick has meaningful aim beyond dead zone. */
    val isFiring: Boolean get() = right.vector.length() > 0.01f

    fun onTouchDown(touchId: Int, position: Vec2) {
        val newLeft = VirtualThumbstickLogic.onTouchDown(left, touchId, position, isLeftStick = true)
        if (newLeft !== left) { left = newLeft; return }
        val newRight = VirtualThumbstickLogic.onTouchDown(right, touchId, position, isLeftStick = false)
        if (newRight !== right) right = newRight
    }

    fun onTouchMove(touchId: Int, position: Vec2) {
        left = VirtualThumbstickLogic.onTouchMove(left, touchId, position)
        right = VirtualThumbstickLogic.onTouchMove(right, touchId, position)
    }

    fun onTouchUp(touchId: Int) {
        left = VirtualThumbstickLogic.onTouchUp(left, touchId)
        right = VirtualThumbstickLogic.onTouchUp(right, touchId)
    }

    fun resetAll() {
        left = VirtualThumbstickLogic.reset()
        right = VirtualThumbstickLogic.reset()
    }
}
