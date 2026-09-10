package com.steelvectors.app.feature.game.base

import korlibs.math.geom.Point

interface Collisionable {
    var position: Point

    fun getImageWidth(): Int

    fun getImageHeight(): Int

    fun isCollidingWith(collisionable: Collisionable): Boolean
}
