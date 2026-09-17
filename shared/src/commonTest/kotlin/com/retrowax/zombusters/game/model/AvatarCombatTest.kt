package com.retrowax.zombusters.game.model

import korlibs.math.geom.Point
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
        // Legacy: expires at deathTimeTotalSeconds + IMMUNE_TIME (8s from death)
        avatar.update(AVATAR_IMMUNE_TIME + 0.1f)
        assertEquals(ObjectStatus.ACTIVE, avatar.status)
    }

    @Test
    fun update_speedBuff_expiresAfterDuration() {
        val avatar = Avatar().apply {
            speedBuff = true
            speedBuffEndTime = 10f
        }
        avatar.update(10.1f)
        assertFalse(avatar.speedBuff)
    }

    @Test
    fun update_immuneBuff_expiresAfterDuration() {
        val avatar = Avatar().apply {
            immuneBuff = true
            immuneBuffEndTime = 20f
        }
        avatar.update(20.1f)
        assertFalse(avatar.immuneBuff)
    }

    @Test
    fun update_immuneBuff_doesNotExpireBeforeDuration() {
        val avatar = Avatar().apply {
            immuneBuff = true
            immuneBuffEndTime = 20f
        }
        avatar.update(19.9f)
        assertTrue(avatar.immuneBuff)
    }

    @Test
    fun respawn_positionResetsToSpawnPoint() {
        val spawn = Point(955.0, 260.0)
        val avatar = Avatar().apply {
            spawnPosition = spawn
            position = Point(100.0, 100.0)
            status = ObjectStatus.DYING
            deathTimeTotalSeconds = 0f
            lives = 1
        }
        avatar.update(AVATAR_RESPAWN_TIME + 0.1f)
        assertEquals(ObjectStatus.IMMUNE, avatar.status)
        assertEquals(spawn, avatar.position)
    }
}
