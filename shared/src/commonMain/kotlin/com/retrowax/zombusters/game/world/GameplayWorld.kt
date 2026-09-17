package com.retrowax.zombusters.game.world

import com.retrowax.zombusters.game.combat.CombatState
import com.retrowax.zombusters.game.model.Avatar
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.systems.EnemySystem
import com.retrowax.zombusters.game.systems.PowerUpSystem
import com.retrowax.zombusters.game.systems.SpawnSystem
import com.retrowax.zombusters.game.systems.WaveSystem
import korlibs.math.geom.Point

class GameplayWorld(val levelDef: LevelDef) {

    val player1: Avatar = Avatar().apply {
        val spawn = Point(levelDef.p1SpawnX.toDouble(), levelDef.p1SpawnY.toDouble())
        position = spawn
        spawnPosition = spawn
        status = ObjectStatus.ACTIVE
    }

    val walls: List<LevelWall> get() = levelDef.walls
    val furnitures: List<Furniture> get() = levelDef.furnitures
    val spawnZones: List<SpawnZone> get() = levelDef.spawnZones

    val waveSystem = WaveSystem(levelDef)
    val spawnSystem = SpawnSystem()
    val enemySystem = EnemySystem()
    val combatState = CombatState()
    val powerUpSystem = PowerUpSystem()
}
