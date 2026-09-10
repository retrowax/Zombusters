package com.retrowax.zombusters.game.model

import korlibs.math.geom.Point

class Avatar {
    var position: Point = Point(0.0, 0.0)
    var status: ObjectStatus = ObjectStatus.INACTIVE
    var lives: Int = AVATAR_LIVES
    var hp: Int = AVATAR_HP
    var score: Int = 0
    var currentGun: GunType = GunType.PISTOL
    var deathTimeTotalSeconds: Float = 0f
    var character: PlayerCharacter = PlayerCharacter.JADE

    val isPlayingTheGame: Boolean
        get() = status != ObjectStatus.INACTIVE

    fun activate() {
        status = ObjectStatus.ACTIVE
    }

    fun deactivate() {
        status = ObjectStatus.INACTIVE
    }

    fun reset() {
        hp = AVATAR_HP
        lives = AVATAR_LIVES
        score = 0
        currentGun = GunType.PISTOL
        deathTimeTotalSeconds = 0f
        status = ObjectStatus.INACTIVE
    }

    fun destroy(totalGameSeconds: Float) {
        deathTimeTotalSeconds = totalGameSeconds
        status = ObjectStatus.DYING
    }

    fun update(totalGameSeconds: Float) {
        when (status) {
            ObjectStatus.DYING -> {
                if (totalGameSeconds > deathTimeTotalSeconds + AVATAR_RESPAWN_TIME) {
                    status = if (lives > 0) ObjectStatus.IMMUNE else ObjectStatus.INACTIVE
                }
            }
            ObjectStatus.IMMUNE -> {
                if (totalGameSeconds > deathTimeTotalSeconds + AVATAR_IMMUNE_TIME) {
                    status = ObjectStatus.ACTIVE
                }
            }
            else -> Unit
        }
    }

    companion object {
        const val COLLISION_RADIUS = AVATAR_BOUNDING_RADIUS
    }
}
