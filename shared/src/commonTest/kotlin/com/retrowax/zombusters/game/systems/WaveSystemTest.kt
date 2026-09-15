package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.world.EnemiesCount
import com.retrowax.zombusters.game.world.LevelDef
import com.retrowax.zombusters.game.world.SubLevel
import kotlinx.coroutines.test.runTest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNotNull
import kotlin.test.assertNull
import kotlin.test.assertTrue

class WaveSystemTest {

    private fun makeLevelDef(waves: List<SubLevel>) = LevelDef(
        p1SpawnX = 100f, p1SpawnY = 100f,
        p2SpawnX = 200f, p2SpawnY = 100f,
        p3SpawnX = 300f, p3SpawnY = 100f,
        p4SpawnX = 400f, p4SpawnY = 100f,
        spawnZones = emptyList(),
        subLevels = waves,
        furnitures = emptyList(),
        walls = emptyList()
    )

    @Test
    fun initialState_firstWaveReady() {
        val level = makeLevelDef(listOf(
            SubLevel(EnemiesCount(zombies = 3)),
            SubLevel(EnemiesCount(zombies = 5))
        ))
        val ws = WaveSystem(level)
        assertEquals(1, ws.waveNumber)
        assertEquals(2, ws.totalWaves)
        assertFalse(ws.spawned)
        assertFalse(ws.isLevelComplete)
        assertEquals(3, ws.currentWave?.enemiesCount?.zombies)
    }

    @Test
    fun markSpawned_setsSpawnedTrue() {
        val level = makeLevelDef(listOf(SubLevel(EnemiesCount(zombies = 2))))
        val ws = WaveSystem(level)
        ws.markSpawned()
        assertTrue(ws.spawned)
    }

    @Test
    fun advanceWave_movesToNextWave() {
        val level = makeLevelDef(listOf(
            SubLevel(EnemiesCount(zombies = 3)),
            SubLevel(EnemiesCount(rats = 2))
        ))
        val ws = WaveSystem(level)
        ws.markSpawned()
        val hasMore = ws.advanceWave()
        assertTrue(hasMore)
        assertEquals(2, ws.waveNumber)
        assertFalse(ws.spawned)
        assertEquals(2, ws.currentWave?.enemiesCount?.rats)
    }

    @Test
    fun advanceWave_pastLastWave_isLevelComplete() {
        val level = makeLevelDef(listOf(SubLevel(EnemiesCount(zombies = 1))))
        val ws = WaveSystem(level)
        ws.markSpawned()
        val hasMore = ws.advanceWave()
        assertFalse(hasMore)
        assertTrue(ws.isLevelComplete)
        assertNull(ws.currentWave)
    }

    @Test
    fun reset_restoresToInitialState() {
        val level = makeLevelDef(listOf(
            SubLevel(EnemiesCount(zombies = 2)),
            SubLevel(EnemiesCount(rats = 1))
        ))
        val ws = WaveSystem(level)
        ws.markSpawned()
        ws.advanceWave()
        ws.reset()
        assertEquals(1, ws.waveNumber)
        assertFalse(ws.spawned)
        assertFalse(ws.isLevelComplete)
    }

    @Test
    fun waveSystemWithLevel1Data() = runTest {
        // Exercises wave system with realistic Level 1 data
        val level = makeLevelDef(listOf(
            SubLevel(EnemiesCount(zombies = 3, rats = 0, wolves = 0)),
            SubLevel(EnemiesCount(zombies = 4, rats = 1, wolves = 0)),
            SubLevel(EnemiesCount(zombies = 5, rats = 1, wolves = 1))
        ))
        val ws = WaveSystem(level)
        assertEquals(3, ws.totalWaves)
        assertNotNull(ws.currentWave)
        assertEquals(3, ws.currentWave!!.enemiesCount.zombies)

        ws.markSpawned()
        ws.advanceWave()
        assertEquals(4, ws.currentWave?.enemiesCount?.zombies)
        assertEquals(1, ws.currentWave?.enemiesCount?.rats)
    }
}
