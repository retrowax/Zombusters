package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.enemy.BaseEnemy
import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.model.ObjectStatus

// Coordinates per-frame enemy updates and contact detection
class EnemySystem {

    val enemies: MutableList<BaseEnemy> = mutableListOf()

    fun addEnemies(newEnemies: List<BaseEnemy>) {
        enemies.addAll(newEnemies)
    }

    fun clear() {
        enemies.clear()
    }

    val activeCount: Int get() = enemies.count { it.status == ObjectStatus.ACTIVE }

    // dt in seconds; dtFactor = dt * 60 scales per-frame physics to match 60fps original
    fun update(dtSecs: Float, playerPos: Vec2, playerEntity: SteeringEntity) {
        val dtFactor = (dtSecs * 60f).coerceAtMost(3f)
        for (enemy in enemies) {
            if (enemy.status == ObjectStatus.ACTIVE) {
                enemy.update(dtFactor, playerPos, playerEntity, enemies)
            }
        }
    }

    // Returns enemies within contact range of playerPos
    fun enemiesInContactRange(playerPos: Vec2, crashRadius: Float = 10f): List<BaseEnemy> =
        enemies.filter { it.status == ObjectStatus.ACTIVE && it.isInRange(playerPos, crashRadius) }
}
