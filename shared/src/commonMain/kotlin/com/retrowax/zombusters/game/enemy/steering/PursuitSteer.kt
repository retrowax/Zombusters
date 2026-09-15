package com.retrowax.zombusters.game.enemy.steering

import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2

// Port of legacy Pursuit.cs — predicts evader's future position and arrives there
class PursuitSteer(deceleration: ArriveSteer.Deceleration = ArriveSteer.Deceleration.FAST, decelerationDist: Float = 50f) {

    private val arrive = ArriveSteer(deceleration, decelerationDist)

    var target: Vec2? = null

    private var evaderPosition: Vec2 = Vec2.ZERO
    private var evaderVelocity: Vec2 = Vec2.ZERO
    private var evaderSpeed: Float = 0f
    private var evaderUpdated = false

    fun updateEvader(entity: SteeringEntity) {
        evaderPosition = entity.position.copy()
        evaderVelocity = entity.velocity.copy()
        evaderSpeed = entity.speed
        evaderUpdated = true
    }

    fun calculate(entity: SteeringEntity): Vec2 {
        val t = target ?: return Vec2.ZERO
        if (!evaderUpdated) return Vec2.ZERO
        evaderUpdated = false

        val toEvader = t - entity.position
        val lookAheadTime = toEvader.length() / (entity.maxSpeed / 2f + evaderSpeed)
        arrive.target = evaderPosition + evaderVelocity * lookAheadTime
        return arrive.calculate(entity)
    }
}
