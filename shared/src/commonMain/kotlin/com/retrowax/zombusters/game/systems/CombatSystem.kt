package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.combat.CombatState
import com.retrowax.zombusters.game.combat.FacingDirection
import com.retrowax.zombusters.game.combat.Projectile
import com.retrowax.zombusters.game.combat.ShotgunShell
import com.retrowax.zombusters.game.model.Avatar
import com.retrowax.zombusters.game.model.AVATAR_RATE_OF_FIRE
import com.retrowax.zombusters.game.model.BULLET_SPEED
import com.retrowax.zombusters.game.model.FLAMETHROWER_RATE_OF_FIRE
import com.retrowax.zombusters.game.model.GunType
import com.retrowax.zombusters.game.model.MACHINEGUN_RATE_OF_FIRE
import com.retrowax.zombusters.game.model.SHOTGUN_RATE_OF_FIRE

private val PISTOL_MUZZLE: Map<FacingDirection, Pair<Float, Float>> = mapOf(
    FacingDirection.N  to Pair(5f, -57f),
    FacingDirection.NE to Pair(27f, -60f),
    FacingDirection.E  to Pair(35f, -34f),
    FacingDirection.SE to Pair(32f, -5f),
    FacingDirection.S  to Pair(5f, 5f),
    FacingDirection.SW to Pair(-35f, -5f),
    FacingDirection.W  to Pair(-37f, -34f),
    FacingDirection.NW to Pair(-32f, -60f)
)

private val MACHINEGUN_MUZZLE: Map<FacingDirection, Pair<Float, Float>> = mapOf(
    FacingDirection.N  to Pair(4f, -55f),
    FacingDirection.NE to Pair(33f, -60f),
    FacingDirection.E  to Pair(35f, -27f),
    FacingDirection.SE to Pair(27f, 0f),
    FacingDirection.S  to Pair(1f, 5f),
    FacingDirection.SW to Pair(-28f, 0f),
    FacingDirection.W  to Pair(-35f, -26f),
    FacingDirection.NW to Pair(-36f, -57f)
)

private val SHOTGUN_MUZZLE: Map<FacingDirection, Pair<Float, Float>> = mapOf(
    FacingDirection.N  to Pair(4f, -60f),
    FacingDirection.NE to Pair(27f, -60f),
    FacingDirection.E  to Pair(37f, -29f),
    FacingDirection.SE to Pair(32f, 0f),
    FacingDirection.S  to Pair(4f, 7f),
    FacingDirection.SW to Pair(-28f, 0f),
    FacingDirection.W  to Pair(-35f, -26f),
    FacingDirection.NW to Pair(-36f, -57f)
)

object CombatSystem {

    fun rateOfFire(gun: GunType): Float = when (gun) {
        GunType.MACHINEGUN -> MACHINEGUN_RATE_OF_FIRE
        GunType.FLAMETHROWER -> FLAMETHROWER_RATE_OF_FIRE
        GunType.SHOTGUN -> SHOTGUN_RATE_OF_FIRE
        else -> AVATAR_RATE_OF_FIRE
    }

    fun tryFire(
        avatar: Avatar,
        combatState: CombatState,
        totalSec: Float,
        fireAngle: Float,
        shooterIndex: Int = 0
    ): Boolean {
        if (avatar.currentGun != GunType.PISTOL && avatar.ammo[avatar.currentGun.id] <= 0) {
            avatar.currentGun = GunType.PISTOL
        }

        val rate = rateOfFire(avatar.currentGun)
        if (!avatar.verifyFire(totalSec, rate)) return false

        avatar.lastShot = totalSec
        val dir = FacingDirection.classify(fireAngle)
        val playerX = avatar.position.x.toFloat()
        val playerY = avatar.position.y.toFloat()

        when (avatar.currentGun) {
            GunType.PISTOL -> {
                val (ox, oy) = PISTOL_MUZZLE[dir] ?: Pair(0f, 0f)
                combatState.bullets.add(Projectile(playerX + ox, playerY + oy, totalSec, fireAngle, dir, BULLET_SPEED, shooterIndex))
            }
            GunType.MACHINEGUN -> {
                val (ox, oy) = MACHINEGUN_MUZZLE[dir] ?: Pair(0f, 0f)
                combatState.bullets.add(Projectile(playerX + ox, playerY + oy, totalSec, fireAngle, dir, BULLET_SPEED, shooterIndex))
                avatar.ammo[GunType.MACHINEGUN.id]--
            }
            GunType.SHOTGUN -> {
                val (ox, oy) = SHOTGUN_MUZZLE[dir] ?: Pair(0f, 0f)
                combatState.shotgunShells.add(ShotgunShell(playerX + ox, playerY + oy, totalSec, fireAngle, dir, shooterIndex))
                avatar.ammo[GunType.SHOTGUN.id]--
            }
            GunType.FLAMETHROWER -> {
                val (ox, oy) = MACHINEGUN_MUZZLE[dir] ?: Pair(0f, 0f)
                combatState.bullets.add(Projectile(playerX + ox, playerY + oy, totalSec, fireAngle, dir, BULLET_SPEED, shooterIndex))
                avatar.ammo[GunType.FLAMETHROWER.id]--
            }
            GunType.GRENADE -> {
                // Deferred
            }
        }
        return true
    }

    fun pruneOutOfBounds(combatState: CombatState, totalSec: Float) {
        combatState.bullets.removeAll { p ->
            val (x, y) = p.positionAt(totalSec)
            p.isOutOfBounds(x, y)
        }
        combatState.shotgunShells.removeAll { shell ->
            (0..2).all { i ->
                val (x, y) = shell.pelletPositionAt(i, totalSec)
                shell.isOutOfBounds(x, y)
            }
        }
    }
}
