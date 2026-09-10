package com.steelvectors.app.feature.game.entities

import com.steelvectors.app.feature.game.base.Collisionable
import com.steelvectors.app.feature.game.base.UpdatableView

abstract class Enemy(
    screenWidth: Float,
    screenHeight: Float
) : UpdatableView(screenWidth, screenHeight), Collisionable {
    open val type = EnemyType.ASSAULT
    open var status = EnemyStatus.ALIVE
    open var strength = DEFAULT_STRENGTH

    fun hit() {
        strength--
    }

    companion object {
        internal const val DEFAULT_SPEED = 2
        internal const val DEFAULT_STRENGTH = 1
    }
}

enum class EnemyStatus {
    ALIVE,
    DEAD
}

enum class EnemyType {
    GUNSHIP,
    ASSAULT,
    BOMBER,
    FIGHTER,
    INTERDICTOR,
    RECON,
    BOSS
}
