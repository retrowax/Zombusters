package com.retrowax.zombusters.game.combat

import com.retrowax.zombusters.game.model.BULLET_SPEED
import com.retrowax.zombusters.game.model.COLLISION_DISTANCE
import kotlin.math.cos
import kotlin.math.sin
import kotlin.math.sqrt

data class Projectile(
    val startX: Float,
    val startY: Float,
    val firedAt: Float,
    val angle: Float,
    val direction: FacingDirection = FacingDirection.classify(angle),
    val speed: Float = BULLET_SPEED
) {
    fun positionAt(totalSec: Float): Pair<Float, Float> {
        val t = totalSec - firedAt
        val d = speed * t
        return when (direction) {
            FacingDirection.N -> Pair(startX, startY - d)
            FacingDirection.NE -> Pair(startX + sin(angle) + d, startY - cos(angle) - d)
            FacingDirection.E -> Pair(startX + d, startY)
            FacingDirection.SE -> Pair(startX + sin(angle) + d, startY + cos(angle) + d)
            FacingDirection.S -> Pair(startX, startY + d)
            FacingDirection.SW -> Pair(startX - sin(angle) - d, startY + cos(angle) + d)
            FacingDirection.W -> Pair(startX - d, startY)
            FacingDirection.NW -> Pair(startX - sin(angle) - d, startY - cos(angle) - d)
        }
    }

    fun isOutOfBounds(x: Float, y: Float): Boolean =
        x < -10f || x > 1290f || y < -10f || y > 730f

    fun isHitting(x: Float, y: Float, enemyX: Float, enemyY: Float): Boolean {
        val dx = x - enemyX
        val dy = y - enemyY
        return sqrt(dx * dx + dy * dy) < COLLISION_DISTANCE
    }
}
