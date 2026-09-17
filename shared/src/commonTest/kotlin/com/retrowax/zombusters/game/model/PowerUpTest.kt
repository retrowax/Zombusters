package com.retrowax.zombusters.game.model

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class PowerUpTest {

    @Test
    fun powerUp_activeToDeathAfter20Seconds() {
        val pu = PowerUp(PowerUpType.LIVE, 100f, 200f, ObjectStatus.ACTIVE, 0f)
        pu.update(POWERUP_ACTIVE_TIME + 0.1f)
        assertEquals(ObjectStatus.DYING, pu.status)
    }

    @Test
    fun powerUp_dyingToInactiveAfterDyingPeriod() {
        val pu = PowerUp(PowerUpType.LIVE, 100f, 200f, ObjectStatus.ACTIVE, 0f)
        // Two updates: first transitions ACTIVE→DYING, second DYING→INACTIVE
        pu.update(POWERUP_ACTIVE_TIME + 0.1f)
        assertEquals(ObjectStatus.DYING, pu.status)
        pu.update(POWERUP_ACTIVE_TIME + POWERUP_DYING_TIME + 0.1f)
        assertEquals(ObjectStatus.INACTIVE, pu.status)
    }

    @Test
    fun applyLive_increasesLifecounter() {
        val avatar = Avatar().apply { lifecounter = 50 }
        val pu = PowerUp(PowerUpType.LIVE, 0f, 0f, ObjectStatus.ACTIVE, 0f)
        pu.applyTo(avatar)
        assertEquals(50 + POWERUP_HEALTH_RESTORE, avatar.lifecounter)
        assertEquals(ObjectStatus.INACTIVE, pu.status)
    }

    @Test
    fun applyLive_doesNotExceedMaxHP() {
        val avatar = Avatar().apply { lifecounter = AVATAR_HP - 5 }
        val pu = PowerUp(PowerUpType.LIVE, 0f, 0f, ObjectStatus.ACTIVE, 0f)
        pu.applyTo(avatar)
        assertEquals(AVATAR_HP, avatar.lifecounter)
    }

    @Test
    fun applyMachinegun_addsAmmo() {
        val avatar = Avatar()
        val pu = PowerUp(PowerUpType.MACHINEGUN, 0f, 0f, ObjectStatus.ACTIVE, 0f)
        pu.applyTo(avatar)
        assertEquals(POWERUP_MACHINEGUN_AMMO, avatar.ammo[GunType.MACHINEGUN.id])
        assertEquals(ObjectStatus.INACTIVE, pu.status)
    }

    @Test
    fun applyShotgun_addsAmmo() {
        val avatar = Avatar()
        val pu = PowerUp(PowerUpType.SHOTGUN, 0f, 0f, ObjectStatus.ACTIVE, 0f)
        pu.applyTo(avatar)
        assertEquals(POWERUP_SHOTGUN_AMMO, avatar.ammo[GunType.SHOTGUN.id])
    }

    @Test
    fun applyExtraLife_increasesLives() {
        val avatar = Avatar().apply { lives = 2 }
        val pu = PowerUp(PowerUpType.EXTRA_LIFE, 0f, 0f, ObjectStatus.ACTIVE, 0f)
        pu.applyTo(avatar)
        assertEquals(3, avatar.lives)
    }

    @Test
    fun applySpeedBuff_setsSpeedBuffTrue() {
        val avatar = Avatar()
        val pu = PowerUp(PowerUpType.SPEED_BUFF, 0f, 0f, ObjectStatus.ACTIVE, 0f)
        pu.applyTo(avatar)
        assertTrue(avatar.speedBuff)
        assertEquals(ObjectStatus.INACTIVE, pu.status)
    }
}
