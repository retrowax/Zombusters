package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.combat.CombatState
import com.retrowax.zombusters.game.combat.FacingDirection
import com.retrowax.zombusters.game.combat.Projectile
import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.enemy.Zombie
import com.retrowax.zombusters.game.model.Avatar
import com.retrowax.zombusters.game.model.BULLET_SPEED
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.model.ZOMBIE_SCORE
import korlibs.math.geom.Point
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class DamageSystemTest {

    private fun makeAvatar(x: Float = 640f, y: Float = 360f): Avatar = Avatar().apply {
        position = Point(x.toDouble(), y.toDouble())
        status = ObjectStatus.ACTIVE
    }

    private fun makeZombieAt(x: Float, y: Float): Zombie = Zombie(Vec2(x, y))

    @Test
    fun processBulletCollisions_bulletOnEnemy_killsAndScores() {
        val avatar = makeAvatar()
        val zombie = makeZombieAt(400f, 300f)
        val state = CombatState()
        // Bullet at enemy position, firedAt=1, totalSec=1 → d=0, position = start
        state.bullets.add(Projectile(400f, 300f, 1f, 0f, FacingDirection.N, BULLET_SPEED))

        val kills = DamageSystem.processBulletCollisions(state, listOf(zombie), avatar, 1f)

        assertEquals(1, kills)
        assertEquals(ZOMBIE_SCORE, avatar.score)
        assertEquals(0, state.bullets.size)  // bullet consumed
        assertFalse(zombie.isActive)
    }

    @Test
    fun processBulletCollisions_bulletFarFromEnemy_noKill() {
        val avatar = makeAvatar()
        val zombie = makeZombieAt(400f, 300f)
        val state = CombatState()
        // Bullet far away (1000,1000)
        state.bullets.add(Projectile(1000f, 1000f, 1f, 0f, FacingDirection.N, BULLET_SPEED))

        val kills = DamageSystem.processBulletCollisions(state, listOf(zombie), avatar, 1f)

        assertEquals(0, kills)
        assertEquals(0, avatar.score)
        assertTrue(zombie.isActive)
    }

    @Test
    fun processEnemyContact_enemyInRange_damagesPlayer() {
        val avatar = makeAvatar(500f, 300f)
        val initialHp = avatar.lifecounter
        // Place zombie right on top of player (within 40px threshold)
        val zombie = makeZombieAt(500f, 300f)

        val damaged = DamageSystem.processEnemyContact(listOf(zombie), avatar, 1f)

        assertTrue(damaged)
        assertTrue(avatar.lifecounter < initialHp)
    }

    @Test
    fun processEnemyContact_enemyFarAway_noDamage() {
        val avatar = makeAvatar(500f, 300f)
        val initialHp = avatar.lifecounter
        val zombie = makeZombieAt(900f, 300f)

        val damaged = DamageSystem.processEnemyContact(listOf(zombie), avatar, 1f)

        assertFalse(damaged)
        assertEquals(initialHp, avatar.lifecounter)
    }

    @Test
    fun processEnemyContact_immuneBuffPlayer_noDamage() {
        val avatar = makeAvatar(500f, 300f).apply { immuneBuff = true }
        val initialHp = avatar.lifecounter
        val zombie = makeZombieAt(500f, 300f)

        val damaged = DamageSystem.processEnemyContact(listOf(zombie), avatar, 1f)

        assertFalse(damaged)
        assertEquals(initialHp, avatar.lifecounter)
    }
}
