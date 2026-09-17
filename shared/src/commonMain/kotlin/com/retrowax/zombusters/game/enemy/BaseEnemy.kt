package com.retrowax.zombusters.game.enemy

import com.retrowax.zombusters.game.enemy.steering.SteeringSystem
import com.retrowax.zombusters.game.model.EnemyType
import com.retrowax.zombusters.game.model.ObjectStatus

// Port of legacy BaseEnemy.cs — pure domain model, no KorGE View inheritance
abstract class BaseEnemy(
    val type: EnemyType,
    initialPosition: Vec2,
    boundingRadius: Float,
    val maxStrength: Float,
    obstacles: List<ObstacleCircle>
) {
    val entity = SteeringEntity(initialPosition, boundingRadius = boundingRadius)
    val steering = SteeringSystem(maxStrength, obstacles)

    var status: ObjectStatus = ObjectStatus.ACTIVE
    var deathTimeTotalSeconds: Float = 0f
    var lifecounter: Float = 1f
    var isLosingLife: Boolean = false
    var playerChased: Int = 0

    // True when within contact range of the chased player
    var isInPlayerRange: Boolean = false

    val isActive: Boolean get() = status == ObjectStatus.ACTIVE

    fun destroy(totalGameSeconds: Float) {
        deathTimeTotalSeconds = totalGameSeconds
        status = ObjectStatus.DYING
    }

    fun crash(totalGameSeconds: Float) {
        deathTimeTotalSeconds = totalGameSeconds
        status = ObjectStatus.INACTIVE
    }

    // Returns true if this enemy is within contact range of playerPos
    fun isInRange(playerPos: Vec2, crashRadius: Float = 20f): Boolean {
        return entity.position.distanceTo(playerPos) < crashRadius + 20f
    }

    // Separation push-away from other active enemies
    fun resolveSeparation(others: List<BaseEnemy>) {
        for (other in others) {
            if (other === this) continue
            if (other.status != ObjectStatus.ACTIVE) continue
            val toSelf = entity.position - other.entity.position
            val dist = toSelf.length()
            if (dist < 0.001f) continue
            val overlap = entity.boundingRadius + 20f - dist
            if (overlap >= 0f) {
                entity.position = entity.position + (toSelf * (1f / dist)) * overlap
            }
        }
    }

    // Called each frame while ACTIVE; concrete types override to apply steering
    abstract fun update(dtFactor: Float, playerPos: Vec2, playerEntity: SteeringEntity, allEnemies: List<BaseEnemy>)
}
