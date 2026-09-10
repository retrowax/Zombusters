package com.retrowax.zombusters.game.world

import com.retrowax.zombusters.game.model.EnemyType
import com.retrowax.zombusters.game.model.FurnitureOrientation
import com.retrowax.zombusters.game.model.FurnitureType

// From legacy Wall.cs (SteeringBehaviors) — a line segment obstacle for AI navigation
data class SteeringWall(
    val fromX: Float,
    val fromY: Float,
    val toX: Float,
    val toY: Float
)

// From legacy ContentManager/Wall.cs — a level boundary/wall line segment
data class LevelWall(
    val fromX: Float,
    val fromY: Float,
    val toX: Float,
    val toY: Float
)

// From legacy Furniture.cs
data class Furniture(
    val type: FurnitureType,
    val orientation: FurnitureOrientation,
    val positionX: Float,
    val positionY: Float,
    val obstaclePositionX: Float,
    val obstaclePositionY: Float,
    val obstacleRadius: Float
)

// From legacy EnemiesCount.cs — enemies per wave/sublevel
data class EnemiesCount(
    val zombies: Int = 0,
    val rats: Int = 0,
    val wolves: Int = 0,
    val minotaurs: Int = 0,
    val tanks: Int = 0
) {
    val total: Int get() = zombies + rats + wolves + minotaurs + tanks

    fun countOf(type: EnemyType): Int = when (type) {
        EnemyType.ZOMBIE -> zombies
        EnemyType.RAT -> rats
        EnemyType.WOLF -> wolves
        EnemyType.MINOTAUR -> minotaurs
        EnemyType.TANK -> tanks
    }
}

// From legacy SubLevel.cs — one wave within a level
data class SubLevel(
    val enemiesCount: EnemiesCount
)

// Spawn zone — rectangular area defined by two corners
data class SpawnZone(
    val originX: Float,
    val originY: Float,
    val endX: Float,
    val endY: Float
)

// From legacy Level.cs — full level definition loaded from LevelsDef.xml
data class LevelDef(
    val p1SpawnX: Float,
    val p1SpawnY: Float,
    val p2SpawnX: Float,
    val p2SpawnY: Float,
    val p3SpawnX: Float,
    val p3SpawnY: Float,
    val p4SpawnX: Float,
    val p4SpawnY: Float,
    val spawnZones: List<SpawnZone>,  // 4 zones: Top, Left, Right, Bottom
    val subLevels: List<SubLevel>,    // up to 10 waves
    val furnitures: List<Furniture>,
    val walls: List<LevelWall>
) {
    fun spawnPositionFor(playerIndex: Int): Pair<Float, Float> = when (playerIndex) {
        0 -> Pair(p1SpawnX, p1SpawnY)
        1 -> Pair(p2SpawnX, p2SpawnY)
        2 -> Pair(p3SpawnX, p3SpawnY)
        3 -> Pair(p4SpawnX, p4SpawnY)
        else -> Pair(p1SpawnX, p1SpawnY)
    }
}
