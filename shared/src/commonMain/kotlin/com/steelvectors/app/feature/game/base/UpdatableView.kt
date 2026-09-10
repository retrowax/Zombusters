package com.steelvectors.app.feature.game.base

import korlibs.korge.view.View
import korlibs.korge.view.addUpdater

abstract class UpdatableView(
    private val screenWidth: Float = 0.0f,
    private val screenHeight: Float = 0.0f
) : View() {
    internal var currentState = GameplayState.Playing

    init {
        addUpdater { update(screenWidth, screenHeight) }
    }

    abstract fun update(screenWidth: Float = 0.0f, screenHeight: Float = 0.0f)

    fun paused() {
        currentState = GameplayState.Paused
    }

    fun playing() {
        currentState = GameplayState.Playing
    }
}
