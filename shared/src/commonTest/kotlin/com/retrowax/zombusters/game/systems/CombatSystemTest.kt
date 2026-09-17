package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.combat.CombatState
import com.retrowax.zombusters.game.combat.FacingDirection
import com.retrowax.zombusters.game.model.Avatar
import com.retrowax.zombusters.game.model.GunType
import com.retrowax.zombusters.game.model.AVATAR_RATE_OF_FIRE
import com.retrowax.zombusters.game.model.MACHINEGUN_RATE_OF_FIRE
import com.retrowax.zombusters.game.model.ObjectStatus
import korlibs.math.geom.Point
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class CombatSystemTest {

    private fun activeAvatar(): Avatar = Avatar().apply {
        status = ObjectStatus.ACTIVE
        position = Point(640.0, 360.0)
        lastShot = -1f  // allow firing immediately
    }

    @Test
    fun rateOfFire_pistol_returns2() {
        assertEquals(AVATAR_RATE_OF_FIRE, CombatSystem.rateOfFire(GunType.PISTOL))
    }

    @Test
    fun rateOfFire_machinegun_returns10() {
        assertEquals(MACHINEGUN_RATE_OF_FIRE, CombatSystem.rateOfFire(GunType.MACHINEGUN))
    }

    @Test
    fun tryFire_pistol_addsBullet() {
        val avatar = activeAvatar()
        val state = CombatState()
        val angle = FacingDirection.angleFrom(0f, -1f)
        val fired = CombatSystem.tryFire(avatar, state, 1f, angle)
        assertTrue(fired)
        assertEquals(1, state.bullets.size)
        assertEquals(0, state.shotgunShells.size)
    }

    @Test
    fun tryFire_rateOfFireNotMet_returnsFalse() {
        val avatar = activeAvatar().apply { lastShot = 0f }
        val state = CombatState()
        val angle = FacingDirection.angleFrom(0f, -1f)
        // Try to fire at t=0.1 with pistol (interval=0.5s)
        val fired = CombatSystem.tryFire(avatar, state, 0.1f, angle)
        assertFalse(fired)
        assertEquals(0, state.bullets.size)
    }

    @Test
    fun tryFire_machinegun_consumesAmmo() {
        val avatar = activeAvatar().apply {
            currentGun = GunType.MACHINEGUN
            ammo[GunType.MACHINEGUN.id] = 10
        }
        val state = CombatState()
        val angle = FacingDirection.angleFrom(0f, -1f)
        CombatSystem.tryFire(avatar, state, 1f, angle)
        assertEquals(9, avatar.ammo[GunType.MACHINEGUN.id])
    }

    @Test
    fun tryFire_machinegunNoAmmo_fallsBackToPistol() {
        val avatar = activeAvatar().apply {
            currentGun = GunType.MACHINEGUN
            ammo[GunType.MACHINEGUN.id] = 0
        }
        val state = CombatState()
        val angle = FacingDirection.angleFrom(0f, -1f)
        CombatSystem.tryFire(avatar, state, 1f, angle)
        assertEquals(GunType.PISTOL, avatar.currentGun)
    }

    @Test
    fun tryFire_shotgun_addsShellNotBullet() {
        val avatar = activeAvatar().apply {
            currentGun = GunType.SHOTGUN
            ammo[GunType.SHOTGUN.id] = 25
        }
        val state = CombatState()
        val angle = FacingDirection.angleFrom(0f, -1f)
        CombatSystem.tryFire(avatar, state, 1f, angle)
        assertEquals(0, state.bullets.size)
        assertEquals(1, state.shotgunShells.size)
    }

    @Test
    fun pruneOutOfBounds_removesOffscreenBullet() {
        val avatar = activeAvatar()
        val state = CombatState()
        val angle = FacingDirection.angleFrom(0f, -1f)
        // Place bullet off-screen to the left
        avatar.position = Point(-500.0, 360.0)
        CombatSystem.tryFire(avatar, state, 1f, angle)
        CombatSystem.pruneOutOfBounds(state, 1f)
        assertEquals(0, state.bullets.size)
    }

    @Test
    fun tryFire_updates_lastShot() {
        val avatar = activeAvatar()
        val state = CombatState()
        val angle = FacingDirection.angleFrom(0f, -1f)
        CombatSystem.tryFire(avatar, state, 2.5f, angle)
        assertEquals(2.5f, avatar.lastShot)
    }
}
