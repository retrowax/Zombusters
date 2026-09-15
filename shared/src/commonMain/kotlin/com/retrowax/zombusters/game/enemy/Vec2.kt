package com.retrowax.zombusters.game.enemy

import kotlin.math.sqrt

data class Vec2(var x: Float, var y: Float) {

    fun length(): Float = sqrt(x * x + y * y)

    operator fun plus(other: Vec2) = Vec2(x + other.x, y + other.y)
    operator fun minus(other: Vec2) = Vec2(x - other.x, y - other.y)
    operator fun times(scalar: Float) = Vec2(x * scalar, y * scalar)
    operator fun unaryMinus() = Vec2(-x, -y)

    fun distanceTo(other: Vec2): Float = (this - other).length()

    fun normalize(): Vec2 {
        val len = length()
        return if (len > 0f) Vec2(x / len, y / len) else Vec2(0f, 0f)
    }

    fun dot(other: Vec2): Float = x * other.x + y * other.y

    fun truncate(max: Float): Vec2 {
        val len = length()
        return if (len > max) normalize() * max else copy()
    }

    companion object {
        val ZERO = Vec2(0f, 0f)
    }
}
