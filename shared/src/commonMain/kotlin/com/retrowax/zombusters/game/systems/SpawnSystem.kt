package com.retrowax.zombusters.game.systems

import com.retrowax.zombusters.game.enemy.BaseEnemy
import com.retrowax.zombusters.game.enemy.EnemyFactory
import com.retrowax.zombusters.game.model.EnemyType
import com.retrowax.zombusters.game.world.EnemiesCount
import com.retrowax.zombusters.game.world.Furniture
import com.retrowax.zombusters.game.world.SpawnZone
import kotlin.random.Random

// Port of legacy Enemies.InitializeEnemy — spawns a full wave from EnemiesCount
class SpawnSystem(private val random: Random = Random.Default) {

    fun spawnWave(
        enemiesCount: EnemiesCount,
        spawnZones: List<SpawnZone>,
        furnitures: List<Furniture>,
        subLevelIndex: Int = 0,
        life: Float = 1f,
        baseSpeed: Float = 0f,
        playerCount: Int = 1
    ): List<BaseEnemy> {
        if (spawnZones.isEmpty()) return emptyList()
        val result = mutableListOf<BaseEnemy>()

        fun spawn(type: EnemyType, count: Int) {
            repeat(count) {
                result.add(
                    EnemyFactory.create(
                        type = type,
                        spawnZones = spawnZones,
                        furnitures = furnitures,
                        subLevelIndex = subLevelIndex,
                        life = life,
                        baseSpeed = baseSpeed,
                        random = random,
                        playerCount = playerCount
                    )
                )
            }
        }

        spawn(EnemyType.ZOMBIE, enemiesCount.zombies)
        spawn(EnemyType.RAT, enemiesCount.rats)
        spawn(EnemyType.WOLF, enemiesCount.wolves)
        // Minotaur and Tank deferred — skip for now

        return result
    }
}
