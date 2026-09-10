package com.retrowax.zombusters.game.input

import kotlin.math.sqrt

data class InputState(
    val moveX: Float = 0f,
    val moveY: Float = 0f,
    val firePressed: Boolean = false,
    val pausePressed: Boolean = false
) {
    val isMoving: Boolean get() = moveX != 0f || moveY != 0f

    fun normalizedMove(): Pair<Float, Float> {
        if (moveX == 0f && moveY == 0f) return 0f to 0f
        val len = sqrt(moveX * moveX + moveY * moveY)
        return moveX / len to moveY / len
    }
}
