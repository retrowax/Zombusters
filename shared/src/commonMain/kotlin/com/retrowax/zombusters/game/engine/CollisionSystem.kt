package com.retrowax.zombusters.game.engine

import com.retrowax.zombusters.game.world.Furniture
import com.retrowax.zombusters.game.world.LevelWall
import korlibs.math.geom.Point
import kotlin.math.sqrt

object CollisionSystem {

    fun wouldCollide(
        newPos: Point,
        walls: List<LevelWall>,
        furnitures: List<Furniture>
    ): Boolean {
        for (wall in walls) {
            if (distanceToSegment(newPos, wall.fromX.toDouble(), wall.fromY.toDouble(),
                    wall.toX.toDouble(), wall.toY.toDouble()) <= 5.0) {
                return true
            }
        }
        for (furniture in furnitures) {
            val rangeDistance = rangeDistanceForRadius(furniture.obstacleRadius)
            val dx = newPos.x - furniture.obstaclePositionX.toDouble()
            val dy = newPos.y - furniture.obstaclePositionY.toDouble()
            if (sqrt(dx * dx + dy * dy) < rangeDistance) {
                return true
            }
        }
        return false
    }

    fun resolveMovement(
        currentPos: Point,
        desiredDelta: Point,
        walls: List<LevelWall>,
        furnitures: List<Furniture>
    ): Point {
        val fullTarget = Point(currentPos.x + desiredDelta.x, currentPos.y + desiredDelta.y)
        if (!wouldCollide(fullTarget, walls, furnitures)) return fullTarget

        val xOnly = Point(currentPos.x + desiredDelta.x, currentPos.y)
        if (!wouldCollide(xOnly, walls, furnitures)) return xOnly

        val yOnly = Point(currentPos.x, currentPos.y + desiredDelta.y)
        if (!wouldCollide(yOnly, walls, furnitures)) return yOnly

        return currentPos
    }

    private fun rangeDistanceForRadius(radius: Float): Double = when {
        radius <= 5f -> 10.0
        radius <= 10f -> 15.0
        radius <= 20f -> 30.0
        else -> 45.0
    }

    fun distanceToSegment(p: Point, ax: Double, ay: Double, bx: Double, by: Double): Double {
        val dx = bx - ax
        val dy = by - ay
        val lenSq = dx * dx + dy * dy
        if (lenSq == 0.0) {
            val ex = p.x - ax
            val ey = p.y - ay
            return sqrt(ex * ex + ey * ey)
        }
        val t = ((p.x - ax) * dx + (p.y - ay) * dy) / lenSq
        val tc = t.coerceIn(0.0, 1.0)
        val closestX = ax + tc * dx
        val closestY = ay + tc * dy
        val ex = p.x - closestX
        val ey = p.y - closestY
        return sqrt(ex * ex + ey * ey)
    }
}
