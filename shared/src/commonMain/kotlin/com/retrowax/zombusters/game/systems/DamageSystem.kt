package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.combat.CombatState
import com.retrowax.zombusters.game.enemy.BaseEnemy
import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.model.AVATAR_CRASH_RADIUS
import com.retrowax.zombusters.game.model.Avatar
import com.retrowax.zombusters.game.model.EnemyType
import com.retrowax.zombusters.game.model.MINOTAUR_SCORE
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.model.RAT_SCORE
import com.retrowax.zombusters.game.model.WOLF_SCORE
import com.retrowax.zombusters.game.model.ZOMBIE_SCORE

object DamageSystem {

    private fun scoreFor(type: EnemyType): Int = when (type) {
        EnemyType.ZOMBIE -> ZOMBIE_SCORE
        EnemyType.RAT -> RAT_SCORE
        EnemyType.WOLF -> WOLF_SCORE
        EnemyType.MINOTAUR -> MINOTAUR_SCORE
        else -> 0
    }

    fun processBulletCollisions(
        combatState: CombatState,
        enemies: List<BaseEnemy>,
        avatar: Avatar,
        totalSec: Float
    ): Int {
        var kills = 0
        val bulletsToRemove = mutableSetOf<Int>()
        val shellsToRemove = mutableSetOf<Int>()

        for (enemy in enemies) {
            if (!enemy.isActive) continue
            val ex = enemy.entity.position.x
            val ey = enemy.entity.position.y

            for (i in combatState.bullets.indices) {
                if (i in bulletsToRemove) continue
                val (bx, by) = combatState.bullets[i].positionAt(totalSec)
                if (combatState.bullets[i].isHitting(bx, by, ex, ey)) {
                    enemy.lifecounter -= 1f
                    bulletsToRemove.add(i)
                    if (enemy.lifecounter <= 0f) {
                        enemy.crash(totalSec)
                        avatar.score += scoreFor(enemy.type)
                        kills++
                    }
                    break
                }
            }

            for (si in combatState.shotgunShells.indices) {
                if (si in shellsToRemove) continue
                val shell = combatState.shotgunShells[si]
                for (pelletIdx in 0..2) {
                    val (px, py) = shell.pelletPositionAt(pelletIdx, totalSec)
                    if (shell.isPelletHitting(px, py, ex, ey)) {
                        enemy.lifecounter -= 1f
                        shellsToRemove.add(si)
                        if (enemy.lifecounter <= 0f) {
                            enemy.crash(totalSec)
                            avatar.score += scoreFor(enemy.type)
                            kills++
                        }
                        break
                    }
                }
            }
        }

        bulletsToRemove.sortedDescending().forEach { combatState.bullets.removeAt(it) }
        shellsToRemove.sortedDescending().forEach { combatState.shotgunShells.removeAt(it) }

        return kills
    }

    fun processEnemyContact(
        enemies: List<BaseEnemy>,
        avatar: Avatar,
        totalSec: Float
    ): Boolean {
        if (avatar.status != ObjectStatus.ACTIVE) return false
        if (avatar.immuneBuff) return false

        val playerPos = Vec2(avatar.position.x.toFloat(), avatar.position.y.toFloat())
        var damaged = false

        for (enemy in enemies) {
            if (!enemy.isActive) continue
            if (enemy.isInRange(playerPos, AVATAR_CRASH_RADIUS)) {
                avatar.lifecounter -= 1
                avatar.isLosingLife = true
                damaged = true
                if (avatar.lifecounter <= 0) {
                    avatar.lives--
                    avatar.destroy(totalSec)
                }
                break
            }
        }
        return damaged
    }
}
