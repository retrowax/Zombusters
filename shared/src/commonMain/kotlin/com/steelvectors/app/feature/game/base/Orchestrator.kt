package com.steelvectors.app.feature.game.base

import com.steelvectors.app.feature.game.entities.Bullet
import com.steelvectors.app.feature.game.entities.BulletDirection
import com.steelvectors.app.feature.game.entities.Enemy
import com.steelvectors.app.feature.game.entities.EnemyStatus
import com.steelvectors.app.feature.game.entities.EnemyType
import com.steelvectors.app.feature.game.entities.Explosion
import com.steelvectors.app.feature.game.entities.Player
import com.steelvectors.app.feature.game.entities.PowerUp
import com.steelvectors.app.feature.game.entities.Score
import com.steelvectors.app.feature.game.entities.ShotType
import korlibs.image.bitmap.Bitmap
import korlibs.korge.render.RenderContext
import korlibs.korge.view.Container
import korlibs.math.geom.Point
import kotlin.random.Random

private const val SHOOT_RATE = 20

class Orchestrator(
    private val container: Container,
    private var player: Player,
    private var enemies: MutableList<Enemy>,
    private var bullets: MutableList<Bullet>,
    private var explosions: MutableList<Explosion>,
    private var explosionBitmapList: List<Bitmap>,
    private var powerUps: MutableList<PowerUp>,
    private var powerUpsBitmapList: List<Bitmap>,
    private val score: Score,
    //private val soundPool: SoundPool?,
    private val explosionSound: Int?,
    screenWidth: Float,
    screenHeight: Float,
    private val scrollingBackground: ScrollingBackground,
    private val cloudsBackground: ScrollingBackground,
    private val bulletBitmap: Bitmap,
    private val levelManager: LevelManager,
    private val onStartLevel: () -> Unit,
    private val onGameOver: () -> Unit,
    private val onRestart: () -> Unit
) : UpdatableView(screenWidth, screenHeight) {
    private var frame = 0

    override fun update(screenWidth: Float, screenHeight: Float) {
        if (currentState == GameplayState.Playing) {
            frame++
            if (enemies != null && bullets != null) {
                val enemiesIterator = enemies.iterator()
                while (enemiesIterator.hasNext()) {
                    val enemy = enemiesIterator.next()

                    val bulletsIterator = bullets.iterator()
                    while (bulletsIterator.hasNext()) {
                        val bullet = bulletsIterator.next()
                        if (enemy.isCollidingWith(bullet) && bullet.direction == BulletDirection.UP) {
                            if (enemy.status == EnemyStatus.ALIVE) {
                                enemy.hit()
                                if (enemy.strength == 0) {
                                    removeEnemy(enemy)
                                    removeBullet(bullet)
                                    updateScore(enemy.type)
                                    addExplosionAt(enemy.position)
                                    addPowerUpAt(enemy.position)
                                    enemy.status = EnemyStatus.DEAD
                                } else {
                                    removeBullet(bullet)
                                    addExplosionAt(bullet.position)
                                }
                            }
                        }
                        if (bullet.position.y < 0 || bullet.position.y > container.windowBounds.height) {
                            removeBullet(bullet)
                        }
                        if (bullet.isCollidingWith(player) && bullet.direction == BulletDirection.DOWN) {
                            player.hit()
                            score.playerHit()
                            removeBullet(bullet)
                            addExplosionAt(player.position)
                        }
                    }

                    if (enemy.isCollidingWith(player)) {
                        if (enemy.status == EnemyStatus.ALIVE) {
                            player.hit()
                            removeEnemy(enemy)
                            updateScore(enemy.type)
                            score.playerHit()
                            addExplosionAt(enemy.position)
                            addExplosionAt(player.position)
                            enemy.status = EnemyStatus.DEAD
                        }
                    }
                }

                if (frame.rem(SHOOT_RATE) == 0) {
                    when (player.shotType) {
                        ShotType.SINGLE -> container.singleShot()
                        ShotType.DOUBLE -> container.doubleShot()
                        ShotType.TRIPLE -> container.tripleShot()
                    }
                }
            }

            checkPowerUps()
            removeExplosionsAfterAnimationEnds()
        }

        when (currentState) {
            GameplayState.Start -> {
                //loadGameAssets()
                //currentState = GameplayState.StartLevel
            }

            GameplayState.StartLevel -> {
                startLevel()
            }

            GameplayState.Playing -> {
                playingGame()
            }

            GameplayState.Paused -> {
                pauseGame()
            }

            GameplayState.GameOver -> {
                gameOver()
            }

            GameplayState.Exit -> {
                //stop()
            }

            GameplayState.OnMenu -> {

            }
        }
    }

    private fun startLevel() {
        player.playing()
        enemies.addAll(levelManager.getLevelEnemies())
        score.updateCurrentGameState(currentState)
        score.setEnemiesLeft(enemies.count())
        score.draw()

        onStartLevel.invoke()
        //(androidConText as GameActivity).onLevelStarted(levelManager.currentLevel)
        currentState = GameplayState.Playing
    }

    private fun playingGame() {
        if (score != null) {
            score.updateCurrentGameState(currentState)
        }
        //playMusic()
        resumeGame()
        if (player != null) {
            player.playing()
            if (player.livesCount == 0) {
                currentState = GameplayState.GameOver
            }
        }
        if (enemies != null) {
            if (enemies.isEmpty()) {
                levelManager.levelFinished()
                currentState = GameplayState.StartLevel
            }
        }
    }

    private fun pauseGame() {
        //music?.pause()
        score.updateCurrentGameState(currentState)
        paused()
        scrollingBackground.paused()
        cloudsBackground.paused()
        player.paused()
        for (enemy in enemies) {
            enemy.paused()
        }
        for (bullet in bullets) {
            bullet.paused()
        }
        for (explosion in explosions) {
            explosion.paused()
        }
        for (powerUp in powerUps) {
            powerUp.paused()
        }
    }

    private fun resumeGame() {
        //orchestrator.playing()
        if (scrollingBackground != null) {
            scrollingBackground.playing()
        }
        if (cloudsBackground != null) {
            cloudsBackground.playing()
        }
        if (player != null) {
            player.playing()
        }
        if (enemies != null) {
            for (enemy in enemies) {
                enemy.playing()
            }
        }
        if (bullets != null) {
            for (bullet in bullets) {
                bullet.playing()
            }
        }
        if (explosions != null) {
            for (explosion in explosions) {
                explosion.playing()
            }
        }
        if (powerUps != null) {
            for (powerUp in powerUps) {
                powerUp.playing()
            }
        }
    }

    private fun Container.singleShot() {
        val bullet = Bullet(player.position, bulletBitmap, BulletDirection.UP)
        bullets.add(bullet)
        addChild(bullet)
        /*phaserSound?.let {
          audioPlayer.playSound(it)
        }*/

        //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
    }

    private fun Container.doubleShot() {
        val bullet1 = Bullet(
            Point(player.position.x - 10, player.position.y),
            bulletBitmap,
            BulletDirection.UP
        )
        val bullet2 = Bullet(
            Point(player.position.x + 10, player.position.y),
            bulletBitmap,
            BulletDirection.UP
        )
        bullets.add(bullet1)
        bullets.add(bullet2)
        addChild(bullet1)
        addChild(bullet2)
        /*phaserSound?.let {
          audioPlayer.playSound(it)
        }*/
        //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
    }

    private fun Container.tripleShot() {
        val bullet1 = Bullet(
            Point(player.position.x - 15, player.position.y),
            bulletBitmap,
            BulletDirection.UP
        )
        val bullet2 =
            Bullet(Point(player.position.x, player.position.y), bulletBitmap, BulletDirection.UP)
        val bullet3 = Bullet(
            Point(player.position.x + 15, player.position.y),
            bulletBitmap,
            BulletDirection.UP
        )
        bullets.add(bullet1)
        bullets.add(bullet2)
        bullets.add(bullet3)
        addChild(bullet1)
        addChild(bullet2)
        addChild(bullet3)
        /*phaserSound?.let {
          audioPlayer.playSound(it)
        }*/
        //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
    }

    private fun gameOver() {
        clearScene()
        /*moveLeftButton.visible = false
        shotButton.visible = false
        moveRightButton.visible = false
        player.visible = false*/
        score.updateCurrentGameState(currentState)
        player.currentState = currentState
        /*gameOverText.visible = true
        finalScoreText.text = score.value.toString()
        finalScoreText.position(190 - (score.value.toString().length * 14), TEXT_CENTER_POSITION_Y - TEXT_OFFSET)
        finalScoreText.visible = true
        catchPhraseText.visible = true
        listingButton.visible = true
        restartButton.visible = true*/
        onGameOver.invoke()
    }

    fun restartGame() {
        score.resetScore()
        levelManager.restart()
        player.restart()
        currentState = GameplayState.StartLevel
        player.currentState = currentState
        onRestart.invoke()
        startLevel()
    }

    private fun clearScene() {
        for (enemy in enemies) {
            if (container.children.contains(enemy)) {
                container.removeChild(enemy)
            }
        }
        enemies.clear()
        for (bullet in bullets) {
            if (container.children.contains(bullet)) {
                container.removeChild(bullet)
            }
        }
        bullets.clear()
    }

    private fun checkPowerUps() {
        if (powerUps != null) {
            val powerUpsIterator = powerUps.iterator()
            while (powerUpsIterator.hasNext()) {
                val powerUp = powerUpsIterator.next()
                if (powerUp.isCollidingWith(player)) {
                    if (powerUp.type == 0) {
                        player.addExtraLive()
                        score.addExtraLive()
                    } else {
                        if (player.shotType == ShotType.TRIPLE) {
                            score.increaseScoreBy(5)
                        } else {
                            player.activateShotPowerUp()
                        }
                    }
                    removePowerUp(powerUp)
                }

                if (powerUp.position.y > container.windowBounds.height) {
                    removePowerUp(powerUp)
                }
            }
        }
    }

    private fun removePowerUp(powerUp: PowerUp) {
        container.removeChild(powerUp)
        powerUps.remove(powerUp)
    }

    private fun addPowerUpAt(position: Point) {
        if (Random.nextInt(0, 10) == 5) {
            val powerUp = PowerUp(position, powerUpsBitmapList)
            powerUps.add(powerUp)
            container.addChild(powerUp)
        }
    }

    private fun removeExplosionsAfterAnimationEnds() {
        if (explosions != null) {
            val explosionsIterator = explosions.iterator()
            while (explosionsIterator.hasNext()) {
                val explosion = explosionsIterator.next()
                if (explosion.currentAnimationState == AnimationState.ENDED) {
                    container.removeChild(explosion)
                    explosions.remove(explosion)
                }
            }
        }
    }

    private fun addExplosionAt(position: Point) {
        val explosion = Explosion(position, explosionBitmapList)
        explosions.add(explosion)
        //soundPool?.play(explosionSound, 0.5f, 0.5f, 0, 0, 1f)
        container.addChild(explosion)
    }

    private fun updateScore(type: EnemyType) {
        when (type) {
            EnemyType.GUNSHIP -> score.increaseScoreBy(10)
            EnemyType.BOMBER -> score.increaseScoreBy(15)
            EnemyType.FIGHTER -> score.increaseScoreBy(20)
            EnemyType.INTERDICTOR -> score.increaseScoreBy(30)
            EnemyType.RECON -> score.increaseScoreBy(35)
            EnemyType.ASSAULT -> score.increaseScoreBy(50)
            EnemyType.BOSS -> score.increaseScoreBy(400)
        }
        score.setEnemiesLeft(enemies.count())
    }

    private fun removeBullet(bullet: Bullet) {
        container.removeChild(bullet)
        bullets.remove(bullet)
    }

    private fun removeEnemy(enemy: Enemy) {
        container.removeChild(enemy)
        enemies.remove(enemy)
    }

    override fun renderInternal(ctx: RenderContext) {
    }
}
