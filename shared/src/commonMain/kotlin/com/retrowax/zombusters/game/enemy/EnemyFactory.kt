package com.retrowax.zombusters.game.enemy

import com.retrowax.zombusters.game.model.EnemyType
import com.retrowax.zombusters.game.world.Furniture
import com.retrowax.zombusters.game.world.SpawnZone
import kotlin.random.Random

// Port of legacy Enemies.InitializeEnemy — creates enemies with legacy defaults
object EnemyFactory {

    fun create(
        type: EnemyType,
        spawnZones: List<SpawnZone>,
        furnitures: List<Furniture>,
        subLevelIndex: Int = 0,
        life: Float = 1f,
        baseSpeed: Float = 0f,
        random: Random = Random.Default,
        playerCount: Int = 1
    ): BaseEnemy {
        val zone = spawnZones[random.nextInt(spawnZones.size)]
        val x = random.nextFloat() * (zone.xMax - zone.xMin) + zone.xMin
        val y = random.nextFloat() * (zone.yMax - zone.yMin) + zone.yMin
        val pos = Vec2(x, y)
        val subspeed = subLevelIndex / 10f
        val speed = baseSpeed + subspeed
        val obstacles = furnitures.map { f ->
            ObstacleCircle(Vec2(f.obstaclePositionX, f.obstaclePositionY), f.obstacleRadius)
        }

        return when (type) {
            EnemyType.ZOMBIE -> Zombie(pos, boundingRadius = 5f, life = life, speed = speed, obstacles = obstacles)
            EnemyType.RAT -> Rat(pos, boundingRadius = 5f, life = life, speed = speed, obstacles = obstacles)
            EnemyType.WOLF -> Wolf(pos, boundingRadius = 5f, life = life, speed = speed, obstacles = obstacles)
            EnemyType.MINOTAUR -> Zombie(pos, boundingRadius = 5f, life = life, speed = speed, obstacles = obstacles) // deferred
            EnemyType.TANK -> Zombie(pos, boundingRadius = 5f, life = life, speed = speed, obstacles = obstacles)     // deferred
        }
    }
}
