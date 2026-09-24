package com.retrowax.zombusters.game.input

/**
 * Active input mode.
 *
 * Used to select appropriate control rendering, hints, and How-To-Play content.
 * The active mode auto-detects from input activity:
 *   keyboard/mouse activity → KEYBOARD_MOUSE
 *   gamepad stick/button activity → GAMEPAD
 *   touch activity → TOUCH
 *
 * Default per platform:
 *   Desktop JVM → KEYBOARD_MOUSE
 *   Android     → TOUCH
 *   iOS         → TOUCH
 */
enum class InputMode {
    KEYBOARD_MOUSE,
    GAMEPAD,
    TOUCH;

    val isTouchMode get() = this == TOUCH
    val isKeyboardMode get() = this == KEYBOARD_MOUSE
    val isGamepadMode get() = this == GAMEPAD
}
