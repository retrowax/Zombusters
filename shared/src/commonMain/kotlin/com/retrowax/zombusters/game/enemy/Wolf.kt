package com.retrowax.zombusters.game.enemy

import com.retrowax.zombusters.game.model.EnemyType
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.model.WOLF_MAX_VELOCITY

// Port of legacy Wolf.cs — MAX_VELOCITY 1.5, MAX_STRENGTH 0.15, scale 1.1
class Wolf(
    position: Vec2,
    boundingRadius: Float = 5f,
    life: Float = 1f,
    speed: Float = 0f,
    obstacles: List<ObstacleCircle> = emptyList()
) : BaseEnemy(EnemyType.WOLF, position, boundingRadius, MAX_STRENGTH, obstacles) {

    companion object {
        const val MAX_STRENGTH = 0.15f
        const val X_OFFSET = 20  // from legacy Wolf.cs
        const val Y_OFFSET = 48  // from legacy Wolf.cs
        const val SCALE = 1.1f   // from legacy Wolf.cs
    }

    init {
        entity.maxSpeed = WOLF_MAX_VELOCITY + speed
        entity.velocity = Vec2.ZERO
        lifecounter = life
    }

    override fun update(dtFactor: Float, playerPos: Vec2, playerEntity: SteeringEntity, allEnemies: List<BaseEnemy>) {
        if (status != ObjectStatus.ACTIVE) return

        steering.pursuit.target = playerPos
        steering.pursuit.updateEvader(playerEntity)
        val force = steering.update(entity)
        entity.velocity = (entity.velocity + force).truncate(entity.maxSpeed / 1.5f)
        entity.position = entity.position + entity.velocity * dtFactor

        resolveSeparation(allEnemies)
        isInPlayerRange = isInRange(playerPos)
        isLosingLife = false
    }
}
