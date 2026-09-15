package com.retrowax.zombusters.game.enemy

import kotlin.math.abs
import kotlin.math.atan2
import kotlin.math.cos
import kotlin.math.sin

class SteeringEntity(
    initialPosition: Vec2,
    var maxSpeed: Float = 1.5f,
    var boundingRadius: Float = 5f,
    var width: Float = 0f,
    var height: Float = 0f
) {
    var velocity: Vec2 = Vec2.ZERO

    private var _position: Vec2 = initialPosition.copy()
    private var _previousPosition: Vec2 = initialPosition.copy()

    var position: Vec2
        get() = _position
        set(value) {
            _previousPosition = _position.copy()
            _position = value.copy()
        }

    val previousPosition: Vec2 get() = _previousPosition

    val speed: Float get() = velocity.length()

    // Matches legacy VectorHelper.GetAngle: atan2(prev.Y - cur.Y, prev.X - cur.X)
    val angle: Float get() = atan2(_previousPosition.y - _position.y, _previousPosition.x - _position.x)

    // Matches legacy SteeringEntity.Heading: |cos(angle)|, |sin(angle)|
    val heading: Vec2 get() = Vec2(abs(cos(angle)), abs(sin(angle)))

    // Perpendicular to heading (-heading.y, heading.x)
    val side: Vec2 get() { val h = heading; return Vec2(-h.y, h.x) }
}
