package com.retrowax.zombusters.game.combat

import com.retrowax.zombusters.game.model.COLLISION_DISTANCE
import com.retrowax.zombusters.game.model.PELLET_SPEED
import kotlin.math.cos
import kotlin.math.sin
import kotlin.math.sqrt

private const val PELLET_SPREAD_ANGLE_90 = 0.09f
private const val PELLET_SPREAD_ANGLE_45 = 1.2f

data class ShotgunShell(
    val startX: Float,
    val startY: Float,
    val firedAt: Float,
    val angle: Float,
    val direction: FacingDirection = FacingDirection.classify(angle),
    val shooterIndex: Int = 0
) {
    fun pelletPositionAt(pelletIndex: Int, totalSec: Float): Pair<Float, Float> {
        val t = totalSec - firedAt
        val d = PELLET_SPEED * t
        return when (direction) {
            FacingDirection.N -> {
                val x = when (pelletIndex) {
                    0 -> startX - d * PELLET_SPREAD_ANGLE_90
                    2 -> startX + d * PELLET_SPREAD_ANGLE_90
                    else -> startX
                }
                Pair(x, startY - d)
            }
            FacingDirection.NE -> when (pelletIndex) {
                0 -> Pair(startX + sin(angle) + d * PELLET_SPREAD_ANGLE_45, startY - cos(angle) - d)
                1 -> Pair(startX + sin(angle) + d, startY - cos(angle) - d)
                2 -> Pair(startX + sin(angle) + d, startY - cos(angle) - d * PELLET_SPREAD_ANGLE_45)
                else -> Pair(startX + sin(angle) + d, startY - cos(angle) - d)
            }
            FacingDirection.E -> {
                val y = when (pelletIndex) {
                    0 -> startY - d * PELLET_SPREAD_ANGLE_90
                    2 -> startY + d * PELLET_SPREAD_ANGLE_90
                    else -> startY
                }
                Pair(startX + d, y)
            }
            FacingDirection.SE -> when (pelletIndex) {
                0 -> Pair(startX + sin(angle) + d * PELLET_SPREAD_ANGLE_45, startY + cos(angle) + d)
                1 -> Pair(startX + sin(angle) + d, startY + cos(angle) + d)
                2 -> Pair(startX + sin(angle) + d, startY + cos(angle) + d * PELLET_SPREAD_ANGLE_45)
                else -> Pair(startX + sin(angle) + d, startY + cos(angle) + d)
            }
            FacingDirection.S -> {
                val x = when (pelletIndex) {
                    0 -> startX - d * PELLET_SPREAD_ANGLE_90
                    2 -> startX + d * PELLET_SPREAD_ANGLE_90
                    else -> startX
                }
                Pair(x, startY + d)
            }
            FacingDirection.SW -> when (pelletIndex) {
                0 -> Pair(startX - sin(angle) - d, startY + cos(angle) + d * PELLET_SPREAD_ANGLE_45)
                1 -> Pair(startX - sin(angle) - d, startY + cos(angle) + d)
                2 -> Pair(startX - sin(angle) - d * PELLET_SPREAD_ANGLE_45, startY + cos(angle) + d)
                else -> Pair(startX - sin(angle) - d, startY + cos(angle) + d)
            }
            FacingDirection.W -> {
                val y = when (pelletIndex) {
                    0 -> startY - d * PELLET_SPREAD_ANGLE_90
                    2 -> startY + d * PELLET_SPREAD_ANGLE_90
                    else -> startY
                }
                Pair(startX - d, y)
            }
            FacingDirection.NW -> when (pelletIndex) {
                0 -> Pair(startX - sin(angle) - d, startY - cos(angle) - d * PELLET_SPREAD_ANGLE_45)
                1 -> Pair(startX - sin(angle) - d, startY - cos(angle) - d)
                2 -> Pair(startX - sin(angle) - d * PELLET_SPREAD_ANGLE_45, startY - cos(angle) - d)
                else -> Pair(startX - sin(angle) - d, startY - cos(angle) - d)
            }
        }
    }

    fun isOutOfBounds(x: Float, y: Float): Boolean =
        x < -10f || x > 1290f || y < -10f || y > 730f

    fun isPelletHitting(x: Float, y: Float, enemyX: Float, enemyY: Float): Boolean {
        val dx = x - enemyX
        val dy = y - enemyY
        return sqrt(dx * dx + dy * dy) < COLLISION_DISTANCE
    }
}
