package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.enemy.BaseEnemy
import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.model.Avatar
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
    fun update(dtSecs: Float, playerPos: Vec2, playerEntity: SteeringEntity, totalSec: Float = 0f) {
        val dtFactor = (dtSecs * 60f).coerceAtMost(3f)
        for (enemy in enemies) {
            when (enemy.status) {
                ObjectStatus.ACTIVE -> enemy.update(dtFactor, playerPos, playerEntity, enemies)
                ObjectStatus.DYING -> {
                    // Legacy: death animation lasts 1.2s then enemy is removed
                    if (totalSec > 0f && totalSec > enemy.deathTimeTotalSeconds + ENEMY_DEATH_TIME) {
                        enemy.status = ObjectStatus.INACTIVE
                    }
                }
                else -> Unit
            }
        }
    }

    companion object {
        const val ENEMY_DEATH_TIME = 1.2f
    }

    // Multi-player overload: each enemy targets the nearest living player
    fun update(
        dtSecs: Float,
        players: List<Avatar>,
        playerSteeringEntities: List<SteeringEntity>,
        totalSec: Float = 0f
    ) {
        val dtFactor = (dtSecs * 60f).coerceAtMost(3f)
        for (enemy in enemies) {
            when (enemy.status) {
                ObjectStatus.ACTIVE -> {
                    val target = nearestLivingPlayer(players, enemy)
                    if (target != null) {
                        val idx = players.indexOf(target).coerceAtLeast(0)
                        val steering = playerSteeringEntities.getOrElse(idx) {
                            SteeringEntity(Vec2(target.position.x.toFloat(), target.position.y.toFloat()))
                        }
                        val targetPos = Vec2(target.position.x.toFloat(), target.position.y.toFloat())
                        enemy.update(dtFactor, targetPos, steering, enemies)
                    }
                }
                ObjectStatus.DYING -> {
                    if (totalSec > 0f && totalSec > enemy.deathTimeTotalSeconds + ENEMY_DEATH_TIME) {
                        enemy.status = ObjectStatus.INACTIVE
                    }
                }
                else -> Unit
            }
        }
    }

    private fun nearestLivingPlayer(players: List<Avatar>, enemy: BaseEnemy): Avatar? {
        val active = players.filter { it.status == ObjectStatus.ACTIVE }
        if (active.isEmpty()) return null
        val ex = enemy.entity.position.x
        val ey = enemy.entity.position.y
        return active.minByOrNull { p ->
            val dx = p.position.x.toFloat() - ex
            val dy = p.position.y.toFloat() - ey
            dx * dx + dy * dy
        }
    }

    // Returns enemies within contact range of playerPos
    fun enemiesInContactRange(playerPos: Vec2, crashRadius: Float = 10f): List<BaseEnemy> =
        enemies.filter { it.status == ObjectStatus.ACTIVE && it.isInRange(playerPos, crashRadius) }
}
