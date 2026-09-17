package com.retrowax.zombusters.game.model

import korlibs.math.geom.Point

class Avatar {
    var position: Point = Point(0.0, 0.0)
    var status: ObjectStatus = ObjectStatus.INACTIVE
    var lives: Int = AVATAR_LIVES
    var lifecounter: Int = AVATAR_HP
    var score: Int = 0
    var currentGun: GunType = GunType.PISTOL
    var deathTimeTotalSeconds: Float = 0f
    var character: PlayerCharacter = PlayerCharacter.JADE
    var lastShot: Float = 0f
    var isLosingLife: Boolean = false
    var speedBuff: Boolean = false
    var speedBuffEndTime: Float = 0f
    var immuneBuff: Boolean = false

    val ammo: IntArray = IntArray(GunType.entries.size) { 0 }

    val isPlayingTheGame: Boolean
        get() = status != ObjectStatus.INACTIVE

    val pixelsPerSecond: Float
        get() = if (speedBuff) AVATAR_PIXELS_PER_SECOND * 1.5f else AVATAR_PIXELS_PER_SECOND

    fun activate() {
        status = ObjectStatus.ACTIVE
    }

    fun deactivate() {
        status = ObjectStatus.INACTIVE
    }

    fun reset() {
        lifecounter = AVATAR_HP
        lives = AVATAR_LIVES
        score = 0
        currentGun = GunType.PISTOL
        deathTimeTotalSeconds = 0f
        status = ObjectStatus.INACTIVE
        lastShot = 0f
        speedBuff = false
        immuneBuff = false
        for (i in ammo.indices) ammo[i] = 0
    }

    fun destroy(totalGameSeconds: Float) {
        deathTimeTotalSeconds = totalGameSeconds
        status = ObjectStatus.DYING
    }

    fun verifyFire(currentSec: Float, rateOfFire: Float): Boolean {
        return (currentSec - lastShot) > (1f / rateOfFire)
    }

    fun cycleWeapon() {
        currentGun = when (currentGun) {
            GunType.PISTOL -> if (ammo[GunType.MACHINEGUN.id] > 0) GunType.MACHINEGUN else
                              if (ammo[GunType.SHOTGUN.id] > 0) GunType.SHOTGUN else
                              if (ammo[GunType.FLAMETHROWER.id] > 0) GunType.FLAMETHROWER else GunType.PISTOL
            GunType.MACHINEGUN -> if (ammo[GunType.SHOTGUN.id] > 0) GunType.SHOTGUN else
                                   if (ammo[GunType.FLAMETHROWER.id] > 0) GunType.FLAMETHROWER else GunType.PISTOL
            GunType.SHOTGUN -> if (ammo[GunType.FLAMETHROWER.id] > 0) GunType.FLAMETHROWER else GunType.PISTOL
            GunType.FLAMETHROWER -> GunType.PISTOL
            GunType.GRENADE -> GunType.PISTOL
        }
    }

    fun update(totalGameSeconds: Float) {
        if (speedBuff && totalGameSeconds > speedBuffEndTime) {
            speedBuff = false
        }
        when (status) {
            ObjectStatus.DYING -> {
                if (totalGameSeconds > deathTimeTotalSeconds + AVATAR_RESPAWN_TIME) {
                    status = if (lives > 0) ObjectStatus.IMMUNE else ObjectStatus.INACTIVE
                    if (status == ObjectStatus.IMMUNE) {
                        lifecounter = AVATAR_HP
                    }
                }
            }
            ObjectStatus.IMMUNE -> {
                if (totalGameSeconds > deathTimeTotalSeconds + AVATAR_RESPAWN_TIME + AVATAR_IMMUNE_TIME) {
                    status = ObjectStatus.ACTIVE
                }
            }
            else -> Unit
        }
        isLosingLife = false
    }

    companion object {
        const val COLLISION_RADIUS = AVATAR_BOUNDING_RADIUS
    }
}
