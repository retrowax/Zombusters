package com.retrowax.zombusters.game.world

import com.retrowax.zombusters.game.model.Avatar
import com.retrowax.zombusters.game.model.ObjectStatus
import korlibs.math.geom.Point

class GameplayWorld(val levelDef: LevelDef) {

    val player1: Avatar = Avatar().apply {
        position = Point(levelDef.p1SpawnX.toDouble(), levelDef.p1SpawnY.toDouble())
        status = ObjectStatus.ACTIVE
    }

    val walls: List<LevelWall> get() = levelDef.walls
    val furnitures: List<Furniture> get() = levelDef.furnitures
    val spawnZones: List<SpawnZone> get() = levelDef.spawnZones
}
