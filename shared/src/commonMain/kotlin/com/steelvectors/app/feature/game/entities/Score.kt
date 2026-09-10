package com.steelvectors.app.feature.game.entities

import com.steelvectors.app.feature.game.base.GameplayState
import com.steelvectors.app.feature.game.base.UpdatableView
import korlibs.image.bitmap.Bitmap
import korlibs.image.bitmap.BitmapSlice
import korlibs.image.bitmap.slice
import korlibs.korge.animate.animator
import korlibs.korge.animate.hide
import korlibs.korge.animate.show
import korlibs.korge.internal.KorgeInternal
import korlibs.korge.render.RenderContext
import korlibs.korge.view.Container
import korlibs.korge.view.image
import korlibs.korge.view.position
import korlibs.korge.view.setText
import korlibs.korge.view.text
import korlibs.math.geom.Point
import korlibs.time.seconds

class Score(
    container: Container,
    heartBitmap: Bitmap,
    screenWidth: Float,
    screenHeight: Float,
    scoreFrameBitmap: Bitmap,
    starIconBitmap: Bitmap,
    pumpkinIconBitmap: Bitmap,
    lifeIconBitmap: Bitmap
) : UpdatableView(screenWidth, screenHeight) {

    var value = 0
    var livesCount = DEFAULT_LIVES
    var position = Point(DEFAULT_POSITION_X, DEFAULT_POSITION_Y)
    private val scoreFrameView = container.image(scoreFrameBitmap).also {
        it.position(position.x - 10, position.y - 5)
        it.scaledWidth = 100.0
        it.scaledHeight = 30.0
        it.smoothing = false
    }
    private val starIconImage = container.image(starIconBitmap).also {
        it.position(scoreFrameView.x - 10, scoreFrameView.y)
        it.scaledWidth = 30.0
        it.scaledHeight = 30.0
        it.smoothing = false
    }
    private val scoreText = container.text(value.toString()).also {
        it.position(starIconImage.x + starIconImage.scaledWidth + 10, starIconImage.y + 6)
    }

    private val enemiesFrameView = container.image(scoreFrameBitmap).also {
        it.position(scoreFrameView.x + scoreFrameView.scaledWidth + 18, scoreFrameView.y)
        it.scaledWidth = 100.0
        it.scaledHeight = 30.0
        it.smoothing = false
    }
    private val enemiesIconImage = container.image(pumpkinIconBitmap).also {
        it.position(enemiesFrameView.x - 10, enemiesFrameView.y)
        it.scaledWidth = 30.0
        it.scaledHeight = 30.0
        it.smoothing = false
    }
    private val enemiesLeftText = container.text("").also {
        it.position(enemiesIconImage.x + enemiesIconImage.scaledWidth + 10, enemiesIconImage.y + 6)
    }

    private val livesFrameView = container.image(scoreFrameBitmap).also {
        it.position(enemiesFrameView.x + scoreFrameView.scaledWidth + 18, enemiesFrameView.y)
        it.scaledWidth = 100.0
        it.scaledHeight = 30.0
        it.smoothing = false
    }
    private val livesIconImage = container.image(lifeIconBitmap).also {
        it.position(livesFrameView.x - 10, livesFrameView.y)
        it.scaledWidth = 30.0
        it.scaledHeight = 30.0
        it.smoothing = false
    }

    private var heartList: MutableList<BitmapSlice<Bitmap>> = mutableListOf()
    private var currentGameplayState = GameplayState.Start


    init {
        for (i in 1..livesCount) {
            heartList.add(heartBitmap.slice())
        }
    }

    override fun update(screenWidth: Float, screenHeight: Float) {
    }

    @OptIn(KorgeInternal::class)
    override fun renderInternal(ctx: RenderContext) {
        if (currentGameplayState == GameplayState.Playing ||
            currentGameplayState == GameplayState.StartLevel ||
            currentGameplayState == GameplayState.Paused
        ) {
            for (i in 0 until DEFAULT_LIVES) {
                val heartPosition = Point(
                    livesIconImage.x + HORIZONTAL_MARGIN + (HORIZONTAL_SPACING * i),
                    livesIconImage.y + 5
                )
                val heartTexture = ctx.getTex(heartList[i])
                if (i < livesCount) {
                    ctx.batch.drawQuad(
                        tex = heartTexture,
                        x = heartPosition.x.toFloat(),
                        y = heartPosition.y.toFloat(),
                        width = 16f,
                        height = 16f,
                        m = globalMatrix
                    )
                }
            }
        }
    }

    fun draw() {
        animator {
            if (currentGameplayState == GameplayState.Playing ||
                currentGameplayState == GameplayState.StartLevel ||
                currentGameplayState == GameplayState.Paused
            ) {
                scoreFrameView.visible = true
                show(scoreFrameView)
                starIconImage.visible = true
                show(starIconImage)
                enemiesFrameView.visible = true
                show(enemiesFrameView)
                enemiesIconImage.visible = true
                show(enemiesIconImage)
                livesFrameView.visible = true
                show(livesFrameView)
                livesIconImage.visible = true
                show(livesIconImage)
                scoreText.visible = true
                show(scoreText)
                enemiesLeftText.visible = true
                show(enemiesLeftText)
            } else {
                hide(scoreText, time = 0.seconds)
                hide(enemiesLeftText, time = 0.seconds)
            }
        }
    }

    fun increaseScoreBy(scorePoints: Int) {
        value += scorePoints
        scoreText.setText(value.toString())
    }

    fun playerHit() {
        if (livesCount - 1 >= 0) {
            livesCount--
        }
    }

    fun addExtraLive() {
        if (livesCount < DEFAULT_LIVES) {
            livesCount++
        } else {
            increaseScoreBy(5)
        }
    }

    fun setEnemiesLeft(count: Int) {
        enemiesLeftText.setText(count.toString())
    }

    fun updateCurrentGameState(currentState: GameplayState) {
        currentGameplayState = currentState
    }

    fun resetScore() {
        value = 0
        livesCount = DEFAULT_LIVES
        scoreText.setText(value.toString())
    }

    fun hide() {
        scoreFrameView.visible = false
        starIconImage.visible = false
        enemiesFrameView.visible = false
        enemiesIconImage.visible = false
        livesFrameView.visible = false
        livesIconImage.visible = false
        scoreText.visible = false
        enemiesLeftText.visible = false
    }

    companion object {
        const val DEFAULT_POSITION_X = 74
        const val DEFAULT_POSITION_Y = 25
        const val HORIZONTAL_SPACING = 22
        const val HORIZONTAL_MARGIN = 35
    }
}
