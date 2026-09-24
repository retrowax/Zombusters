package com.retrowax.zombusters.game.input

import com.retrowax.zombusters.game.enemy.Vec2

/**
 * Platform-neutral single-frame gameplay input.
 *
 * GameplayScene consumes this; it does NOT care whether
 * it originated from keyboard, gamepad, or touch.
 *
 * movement: normalized [-1,1] × [-1,1], pointing in movement direction.
 * aim:      normalized [-1,1] × [-1,1], pointing toward aim target.
 * firing:   true while the player holds fire (right stick out / space held / right mouse held).
 *
 * Debug keys remain in GameplayScene directly (F1, F2, K, G, H) — developer tooling only.
 */
data class GameInput(
    val movement: Vec2 = Vec2.ZERO,
    val aim: Vec2 = Vec2.ZERO,
    val firing: Boolean = false,
    val pauseJustPressed: Boolean = false,
    val confirmJustPressed: Boolean = false,
    val cancelJustPressed: Boolean = false,
    val weaponNextJustPressed: Boolean = false,
) {
    companion object {
        val EMPTY = GameInput()
    }
}

/**
 * Platform-neutral single-frame menu input.
 *
 * Used by all menu/UI scenes instead of direct keyboard state.
 * Pointer position is in logical 1280×720 coordinates.
 */
data class MenuInput(
    val up: Boolean = false,
    val down: Boolean = false,
    val left: Boolean = false,
    val right: Boolean = false,
    val confirm: Boolean = false,
    val cancel: Boolean = false,
    /** Most recent pointer position in logical viewport coords, or null if no pointer. */
    val pointerPos: Vec2? = null,
    /** true if pointer was just pressed/tapped this frame */
    val pointerJustPressed: Boolean = false,
    /** true if pointer was just released this frame */
    val pointerJustReleased: Boolean = false,
) {
    companion object {
        val EMPTY = MenuInput()
    }
}

