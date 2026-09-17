package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.model.Avatar
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.model.PowerUp
import com.retrowax.zombusters.game.model.PowerUpType
import kotlin.random.Random

// Droppable types from enemy kills (legacy: indices 0–3 of PowerUpType)
private val DROPPABLE_TYPES = listOf(
    PowerUpType.LIVE, PowerUpType.MACHINEGUN, PowerUpType.FLAMETHROWER, PowerUpType.SHOTGUN
)

class PowerUpSystem(private val random: Random = Random.Default) {
    val powerUps: MutableList<PowerUp> = mutableListOf()

    // Legacy: 1/13 chance of a power-up drop, 1/127 separate chance of extra life
    fun trySpawnOnKill(x: Float, y: Float, totalSec: Float) {
        if (random.nextInt(1, 14) == 8) {
            val type = DROPPABLE_TYPES[random.nextInt(DROPPABLE_TYPES.size)]
            powerUps.add(PowerUp(type, x, y, ObjectStatus.ACTIVE, totalSec))
        } else if (random.nextInt(1, 128) == 12) {
            powerUps.add(PowerUp(PowerUpType.EXTRA_LIFE, x, y, ObjectStatus.ACTIVE, totalSec))
        }
    }

    fun update(totalSec: Float, avatar: Avatar) {
        powerUps.forEach { it.update(totalSec) }

        val px = avatar.position.x.toFloat()
        val py = avatar.position.y.toFloat()
        for (pu in powerUps) {
            if (!pu.isActive) continue
            val dx = px - pu.x
            val dy = py - pu.y
            if (dx * dx + dy * dy < 30f * 30f) {
                pu.applyTo(avatar, totalSec)
            }
        }

        powerUps.removeAll { it.status == ObjectStatus.INACTIVE }
    }
}
