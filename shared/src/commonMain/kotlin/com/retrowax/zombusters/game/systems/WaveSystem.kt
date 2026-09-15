package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.world.LevelDef
import com.retrowax.zombusters.game.world.SubLevel

// Manages SubLevel (wave) progression for a level
class WaveSystem(private val levelDef: LevelDef) {

    private var currentWaveIndex = 0
    var spawned = false  // whether the current wave has been spawned yet

    val currentWave: SubLevel? get() = levelDef.subLevels.getOrNull(currentWaveIndex)
    val waveNumber: Int get() = currentWaveIndex + 1
    val totalWaves: Int get() = levelDef.subLevels.size
    val isLevelComplete: Boolean get() = currentWaveIndex >= levelDef.subLevels.size

    fun markSpawned() {
        spawned = true
    }

    // Call when all active enemies are eliminated; returns true if another wave follows
    fun advanceWave(): Boolean {
        currentWaveIndex++
        spawned = false
        return !isLevelComplete
    }

    fun reset() {
        currentWaveIndex = 0
        spawned = false
    }
}
