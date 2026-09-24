package com.retrowax.zombusters.game.world

import com.retrowax.zombusters.game.combat.CombatState
import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.model.Avatar
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.systems.EnemySystem
import com.retrowax.zombusters.game.systems.PowerUpSystem
import com.retrowax.zombusters.game.systems.SpawnSystem
import com.retrowax.zombusters.game.systems.WaveSystem
import korlibs.math.geom.Point

class GameplayWorld(val levelDef: LevelDef, numPlayers: Int = 1) {

    val players: List<Avatar> = List(numPlayers.coerceIn(1, 4)) { idx ->
        Avatar().apply {
            val (sx, sy) = levelDef.spawnPositionFor(idx)
            val spawn = Point(sx.toDouble(), sy.toDouble())
            position = spawn
            spawnPosition = spawn
            status = ObjectStatus.ACTIVE
        }
    }

    // Per-player SteeringEntity — updated each frame by GameplayScene from the player's position/velocity
    val playerSteeringEntities: List<SteeringEntity> = List(numPlayers.coerceIn(1, 4)) { idx ->
        SteeringEntity(Vec2(players[idx].position.x.toFloat(), players[idx].position.y.toFloat()))
    }

    // Per-player CombatState — each player's bullets tracked separately (for score attribution)
    val combatStates: List<CombatState> = List(numPlayers.coerceIn(1, 4)) { CombatState() }

    // Backward-compat aliases (single-player code paths)
    val player1: Avatar get() = players[0]
    val combatState: CombatState get() = combatStates[0]

    val walls: List<LevelWall> get() = levelDef.walls
    val furnitures: List<Furniture> get() = levelDef.furnitures
    val spawnZones: List<SpawnZone> get() = levelDef.spawnZones

    val waveSystem = WaveSystem(levelDef)
    val spawnSystem = SpawnSystem()
    val enemySystem = EnemySystem()
    val powerUpSystem = PowerUpSystem()
}
