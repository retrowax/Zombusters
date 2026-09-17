package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.model.Avatar
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.model.POWERUP_SPEED_BUFF_DURATION
import com.retrowax.zombusters.game.model.PowerUp
import com.retrowax.zombusters.game.model.PowerUpType
import kotlin.random.Random

class PowerUpSystem {
    val powerUps: MutableList<PowerUp> = mutableListOf()

    fun trySpawnOnKill(x: Float, y: Float, totalSec: Float) {
        if (Random.nextInt(14) == 0) {
            val type = PowerUpType.entries[Random.nextInt(PowerUpType.entries.size)]
            powerUps.add(PowerUp(type, x, y, ObjectStatus.ACTIVE, totalSec))
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
                if (pu.type == PowerUpType.SPEED_BUFF) {
                    avatar.speedBuff = true
                    avatar.speedBuffEndTime = totalSec + POWERUP_SPEED_BUFF_DURATION
                }
                pu.applyTo(avatar)
            }
        }

        powerUps.removeAll { it.status == ObjectStatus.INACTIVE }
    }
}
