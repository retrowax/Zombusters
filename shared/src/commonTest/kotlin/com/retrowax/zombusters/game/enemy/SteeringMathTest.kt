package com.retrowax.zombusters.game.enemy

import com.retrowax.zombusters.game.enemy.steering.ArriveSteer
import com.retrowax.zombusters.game.enemy.steering.PursuitSteer
import kotlin.math.abs
import kotlin.math.sqrt
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class SteeringMathTest {

    @Test
    fun vec2Length_pythagoras() {
        val v = Vec2(3f, 4f)
        assertEquals(5f, v.length(), 0.001f)
    }

    @Test
    fun vec2Normalize_unitLength() {
        val v = Vec2(3f, 4f).normalize()
        assertEquals(1f, v.length(), 0.001f)
    }

    @Test
    fun vec2Truncate_reducesOverMax() {
        val v = Vec2(3f, 4f)  // length 5
        val t = v.truncate(2f)
        assertTrue(t.length() <= 2.001f, "Truncated length should be <= 2")
    }

    @Test
    fun vec2Truncate_leavesUnderMax() {
        val v = Vec2(1f, 1f)  // length ~1.41
        val t = v.truncate(2f)
        assertEquals(v.x, t.x, 0.001f)
        assertEquals(v.y, t.y, 0.001f)
    }

    @Test
    fun arriveSteer_targetReached_returnsNearZero() {
        val arrive = ArriveSteer(ArriveSteer.Deceleration.FAST, 50f)
        val entity = SteeringEntity(Vec2(100f, 100f), maxSpeed = 1.5f)
        arrive.target = Vec2(100f, 100f)  // already at target
        val force = arrive.calculate(entity)
        assertEquals(0f, force.x, 0.001f)
        assertEquals(0f, force.y, 0.001f)
    }

    @Test
    fun arriveSteer_farFromTarget_seeksWithMaxSpeed() {
        val arrive = ArriveSteer(ArriveSteer.Deceleration.FAST, 50f)
        val entity = SteeringEntity(Vec2(0f, 0f), maxSpeed = 1.5f)
        arrive.target = Vec2(1000f, 0f)  // far away, should seek at max speed
        val force = arrive.calculate(entity)
        // Desired velocity = maxSpeed in X direction; entity.velocity = 0, so force ≈ maxSpeed
        assertTrue(force.x > 0f, "Force should point toward target")
        assertTrue(abs(force.x - 1.5f) < 0.1f, "Force magnitude should approximate maxSpeed")
    }

    @Test
    fun pursuitSteer_predictsAheadOfEvader() {
        val pursuit = PursuitSteer(ArriveSteer.Deceleration.FAST, 50f)
        val entity = SteeringEntity(Vec2(0f, 0f), maxSpeed = 1.5f)
        val evader = SteeringEntity(Vec2(100f, 0f), maxSpeed = 1.5f)
        evader.velocity = Vec2(1f, 0f)  // moving right
        pursuit.target = Vec2(100f, 0f)
        pursuit.updateEvader(evader)
        val force = pursuit.calculate(entity)
        assertTrue(force.x > 0f, "Should steer toward predicted future evader position")
    }

    @Test
    fun steeringEntity_headingUpdate_afterPositionChange() {
        val entity = SteeringEntity(Vec2(0f, 0f))
        entity.position = Vec2(1f, 0f)  // moved right
        val h = entity.heading
        assertTrue(h.x >= 0f, "Heading X should be non-negative after rightward move")
        assertTrue(h.y >= 0f, "Heading Y should be non-negative (uses abs)")
    }

    @Test
    fun obstacleCircle_distanceTo_correctlyCalculated() {
        val a = Vec2(0f, 0f)
        val b = Vec2(3f, 4f)
        assertEquals(5f, a.distanceTo(b), 0.001f)
    }

    @Test
    fun separationLogic_pushesApart() {
        val obstacles = emptyList<ObstacleCircle>()
        val zombie1 = com.retrowax.zombusters.game.enemy.Zombie(Vec2(100f, 100f), obstacles = obstacles)
        val zombie2 = com.retrowax.zombusters.game.enemy.Zombie(Vec2(105f, 100f), obstacles = obstacles)
        val allEnemies = listOf(zombie1, zombie2)
        val distBefore = zombie1.entity.position.distanceTo(zombie2.entity.position)

        // Manually run separation
        zombie1.resolveSeparation(allEnemies)

        // After separation, distance should be >= before (pushed apart or unchanged)
        val distAfter = zombie1.entity.position.distanceTo(zombie2.entity.position)
        assertTrue(distAfter >= distBefore - 0.01f, "Separation should not decrease distance")
    }
}
