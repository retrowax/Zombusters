package com.retrowax.zombusters.game.engine

import korlibs.math.geom.Point

interface Collisionable {
    var position: Point
    fun getCollisionWidth(): Int
    fun getCollisionHeight(): Int
    fun isCollidingWith(other: Collisionable): Boolean
}
