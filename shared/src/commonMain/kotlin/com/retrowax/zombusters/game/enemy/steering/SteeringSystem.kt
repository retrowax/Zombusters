package com.retrowax.zombusters.game.enemy.steering

import com.retrowax.zombusters.game.enemy.ObstacleCircle
import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.model.STEERING_WEIGHT_OBSTACLE_AVOIDANCE
import com.retrowax.zombusters.game.model.STEERING_WEIGHT_PURSUIT

// Port of legacy SteeringBehaviors.cs — prioritized combination (obstacle avoidance first, then pursuit)
class SteeringSystem(
    private val maxForce: Float,
    obstacles: List<ObstacleCircle>
) {
    val pursuit = PursuitSteer()
    private val obstacleSteer = ObstacleSteer(obstacles = obstacles)

    // Prioritized accumulation: returns false if budget exhausted
    private fun accumulateForce(running: Vec2, forceToAdd: Vec2): Pair<Vec2, Boolean> {
        val magnitudeSoFar = running.length()
        val magnitudeRemaining = maxForce - magnitudeSoFar
        if (magnitudeRemaining <= 0f) return Pair(running, false)

        val magnitudeToAdd = forceToAdd.length()
        return if (magnitudeToAdd < magnitudeRemaining) {
            Pair(running + forceToAdd, true)
        } else {
            Pair(running + forceToAdd.normalize() * magnitudeRemaining, true)
        }
    }

    fun update(entity: SteeringEntity): Vec2 {
        var steeringForce = Vec2.ZERO

        val obstacleForce = obstacleSteer.calculate(entity) * STEERING_WEIGHT_OBSTACLE_AVOIDANCE
        val (f1, cont1) = accumulateForce(steeringForce, obstacleForce)
        steeringForce = f1
        if (!cont1) return steeringForce

        val pursuitForce = pursuit.calculate(entity) * STEERING_WEIGHT_PURSUIT
        val (f2, _) = accumulateForce(steeringForce, pursuitForce)
        steeringForce = f2

        return steeringForce
    }
}
