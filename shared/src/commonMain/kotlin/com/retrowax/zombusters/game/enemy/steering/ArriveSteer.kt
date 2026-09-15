package com.retrowax.zombusters.game.enemy.steering

import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2
import kotlin.math.min

// Port of legacy Arrive.cs — slows to a stop as it reaches the target
class ArriveSteer(
    private val deceleration: Deceleration = Deceleration.NORMAL,
    private val decelerationBeginsAtDistance: Float = 10f
) {
    enum class Deceleration(val value: Int) { SLOW(3), NORMAL(2), FAST(1) }

    var target: Vec2? = null

    fun calculate(entity: SteeringEntity): Vec2 {
        val t = target ?: return Vec2.ZERO
        val toTarget = t - entity.position
        val distance = toTarget.length()
        if (distance < 0.001f) return Vec2.ZERO

        val desiredVelocity = if (distance <= decelerationBeginsAtDistance) {
            val decelerationTweaker = 90.3f
            val speed = min(distance / (deceleration.value * decelerationTweaker), entity.maxSpeed)
            toTarget * (speed / distance)
        } else {
            toTarget.normalize() * entity.maxSpeed
        }

        return desiredVelocity - entity.velocity
    }
}
