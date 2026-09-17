package com.retrowax.zombusters.game.model

data class PowerUp(
    val type: PowerUpType,
    var x: Float,
    var y: Float,
    var status: ObjectStatus = ObjectStatus.ACTIVE,
    var spawnedAt: Float = 0f
) {
    val isActive: Boolean get() = status == ObjectStatus.ACTIVE

    fun update(totalSec: Float) {
        when (status) {
            ObjectStatus.ACTIVE -> {
                if (totalSec - spawnedAt > POWERUP_ACTIVE_TIME) {
                    status = ObjectStatus.DYING
                }
            }
            ObjectStatus.DYING -> {
                if (totalSec - spawnedAt > POWERUP_ACTIVE_TIME + POWERUP_DYING_TIME) {
                    status = ObjectStatus.INACTIVE
                }
            }
            else -> Unit
        }
    }

    fun applyTo(avatar: Avatar, totalSec: Float = 0f): Boolean {
        when (type) {
            PowerUpType.LIVE -> {
                avatar.lifecounter = minOf(AVATAR_HP, avatar.lifecounter + POWERUP_HEALTH_RESTORE)
            }
            PowerUpType.MACHINEGUN -> {
                avatar.ammo[GunType.MACHINEGUN.id] += POWERUP_MACHINEGUN_AMMO
                if (avatar.currentGun == GunType.PISTOL) avatar.currentGun = GunType.MACHINEGUN
            }
            PowerUpType.SHOTGUN -> {
                avatar.ammo[GunType.SHOTGUN.id] += POWERUP_SHOTGUN_AMMO
                if (avatar.currentGun == GunType.PISTOL) avatar.currentGun = GunType.SHOTGUN
            }
            PowerUpType.FLAMETHROWER -> {
                avatar.ammo[GunType.FLAMETHROWER.id] += POWERUP_FLAMETHROWER_AMMO
                if (avatar.currentGun == GunType.PISTOL) avatar.currentGun = GunType.FLAMETHROWER
            }
            PowerUpType.GRENADE -> {
                avatar.ammo[GunType.GRENADE.id] += POWERUP_GRENADES
            }
            PowerUpType.SPEED_BUFF -> {
                avatar.speedBuff = true
                avatar.speedBuffEndTime = totalSec + POWERUP_SPEED_BUFF_DURATION
            }
            PowerUpType.IMMUNE_BUFF -> {
                avatar.immuneBuff = true
                avatar.immuneBuffEndTime = totalSec + POWERUP_IMMUNE_BUFF_DURATION
            }
            PowerUpType.EXTRA_LIFE -> {
                if (avatar.lives < 9) avatar.lives++
            }
        }
        status = ObjectStatus.INACTIVE
        return true
    }
}
