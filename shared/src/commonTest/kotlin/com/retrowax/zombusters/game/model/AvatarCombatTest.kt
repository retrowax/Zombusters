package com.retrowax.zombusters.game.model

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class AvatarCombatTest {

    @Test
    fun initialLifecounterIsMaxHP() {
        val avatar = Avatar()
        assertEquals(AVATAR_HP, avatar.lifecounter)
    }

    @Test
    fun initialAmmoIsAllZero() {
        val avatar = Avatar()
        avatar.ammo.forEach { assertEquals(0, it) }
    }

    @Test
    fun verifyFire_sameTime_returnsFalse() {
        val avatar = Avatar().apply { lastShot = 5f }
        assertFalse(avatar.verifyFire(5f, AVATAR_RATE_OF_FIRE))
    }

    @Test
    fun verifyFire_afterInterval_returnsTrue() {
        val avatar = Avatar().apply { lastShot = 0f }
        // Pistol fires at 2/s → interval = 0.5s; check at 0.6s
        assertTrue(avatar.verifyFire(0.6f, AVATAR_RATE_OF_FIRE))
    }

    @Test
    fun verifyFire_beforeInterval_returnsFalse() {
        val avatar = Avatar().apply { lastShot = 0f }
        // 0.3s < 0.5s interval for pistol
        assertFalse(avatar.verifyFire(0.3f, AVATAR_RATE_OF_FIRE))
    }

    @Test
    fun cycleWeapon_noAmmo_staysOnPistol() {
        val avatar = Avatar().apply { currentGun = GunType.PISTOL }
        avatar.cycleWeapon()
        assertEquals(GunType.PISTOL, avatar.currentGun)
    }

    @Test
    fun cycleWeapon_withMachinegunAmmo_switchesToMachinegun() {
        val avatar = Avatar().apply {
            currentGun = GunType.PISTOL
            ammo[GunType.MACHINEGUN.id] = 50
        }
        avatar.cycleWeapon()
        assertEquals(GunType.MACHINEGUN, avatar.currentGun)
    }

    @Test
    fun destroy_setsStatusDying() {
        val avatar = Avatar().apply { status = ObjectStatus.ACTIVE }
        avatar.destroy(10f)
        assertEquals(ObjectStatus.DYING, avatar.status)
        assertEquals(10f, avatar.deathTimeTotalSeconds)
    }

    @Test
    fun update_dyingToImmune_afterRespawnTime() {
        val avatar = Avatar().apply {
            status = ObjectStatus.DYING
            deathTimeTotalSeconds = 0f
            lives = AVATAR_LIVES
        }
        // Advance past AVATAR_RESPAWN_TIME (2s)
        avatar.update(AVATAR_RESPAWN_TIME + 0.1f)
        assertEquals(ObjectStatus.IMMUNE, avatar.status)
        assertEquals(AVATAR_HP, avatar.lifecounter)
    }

    @Test
    fun update_immuneToActive_afterImmunePeriod() {
        val avatar = Avatar().apply {
            status = ObjectStatus.IMMUNE
            deathTimeTotalSeconds = 0f
        }
        // Advance past RESPAWN + IMMUNE time
        avatar.update(AVATAR_RESPAWN_TIME + AVATAR_IMMUNE_TIME + 0.1f)
        assertEquals(ObjectStatus.ACTIVE, avatar.status)
    }
}
