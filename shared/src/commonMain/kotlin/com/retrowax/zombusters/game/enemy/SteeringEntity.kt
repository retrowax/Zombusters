package com.retrowax.zombusters.game.enemy

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

    // Heading derived from velocity direction; falls back to east (1,0) when stationary
    val heading: Vec2 get() {
        val len = velocity.length()
        return if (len > 0.001f) Vec2(velocity.x / len, velocity.y / len) else Vec2(1f, 0f)
    }

    // Perpendicular to heading (-heading.y, heading.x)
    val side: Vec2 get() { val h = heading; return Vec2(-h.y, h.x) }
}
