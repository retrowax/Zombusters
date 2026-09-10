package com.retrowax.zombusters.game.world

import kotlinx.coroutines.test.runTest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNotNull
import kotlin.test.assertTrue

class LevelParserTest {

    // On JVM, resourcesVfs resolves against the classpath root.
    // Compose resources for JVM are at composeResources/com.retrowax.zombusters.resources/files/...
    private val filesPath = "composeResources/com.retrowax.zombusters.resources/files"
    private val parser = LevelParser(filesPath)

    @Test
    fun level1P1SpawnIs955_260() = runTest {
        val level = parser.loadLevel(1)
        assertNotNull(level)
        assertEquals(955f, level.p1SpawnX, "P1 spawn X should be 955")
        assertEquals(260f, level.p1SpawnY, "P1 spawn Y should be 260")
    }

    @Test
    fun level1P2SpawnIs1099_260() = runTest {
        val level = parser.loadLevel(1)
        assertNotNull(level)
        assertEquals(1099f, level.p2SpawnX, "P2 spawn X should be 1099")
        assertEquals(260f, level.p2SpawnY, "P2 spawn Y should be 260")
    }

    @Test
    fun level1P3SpawnIs955_392() = runTest {
        val level = parser.loadLevel(1)
        assertNotNull(level)
        assertEquals(955f, level.p3SpawnX, "P3 spawn X should be 955")
        assertEquals(392f, level.p3SpawnY, "P3 spawn Y should be 392")
    }

    @Test
    fun level1P4SpawnIs1099_392() = runTest {
        val level = parser.loadLevel(1)
        assertNotNull(level)
        assertEquals(1099f, level.p4SpawnX, "P4 spawn X should be 1099")
        assertEquals(392f, level.p4SpawnY, "P4 spawn Y should be 392")
    }

    @Test
    fun level1Wave1Has10Zombies() = runTest {
        val level = parser.loadLevel(1)
        assertNotNull(level)
        assertTrue(level.subLevels.isNotEmpty(), "Level 1 should have waves")
        assertEquals(10, level.subLevels[0].enemiesCount.zombies, "Wave 1 should have 10 zombies")
    }

    @Test
    fun level1Wave2Has15ZombiesAnd2Rats() = runTest {
        val level = parser.loadLevel(1)
        assertNotNull(level)
        assertTrue(level.subLevels.size >= 2, "Level 1 should have at least 2 waves")
        assertEquals(15, level.subLevels[1].enemiesCount.zombies, "Wave 2 should have 15 zombies")
        assertEquals(2, level.subLevels[1].enemiesCount.rats, "Wave 2 should have 2 rats")
    }

    @Test
    fun level1FirstFurnitureIsCocheArdiendo() = runTest {
        val level = parser.loadLevel(1)
        assertNotNull(level)
        assertTrue(level.furnitures.isNotEmpty(), "Level 1 should have furnitures")
        val first = level.furnitures[0]
        assertEquals(com.retrowax.zombusters.game.model.FurnitureType.COCHE_ARDIENDO, first.type)
        assertEquals(com.retrowax.zombusters.game.model.FurnitureOrientation.SOUTH_EAST, first.orientation)
        assertEquals(450f, first.positionX, "First furniture X should be 450")
        assertEquals(533f, first.positionY, "First furniture Y should be 533")
        assertEquals(496f, first.obstaclePositionX, "First obstacle X should be 496")
        assertEquals(562f, first.obstaclePositionY, "First obstacle Y should be 562")
        assertEquals(40f, first.obstacleRadius, "First obstacle radius should be 40")
    }

    @Test
    fun level1FirstWallFrom_n250_255To0_255() = runTest {
        val level = parser.loadLevel(1)
        assertNotNull(level)
        assertTrue(level.walls.isNotEmpty(), "Level 1 should have walls")
        val first = level.walls[0]
        assertEquals(-250f, first.fromX, "First wall fromX should be -250")
        assertEquals(255f, first.fromY, "First wall fromY should be 255")
        assertEquals(0f, first.toX, "First wall toX should be 0")
        assertEquals(255f, first.toY, "First wall toY should be 255")
    }

    @Test
    fun level10P1SpawnIs709_367() = runTest {
        val level = parser.loadLevel(10)
        assertNotNull(level)
        assertEquals(709f, level.p1SpawnX, "Level 10 P1 spawn X should be 709")
        assertEquals(367f, level.p1SpawnY, "Level 10 P1 spawn Y should be 367")
    }
}
