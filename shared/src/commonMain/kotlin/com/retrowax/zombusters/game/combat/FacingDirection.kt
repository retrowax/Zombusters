package com.retrowax.zombusters.game.combat

import kotlin.math.atan2

enum class FacingDirection {
    N, NE, E, SE, S, SW, W, NW;

    companion object {
        fun classify(angle: Float): FacingDirection = when {
            angle > -0.3925f && angle < 0.3925f -> N
            angle > 0.3925f && angle < 1.1775f -> NE
            angle > 1.1775f && angle < 1.9625f -> E
            angle > 1.9625f && angle < 2.7275f -> SE
            angle > 2.7275f || angle < -2.7275f -> S
            angle < -1.9625f && angle > -2.7275f -> SW
            angle < -1.1775f && angle > -1.9625f -> W
            angle < -0.3925f && angle > -1.1775f -> NW
            else -> S
        }

        fun angleFrom(dx: Float, dy: Float): Float = atan2(dx, -dy)
    }
}
