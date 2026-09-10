package com.retrowax.zombusters.game.engine

import com.retrowax.zombusters.game.world.Furniture
import com.retrowax.zombusters.game.world.LevelWall
import com.retrowax.zombusters.game.model.FurnitureOrientation
import com.retrowax.zombusters.game.model.FurnitureType
import korlibs.math.geom.Point
import kotlin.test.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue
import kotlin.test.assertEquals

class CollisionSystemTest {

    private val emptyWalls = emptyList<LevelWall>()
    private val emptyFurnitures = emptyList<Furniture>()

    // Simple horizontal wall from (0,100) to (200,100)
    private val wall = LevelWall(0f, 100f, 200f, 100f)
    // Obstacle at (300,300) with radius 40 → rangeDistance = 45
    private val obstacle = Furniture(
        type = FurnitureType.COCHE_ARDIENDO,
        orientation = FurnitureOrientation.SOUTH_EAST,
        positionX = 300f, positionY = 300f,
        obstaclePositionX = 300f, obstaclePositionY = 300f,
        obstacleRadius = 40f
    )

    @Test
    fun openSpaceNoCollision() {
        val pos = Point(640.0, 360.0)
        assertFalse(CollisionSystem.wouldCollide(pos, emptyWalls, emptyFurnitures))
    }

    @Test
    fun wallBlocksCollision() {
        // Point exactly on the wall line
        val pos = Point(100.0, 100.0)
        assertTrue(CollisionSystem.wouldCollide(pos, listOf(wall), emptyFurnitures))
    }

    @Test
    fun wallNotBlockedFarAway() {
        val pos = Point(100.0, 200.0) // 100 units below wall
        assertFalse(CollisionSystem.wouldCollide(pos, listOf(wall), emptyFurnitures))
    }

    @Test
    fun obstacleBlocksWithinRange() {
        // Within 45 units of obstacle center at (300,300)
        val pos = Point(300.0, 330.0) // 30 units away → inside range of 45
        assertTrue(CollisionSystem.wouldCollide(pos, emptyWalls, listOf(obstacle)))
    }

    @Test
    fun obstacleNotBlockedBeyondRange() {
        // 100 units away from obstacle center → beyond range of 45
        val pos = Point(300.0, 400.0)
        assertFalse(CollisionSystem.wouldCollide(pos, emptyWalls, listOf(obstacle)))
    }

    @Test
    fun resolveMovementAllowsParallelSlide() {
        // Wall at y=100; player at y=110 trying to move left (no wall blocking left movement)
        val current = Point(100.0, 110.0)
        val delta = Point(-20.0, 0.0) // move left
        val result = CollisionSystem.resolveMovement(current, delta, listOf(wall), emptyFurnitures)
        assertEquals(80.0, result.x, "Should slide left along wall")
        assertEquals(110.0, result.y, "Y should stay same")
    }

    @Test
    fun resolveMovementBlockedIntoWall() {
        // Player at y=106 trying to move upward into the wall at y=100
        val current = Point(100.0, 106.0)
        val delta = Point(0.0, -10.0) // move up toward wall
        val result = CollisionSystem.resolveMovement(current, delta, listOf(wall), emptyFurnitures)
        // Full move blocked, x-only not possible (no x movement), so stays
        assertEquals(100.0, result.x)
        assertEquals(106.0, result.y)
    }

    @Test
    fun distanceToSegmentOnEndpoint() {
        val p = Point(0.0, 0.0)
        val d = CollisionSystem.distanceToSegment(p, 0.0, 0.0, 10.0, 0.0)
        assertEquals(0.0, d, 0.001)
    }

    @Test
    fun distanceToSegmentPerpendicular() {
        val p = Point(5.0, 3.0)
        val d = CollisionSystem.distanceToSegment(p, 0.0, 0.0, 10.0, 0.0)
        assertEquals(3.0, d, 0.001)
    }
}
