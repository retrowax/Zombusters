package com.retrowax.zombusters.game.world

import com.retrowax.zombusters.game.model.FurnitureOrientation
import com.retrowax.zombusters.game.model.FurnitureType
import korlibs.io.file.std.resourcesVfs
import korlibs.io.serialization.xml.Xml

class LevelParser(private val filesResourcesPath: String) {

    suspend fun loadAllLevels(): Map<Int, LevelDef> {
        val levelDefs = parseLevelsDef()
        val result = mutableMapOf<Int, LevelDef>()
        for (n in 1..10) {
            val base = levelDefs[n] ?: continue
            val subLevels = parseLevelEnemies(n)
            val furnitures = parseLevelFurnitures(n)
            val walls = parseLevelWalls(n)
            result[n] = base.copy(subLevels = subLevels, furnitures = furnitures, walls = walls)
        }
        return result
    }

    suspend fun loadLevel(levelNumber: Int): LevelDef? {
        val levelDefs = parseLevelsDef()
        val base = levelDefs[levelNumber] ?: return null
        val subLevels = parseLevelEnemies(levelNumber)
        val furnitures = parseLevelFurnitures(levelNumber)
        val walls = parseLevelWalls(levelNumber)
        return base.copy(subLevels = subLevels, furnitures = furnitures, walls = walls)
    }

    private suspend fun parseLevelsDef(): Map<Int, LevelDef> {
        val xml = Xml(resourcesVfs["$filesResourcesPath/levels/LevelsDef.xml"].readString())
        val result = mutableMapOf<Int, LevelDef>()
        for (n in 1..10) {
            val levelNode = xml.child("Level$n") ?: continue
            result[n] = parseLevelNode(levelNode)
        }
        return result
    }

    private fun parseLevelNode(node: Xml): LevelDef {
        fun attr(name: String): Pair<Float, Float> {
            val v = node.attributes[name] ?: "0,0"
            val parts = v.split(",")
            return Pair(parts[0].trim().toFloatOrNull() ?: 0f, parts[1].trim().toFloatOrNull() ?: 0f)
        }

        val (p1x, p1y) = attr("P1SpawnPos")
        val (p2x, p2y) = attr("P2SpawnPos")
        val (p3x, p3y) = attr("P3SpawnPos")
        val (p4x, p4y) = attr("P4SpawnPos")

        val zones = (1..4).map { i ->
            val (ox, oy) = attr("ZSpawnZone${i}Origin")
            val (ex, ey) = attr("ZSpawnZone${i}End")
            SpawnZone(ox, oy, ex, ey)
        }

        return LevelDef(
            p1SpawnX = p1x, p1SpawnY = p1y,
            p2SpawnX = p2x, p2SpawnY = p2y,
            p3SpawnX = p3x, p3SpawnY = p3y,
            p4SpawnX = p4x, p4SpawnY = p4y,
            spawnZones = zones,
            subLevels = emptyList(),
            furnitures = emptyList(),
            walls = emptyList()
        )
    }

    private suspend fun parseLevelEnemies(n: Int): List<SubLevel> {
        val path = "$filesResourcesPath/levels/Level$n/Level${n}_enemies.xml"
        val xml = Xml(resourcesVfs[path].readString())
        return xml.children("EnemiesCount").map { ec ->
            SubLevel(
                enemiesCount = EnemiesCount(
                    zombies = ec.childText("Zombies")?.toIntOrNull() ?: 0,
                    rats = ec.childText("Rats")?.toIntOrNull() ?: 0,
                    wolves = ec.childText("Wolfs")?.toIntOrNull() ?: 0,
                    minotaurs = ec.childText("Minotaurs")?.toIntOrNull() ?: 0,
                    tanks = ec.childText("Tanks")?.toIntOrNull() ?: 0
                )
            )
        }
    }

    private suspend fun parseLevelFurnitures(n: Int): List<Furniture> {
        val path = "$filesResourcesPath/levels/Level$n/Level${n}_furnitures.xml"
        val xml = Xml(resourcesVfs[path].readString())
        return xml.children("Furniture").mapNotNull { f ->
            val typeStr = f.childText("Type") ?: return@mapNotNull null
            val orientStr = f.childText("Orientation") ?: return@mapNotNull null
            val pos = f.child("Position")
            val obsPos = f.child("ObstaclePosition")
            Furniture(
                type = parseFurnitureType(typeStr) ?: return@mapNotNull null,
                orientation = parseFurnitureOrientation(orientStr) ?: return@mapNotNull null,
                positionX = pos?.childText("X")?.toFloatOrNull() ?: 0f,
                positionY = pos?.childText("Y")?.toFloatOrNull() ?: 0f,
                obstaclePositionX = obsPos?.childText("X")?.toFloatOrNull() ?: 0f,
                obstaclePositionY = obsPos?.childText("Y")?.toFloatOrNull() ?: 0f,
                obstacleRadius = f.childText("ObstacleRadius")?.toFloatOrNull() ?: 0f
            )
        }
    }

    private suspend fun parseLevelWalls(n: Int): List<LevelWall> {
        val path = "$filesResourcesPath/levels/Level$n/Level${n}_walls.xml"
        val xml = Xml(resourcesVfs[path].readString())
        return xml.children("Wall").mapNotNull { w ->
            val from = w.child("From")
            val to = w.child("To")
            LevelWall(
                fromX = from?.childText("X")?.toFloatOrNull() ?: return@mapNotNull null,
                fromY = from.childText("Y")?.toFloatOrNull() ?: return@mapNotNull null,
                toX = to?.childText("X")?.toFloatOrNull() ?: return@mapNotNull null,
                toY = to?.childText("Y")?.toFloatOrNull() ?: return@mapNotNull null
            )
        }
    }

    private fun Xml.childText(name: String): String? = child(name)?.text?.trim()

    private fun parseFurnitureType(s: String): FurnitureType? = when (s) {
        "Basura" -> FurnitureType.BASURA
        "Arbol" -> FurnitureType.ARBOL
        "Banco" -> FurnitureType.BANCO
        "Farola" -> FurnitureType.FAROLA
        "Coche" -> FurnitureType.COCHE
        "CocheArdiendo" -> FurnitureType.COCHE_ARDIENDO
        "Puente" -> FurnitureType.PUENTE
        else -> null
    }

    private fun parseFurnitureOrientation(s: String): FurnitureOrientation? = when (s) {
        "NorthEast" -> FurnitureOrientation.NORTH_EAST
        "NorthWest" -> FurnitureOrientation.NORTH_WEST
        "SouthEast" -> FurnitureOrientation.SOUTH_EAST
        "SouthWest" -> FurnitureOrientation.SOUTH_WEST
        else -> null
    }
}
