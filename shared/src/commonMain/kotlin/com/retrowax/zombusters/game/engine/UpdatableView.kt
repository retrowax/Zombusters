package com.retrowax.zombusters.game.engine

import com.retrowax.zombusters.game.model.GameplayState
import korlibs.korge.view.Container
import korlibs.korge.view.addUpdater
import kotlin.time.Duration

abstract class UpdatableView : Container() {
    var currentState = GameplayState.NOT_PLAYING

    init {
        addUpdater { dt -> update(dt) }
    }

    abstract fun update(dt: Duration)

    fun paused() { currentState = GameplayState.PAUSE }
    fun playing() { currentState = GameplayState.PLAYING }
}
