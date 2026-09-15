package com.retrowax.zombusters.game.enemy.steering

import com.retrowax.zombusters.game.enemy.ObstacleCircle
import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2

// Port of legacy ObstacleAvoidance.cs — circle obstacles only (wall avoidance omitted)
class ObstacleSteer(
    private val minDetectionBoxLength: Float = 15f,
    private val obstacles: List<ObstacleCircle>
) {
    fun calculate(entity: SteeringEntity): Vec2 {
        if (obstacles.isEmpty()) return Vec2.ZERO

        val minDistToCollision = 0.050f * entity.speed
        val detectionBoxLength = minDetectionBoxLength +
            (entity.speed / entity.maxSpeed.coerceAtLeast(0.001f)) * minDetectionBoxLength

        var nearestDist = Float.MAX_VALUE
        var nearestSteering = Vec2.ZERO
        var nearestObstacle: ObstacleCircle? = null

        for (obstacle in obstacles) {
            val minDistToCenter = minDistToCollision + obstacle.radius
            val totalRadius = obstacle.radius + entity.boundingRadius
            val localOffset = obstacle.center - entity.position

            val forwardComponent = localOffset.dot(entity.heading)
            val forwardOffset = entity.heading * forwardComponent
            val offForwardOffset = localOffset - forwardOffset

            val inCylinder = offForwardOffset.length() < totalRadius
            val nearby = forwardComponent < minDistToCenter
            val inFront = forwardComponent > 0

            if (inCylinder || inFront || nearby) {
                val length = (-offForwardOffset).length()
                if (length < nearestDist) {
                    nearestDist = length
                    nearestSteering = -offForwardOffset
                    nearestObstacle = obstacle
                }
            }
        }

        nearestObstacle?.let { obs ->
            enforceNonPenetration(entity, obs)
        }

        return nearestSteering
    }

    private fun enforceNonPenetration(entity: SteeringEntity, obstacle: ObstacleCircle) {
        val toEntity = entity.position - obstacle.center
        val dist = toEntity.length()
        val overlap = entity.boundingRadius + obstacle.radius - dist
        if (overlap >= 0 && dist > 0f) {
            entity.position = entity.position + (toEntity * (1f / dist)) * overlap
        }
    }
}
