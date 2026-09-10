package com.steelvectors.app.feature.game

import com.steelvectors.app.feature.game.base.GameplayState
import com.steelvectors.app.feature.game.base.LevelManager
import com.steelvectors.app.feature.game.base.Orchestrator
import com.steelvectors.app.feature.game.base.ScrollingBackground
import com.steelvectors.app.feature.game.entities.Bullet
import com.steelvectors.app.feature.game.entities.BulletDirection
import com.steelvectors.app.feature.game.entities.Enemy
import com.steelvectors.app.feature.game.entities.Explosion
import com.steelvectors.app.feature.game.entities.Player
import com.steelvectors.app.feature.game.entities.PowerUp
import com.steelvectors.app.feature.game.entities.RoundImageButton
import com.steelvectors.app.feature.game.entities.Score
import com.steelvectors.app.localization.getCurrentLocalization
import com.steelvectors.app.platform.AudioPlayer
import korlibs.image.bitmap.Bitmap
import korlibs.image.bitmap.slice
import korlibs.image.color.Colors
import korlibs.image.color.MaterialColors
import korlibs.image.color.RGBA
import korlibs.image.font.Font
import korlibs.image.font.readFont
import korlibs.image.format.readBitmap
import korlibs.io.file.std.resourcesVfs
import korlibs.korge.animate.animator
import korlibs.korge.animate.hide
import korlibs.korge.animate.show
import korlibs.korge.input.onClick
import korlibs.korge.scene.Scene
import korlibs.korge.ui.UIButton
import korlibs.korge.ui.uiButton
import korlibs.korge.view.Container
import korlibs.korge.view.Image
import korlibs.korge.view.SContainer
import korlibs.korge.view.Text
import korlibs.korge.view.image
import korlibs.korge.view.position
import korlibs.korge.view.text
import korlibs.korge.view.visible
import korlibs.math.geom.Point
import korlibs.math.geom.RectCorners
import korlibs.math.geom.Size
import kotlin.time.Duration.Companion.seconds

private const val START_POSITION_Y_OFFSET = 240
private const val KORGE_TEXT = "powered by KorGE"
private const val MENU_BUTTON_WIDTH = 250.0
private const val MENU_BUTTON_HEIGHT = 50.0

class MainGameScreen(
    private val screenWidth: Float,
    private val screenHeight: Float,
    private val buttonBackgroundColor: RGBA = MaterialColors.AMBER_500,
    private val exit: () -> Unit,
    private val drawableResourcesPath: String,
    private val fontResourcesPath: String
) : Scene() {
    private lateinit var player: Player
    private lateinit var score: Score
    private lateinit var scrollingBackground: ScrollingBackground
    private lateinit var cloudsBackground: ScrollingBackground
    private lateinit var orchestrator: Orchestrator
    private lateinit var levelManager: LevelManager
    private val enemies = mutableListOf<Enemy>()
    private val bullets = mutableListOf<Bullet>()
    private val explosions = mutableListOf<Explosion>()
    private val powerUps = mutableListOf<PowerUp>()
    private var currentState = GameplayState.OnMenu

    //private var music: MediaPlayer? = null
    //private var soundPool: SoundPool? = null
    private val audioPlayer = AudioPlayer()
    private var phaserSound: Int? = null
    private var explosionSound: Int? = null

    private lateinit var backgroundImage: Image
    private lateinit var cloudsImage: Image
    private lateinit var logoImage: Image
    private lateinit var playerBitmap: Bitmap
    private lateinit var bulletBitmap: Bitmap
    private lateinit var enemyBulletBitmap: Bitmap
    private lateinit var backgroundBitmap: Bitmap
    private lateinit var cloudsBitmap: Bitmap
    private lateinit var spitfireBitmap: Bitmap
    private var bossBitmapList: MutableList<Bitmap> = mutableListOf()
    private var enemiesBitmapList: MutableList<Bitmap> = mutableListOf()
    private var explosionBitmapList: MutableList<Bitmap> = mutableListOf()
    private var shipThrustersBitmapList: MutableList<Bitmap> = mutableListOf()
    private var powerUpsBitmapList: MutableList<Bitmap> = mutableListOf()

    private lateinit var lucidaSansFont: Font
    private lateinit var loadingText: Text
    private lateinit var levelText: Text
    private lateinit var gameOverText: Text
    private lateinit var finalScoreText: Text
    private lateinit var poweredByKorge: Text

    private lateinit var moveLeftButton: RoundImageButton
    private lateinit var moveRightButton: RoundImageButton

    private var startButton: UIButton? = null
    private var pauseButton: UIButton? = null
    private var restartButton: UIButton? = null
    private var exitButton: UIButton? = null
    private val localization = getCurrentLocalization()

    override suspend fun SContainer.sceneInit() {
        // BLOCK. This is called to setup the scene. **Nothing will be shown until this method completes**.
        // Here you can read and wait for resources. No need to call super.
        backgroundBitmap = resourcesVfs["$drawableResourcesPath/background_brown.png"].readBitmap()
        cloudsBitmap = resourcesVfs["$drawableResourcesPath/clouds.png"].readBitmap()
        lucidaSansFont = resourcesVfs["$fontResourcesPath/poppins_black.ttf"].readFont()

        loadingText = text(localization.loading.uppercase()).also {
            it.position(screenWidth / 2 - it.width / 2, screenHeight - 70)
        }

        backgroundImage = image(backgroundBitmap)
        backgroundImage.scaledWidth = screenWidth.toDouble()
        scrollingBackground = ScrollingBackground(backgroundImage, screenWidth, screenHeight)

        cloudsImage = image(cloudsBitmap)
        cloudsImage.scaledWidth = screenWidth.toDouble()
        cloudsBackground = ScrollingBackground(
            cloudsImage,
            screenWidth,
            screenHeight,
            5
        )

        addChild(backgroundImage)
        addChild(scrollingBackground)
        addChild(cloudsBackground)
    }

    override suspend fun SContainer.sceneMain() {
        // DO NOT BLOCK. This is called as a main method of the scene. This is called after [sceneInit].
        // This method doesn't need to complete as long as it suspends.
        // Its underlying job will be automatically closed on the [sceneAfterDestroy]. No need to call super.
        loadAssets()
        loadGameAssets()
    }

    override suspend fun sceneAfterInit() {
        // DO NOT BLOCK. Called after the old scene has been destroyed and the transition has been completed.
    }

    override suspend fun sceneBeforeLeaving() {
        // BLOCK. Called on the old scene after the new scene has been
        // initialized, and before the transition is performed.
    }

    override suspend fun sceneDestroy() {
        // BLOCK. Called on the old scene after the transition
        // has been performed, and the old scene is not visible anymore.
    }

    override suspend fun sceneAfterDestroy() {
        // DO NOT BLOCK. Called on the old scene after the transition has been performed, and the old scene is not visible anymore.
        //
        // At this stage the scene [coroutineContext] [Job] will be cancelled.
        // Stopping [sceneMain] and other [launch] methods using this scene [CoroutineScope].
    }

    override fun onSizeChanged(size: Size) {
        super.onSizeChanged(size)
        // Do something here if the scene size is changed
    }


    private fun Container.singleShot() {
        val bullet = Bullet(player.position, bulletBitmap, BulletDirection.UP)
        bullets.add(bullet)
        addChild(bullet)
        phaserSound?.let {
            audioPlayer.playSound(it)
        }

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
        phaserSound?.let {
            audioPlayer.playSound(it)
        }
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
        phaserSound?.let {
            audioPlayer.playSound(it)
        }
        //soundPool?.play(phaserSound, 0.5f, 0.5f, 0, 0, 1f)
    }

    private fun playMusic() {
        /*music?.let {
            if (!it.isPlaying) {
                it.start()
            }
        }*/
    }

    private fun pauseMusic() {
        /*music?.let {
            if (it.isPlaying) {
                it.pause()
            }
        }*/
    }

    private fun Container.displayLevelText() {
        animator {
            levelText = text(localization.gameLevel + (levelManager.currentLevel).toString()).also {
                it.textSize = 40.0
                it.position(screenWidth / 2 - it.width / 2, screenHeight / 2)
            }
            show(levelText, time = 2.seconds)
            hide(levelText, time = 0.5.seconds)
        }
    }

    private fun Container.onStartGame() {
        hideMenu()
        currentState = GameplayState.StartLevel
        addChild(player)
        addChild(orchestrator)
        loadPauseButton()
        orchestrator.restartGame()
        moveLeftButton.visible = true
        moveRightButton.visible = true
        score.draw()
    }

    private fun Container.onGameOver() {
        hidePauseButton()
        hideControls()
        loadRestartButton()
        displayGameOverText()
        score.hide()
    }

    private fun onRestartGame() {
        hideRestartButton()
        hideGameOverText()
        displayPauseButton()
        displayControls()
        score.draw()
    }

    private fun displayPauseButton() {
        pauseButton?.visible = true
    }

    private fun displayControls() {
        moveLeftButton.visible = true
        moveRightButton.visible = true
    }

    private fun Container.displayGameOverText() {
        animator {
            gameOverText.visible = true
            show(gameOverText, time = 2.seconds)

            finalScoreText.also {
                it.text = localization.gameFinalScore + score.value.toString()
                it.position(screenWidth / 2 - it.size.width / 2, screenHeight / 2 + 50)
                it.visible = true
            }
            show(finalScoreText, time = 2.seconds)
        }
    }

    private fun hideGameOverText() {
        gameOverText.visible = false
        finalScoreText.visible = false
    }

    private fun hideRestartButton() {
        restartButton?.visible = false
    }

    private fun hidePauseButton() {
        pauseButton?.visible = false
    }

    private fun hideControls() {
        moveLeftButton.visible = false
        moveRightButton.visible = false
    }

    private fun hideMenu() {
        logoImage.visible = false
        startButton?.visible = false
        exitButton?.visible = false
        loadingText.visible = false
    }

    private suspend fun Container.loadAssets() {
        val logoBitmap =
            resourcesVfs["$drawableResourcesPath/logo_steel_vectors_menu.png"].readBitmap()
        playerBitmap = resourcesVfs["$drawableResourcesPath/green_04.png"].readBitmap()
        spitfireBitmap =
            resourcesVfs["$drawableResourcesPath/Spitfire_Spritesheet.png"].readBitmap()
        bulletBitmap = resourcesVfs["$drawableResourcesPath/bullet3.png"].readBitmap()
        enemyBulletBitmap = resourcesVfs["$drawableResourcesPath/bullet1.png"].readBitmap()
        val scoreFrameBitmap = resourcesVfs["$drawableResourcesPath/score_frame.png"].readBitmap()
        val starIconBitmap = resourcesVfs["$drawableResourcesPath/star_icon.png"].readBitmap()
        val pumpkinIconBitmap = resourcesVfs["$drawableResourcesPath/pumpkin_icon.png"].readBitmap()
        val lifeIconBitmap = resourcesVfs["$drawableResourcesPath/life_icon.png"].readBitmap()
        score = Score(
            container = this,
            screenWidth = screenWidth,
            screenHeight = screenHeight,
            scoreFrameBitmap = scoreFrameBitmap,
            heartBitmap = resourcesVfs["$drawableResourcesPath/heart.png"].readBitmap(),
            starIconBitmap = starIconBitmap,
            pumpkinIconBitmap = pumpkinIconBitmap,
            lifeIconBitmap = lifeIconBitmap
        )
        score.hide()

        finalScoreText = text("").also {
            it.position(screenWidth / 2 - it.size.width / 2, screenHeight / 2 + 50)
        }
        finalScoreText.visible(false)

        gameOverText = text(localization.gameGameOver).also {
            it.textSize = 40.0
            it.position(screenWidth / 2 - it.size.width / 2, screenHeight / 2)
        }
        gameOverText.visible(false)

        poweredByKorge = text(KORGE_TEXT).also {
            it.position(screenWidth / 2 - it.size.width / 2, screenHeight - 25)
        }

        logoImage = image(logoBitmap)
        logoImage.also {
            it.scaledWidth = screenWidth * 0.8
            it.scaledHeight = screenWidth * 0.8
            it.position(screenWidth / 2 - it.scaledWidth / 2, screenHeight / 11)
        }

        loadingText.visible = true
        addChild(loadingText)
    }

    private suspend fun Container.loadGameAssets() {
        loadMusic()
        loadSoundFX()
        loadExplosionAnimation()
        loadShipThrustersAnimation()
        loadBossBitmaps()
        loadEnemiesBitmaps()
        loadPowerUpsBitmaps()

        levelManager = LevelManager(
            bossBitmapList,
            enemiesBitmapList,
            enemyBulletBitmap,
            bullets,
            this,
            //soundPool,
            phaserSound,
            screenWidth,
            screenHeight
        )

        score.updateCurrentGameState(currentState)

        player = Player(
            Point(
                views.virtualWidth / 2 - playerBitmap.slice().width / 2,
                views.virtualHeight - START_POSITION_Y_OFFSET
            ),
            spitfireBitmap,
            screenWidth,
            screenHeight
        )
        player.playing()

        orchestrator = Orchestrator(
            container = this,
            player = player,
            enemies = enemies,
            bullets = bullets,
            explosions = explosions,
            explosionBitmapList = explosionBitmapList,
            powerUps = powerUps,
            powerUpsBitmapList = powerUpsBitmapList,
            score = score,
            //soundPool,
            explosionSound = explosionSound,
            screenWidth = screenWidth,
            screenHeight = screenHeight,
            scrollingBackground = scrollingBackground,
            cloudsBackground = cloudsBackground,
            bulletBitmap = bulletBitmap,
            levelManager = levelManager,
            onStartLevel = { displayLevelText() },
            onGameOver = { onGameOver() },
            onRestart = { onRestartGame() }
        )

        loadMoveLeftButton()
        //loadShotButton()
        loadMoveRightButton()

        addChild(poweredByKorge)
        addChild(gameOverText)
        addChild(finalScoreText)
        addChild(score)
        addChild(logoImage)

        loadStartButton()
        loadExitButton()

        loadingText.visible = false
    }

    private fun Container.loadPauseButton() {
        if (pauseButton == null) {
            pauseButton = uiButton(localization.gamePause).also {
                it.position(screenWidth - it.size.width - 12, 60)
                it.bgColorOut = buttonBackgroundColor
                it.bgColorOver = MaterialColors.AMBER_800
                it.textColor = Colors.WHITE
                it.background.radius = RectCorners(16f, 16f, 16f, 16f)
            }.onClick {
                when (orchestrator.currentState) {
                    GameplayState.Paused -> {
                        orchestrator.currentState = GameplayState.Playing
                        pauseButton?.text = localization.gamePause
                        displayControls()
                    }

                    GameplayState.GameOver -> {
                        orchestrator.restartGame()
                    }

                    else -> {
                        orchestrator.currentState = GameplayState.Paused
                        pauseButton?.text = localization.gameUnPause
                        hideControls()
                    }
                }
            }
        }
    }

    private suspend fun loadPowerUpsBitmaps() {
        powerUpsBitmapList.add(resourcesVfs["$drawableResourcesPath/heart_power_up.png"].readBitmap())
        powerUpsBitmapList.add(resourcesVfs["$drawableResourcesPath/shot_power_up.png"].readBitmap())
    }

    private suspend fun loadEnemiesBitmaps() {
        enemiesBitmapList.add(resourcesVfs["$drawableResourcesPath/BF_109_Spritesheet.png"].readBitmap())
        enemiesBitmapList.add(resourcesVfs["$drawableResourcesPath/FW_190_Spritesheet.png"].readBitmap())
        enemiesBitmapList.add(resourcesVfs["$drawableResourcesPath/P_38_Spritesheet.png"].readBitmap())
        enemiesBitmapList.add(resourcesVfs["$drawableResourcesPath/Huricane_Spritesheet.png"].readBitmap())
        enemiesBitmapList.add(resourcesVfs["$drawableResourcesPath/Yak_3_Spritesheet.png"].readBitmap())
        enemiesBitmapList.add(resourcesVfs["$drawableResourcesPath/Zero_Spritesheet.png"].readBitmap())
    }

    private suspend fun loadBossBitmaps() {
        bossBitmapList.add(resourcesVfs["$drawableResourcesPath/large_red_01.png"].readBitmap())
        bossBitmapList.add(resourcesVfs["$drawableResourcesPath/large_blue_01.png"].readBitmap())
        bossBitmapList.add(resourcesVfs["$drawableResourcesPath/large_blue_02.png"].readBitmap())
        bossBitmapList.add(resourcesVfs["$drawableResourcesPath/large_green_01.png"].readBitmap())
        bossBitmapList.add(resourcesVfs["$drawableResourcesPath/large_grey_01.png"].readBitmap())
        bossBitmapList.add(resourcesVfs["$drawableResourcesPath/large_grey_02.png"].readBitmap())
        bossBitmapList.add(resourcesVfs["$drawableResourcesPath/large_purple_01.png"].readBitmap())
    }

    private fun Container.loadStartButton() {
        if (startButton == null) {
            startButton = uiButton(localization.gameStartGame).also {
                it.width = MENU_BUTTON_WIDTH
                it.height = MENU_BUTTON_HEIGHT
                it.position(screenWidth / 2 - it.size.width / 2, screenHeight / 2 + 70)
                it.bgColorOut = buttonBackgroundColor
                it.bgColorOver = MaterialColors.AMBER_800
                it.textColor = Colors.WHITE
                it.textSize = 20.0
                it.background.radius = RectCorners(16f, 16f, 16f, 16f)
            }.onClick {
                onStartGame()
                addChild(orchestrator)
                orchestrator.restartGame()
            }
        }
    }

    private fun Container.loadExitButton() {
        if (exitButton == null) {
            exitButton = uiButton(localization.gameExit).also {
                it.width = MENU_BUTTON_WIDTH
                it.height = MENU_BUTTON_HEIGHT
                it.position(screenWidth / 2 - it.size.width / 2, screenHeight / 2 + 150)
                it.bgColorOut = buttonBackgroundColor
                it.bgColorOver = MaterialColors.AMBER_800
                it.textColor = Colors.WHITE
                it.textSize = 20.0
                it.background.radius = RectCorners(16f, 16f, 16f, 16f)
            }.onClick {
                exit.invoke()
            }
        }
    }

    private fun Container.loadRestartButton() {
        if (restartButton == null) {
            restartButton = uiButton(localization.gameRestart).also {
                it.width = MENU_BUTTON_WIDTH
                it.height = MENU_BUTTON_HEIGHT
                it.position(screenWidth / 2 - it.size.width / 2, screenHeight / 2 + 90)
                it.bgColorOut = buttonBackgroundColor
                it.bgColorOver = MaterialColors.AMBER_800
                it.textColor = Colors.WHITE
                it.textSize = 20.0
                it.background.radius = RectCorners(16f, 16f, 16f, 16f)
            }.onClick {
                orchestrator.restartGame()
            }
        } else {
            restartButton?.visible = true
        }
    }

    /*
        private suspend fun Container.loadListingButton() {
            listingButton = Button(
                position = Point(8, 220),
                text = LISTING_BUTTON_TEXT,
                onButton = { pressed ->
                    if (pressed && listingButton.visible) {
                        //(androidConText as GameActivity).navigateToVideoGamesListing()
                    }
                },
                //size = Pair(300.0, 26.0),
                bitmapFont = lucidaSansFont.toBitmapFont(
                    fontSize = 32.0,
                    chars = CharacterSet.LATIN_ALL,
                    paint = Colors.WHITE,
                    mipmaps = true,
                    effect = BitmapEffect(
                        dropShadowX = 2, dropShadowY = 1, dropShadowRadius = 2,
                        borderSize = 2, borderColor = Colors.RED,
                    )
                )
            )
            //listingButton.hide(time = 0.seconds)
            listingButton.visible = false
            addChild(listingButton)
        }
    */
    private suspend fun Container.loadMoveLeftButton() {
        val containerWidth = views.virtualWidth.toDouble()
        val containerHeight = views.virtualHeight.toDouble()
        val radius = containerHeight / 18
        moveLeftButton = RoundImageButton(
            position = Point(containerWidth / 4, containerHeight - containerHeight / 10),
            bitmap = resourcesVfs["$drawableResourcesPath/arrow.png"].readBitmap(),
            onButton = { pressed ->
                if (pressed &&
                    currentState != GameplayState.Paused &&
                    currentState != GameplayState.GameOver
                ) {
                    player.moveLeft()
                } else {
                    player.stop()
                }
            },
            buttonRadius = radius
        )
        moveLeftButton.visible = false
        addChild(moveLeftButton)
    }

    /*private suspend fun Container.loadShotButton() {
        val containerWidth = views.virtualWidth.toDouble()
        val containerHeight = views.virtualHeight.toDouble()
        val radius = containerHeight / 18
        val shotButtonBitmap = resourcesVfs["$resourcesPath/drawable/bullet3_button.png"].readBitmap()
        shotButton = RoundImageButton(
            position = Point(containerWidth / 2, containerHeight - containerHeight / 10),
            bitmap = shotButtonBitmap,
            onButton = { pressed ->
                if (pressed &&
                    currentState != GameplayState.Paused &&
                    currentState != GameplayState.GameOver
                ) {
                    when (player.shotType) {
                        ShotType.SINGLE -> singleShot()
                        ShotType.DOUBLE -> doubleShot()
                        ShotType.TRIPLE -> tripleShot()
                    }
                }
            },
            buttonRadius = radius
        )
        shotButton.visible = false
        addChild(shotButton)
    }*/

    private suspend fun Container.loadMoveRightButton() {
        val containerWidth = views.virtualWidth.toDouble()
        val containerHeight = views.virtualHeight.toDouble()
        val radius = containerHeight / 18
        moveRightButton = RoundImageButton(
            position = Point(
                containerWidth - (containerWidth / 4),
                containerHeight - containerHeight / 10
            ),
            bitmap = resourcesVfs["$drawableResourcesPath/arrow.png"].readBitmap(),
            onButton = { pressed ->
                if (pressed &&
                    currentState != GameplayState.Paused &&
                    currentState != GameplayState.GameOver
                ) {
                    player.moveRight()
                } else {
                    player.stop()
                }
            },
            flipHorizontal = true,
            buttonRadius = radius
        )
        moveRightButton.visible = false
        addChild(moveRightButton)
    }

    private suspend fun loadShipThrustersAnimation() {
        val thrustersAnim1 =
            resourcesVfs["$drawableResourcesPath/vertical_thrust_01.png"].readBitmap()
        shipThrustersBitmapList.add(thrustersAnim1)
        val thrustersAnim2 =
            resourcesVfs["$drawableResourcesPath/vertical_thrust_02.png"].readBitmap()
        shipThrustersBitmapList.add(thrustersAnim2)
        val thrustersAnim3 =
            resourcesVfs["$drawableResourcesPath/vertical_thrust_03.png"].readBitmap()
        shipThrustersBitmapList.add(thrustersAnim3)
        val thrustersAnim4 =
            resourcesVfs["$drawableResourcesPath/vertical_thrust_04.png"].readBitmap()
        shipThrustersBitmapList.add(thrustersAnim4)
    }

    private suspend fun loadExplosionAnimation() {
        val explosionAnim1 = resourcesVfs["$drawableResourcesPath/explosion_01.png"].readBitmap()
        explosionBitmapList.add(explosionAnim1)
        val explosionAnim2 = resourcesVfs["$drawableResourcesPath/explosion_02.png"].readBitmap()
        explosionBitmapList.add(explosionAnim2)
        val explosionAnim3 = resourcesVfs["$drawableResourcesPath/explosion_03.png"].readBitmap()
        explosionBitmapList.add(explosionAnim3)
        val explosionAnim4 = resourcesVfs["$drawableResourcesPath/explosion_04.png"].readBitmap()
        explosionBitmapList.add(explosionAnim4)
        val explosionAnim5 = resourcesVfs["$drawableResourcesPath/explosion_05.png"].readBitmap()
        explosionBitmapList.add(explosionAnim5)
        val explosionAnim6 = resourcesVfs["$drawableResourcesPath/explosion_06.png"].readBitmap()
        explosionBitmapList.add(explosionAnim6)
        val explosionAnim7 = resourcesVfs["$drawableResourcesPath/explosion_07.png"].readBitmap()
        explosionBitmapList.add(explosionAnim7)
        val explosionAnim8 = resourcesVfs["$drawableResourcesPath/explosion_08.png"].readBitmap()
        explosionBitmapList.add(explosionAnim8)
        val explosionAnim9 = resourcesVfs["$drawableResourcesPath/explosion_09.png"].readBitmap()
        explosionBitmapList.add(explosionAnim9)
        val explosionAnim10 = resourcesVfs["$drawableResourcesPath/explosion_10.png"].readBitmap()
        explosionBitmapList.add(explosionAnim10)
        val explosionAnim11 = resourcesVfs["$drawableResourcesPath/explosion_11.png"].readBitmap()
        explosionBitmapList.add(explosionAnim11)
    }

    private fun loadSoundFX() {
        //phaserSound = audioPlayer.loadSound(Res.getUri("raw/phaser2.wav"))
        //explosionSound = audioPlayer.loadSound("$resourcesPath/drawable/phaser2.wav")
        /*soundPool = SoundPool.Builder().setMaxStreams(2).build()
        soundPool?.let {
            phaserSound = it.load(androidConText.applicationContext, R.raw.phaser2, 1)
            explosionSound = it.load(androidConText.applicationContext, R.raw.explosion1, 1)
        }*/
    }

    private fun loadMusic() {
        //audioPlayer.loadMusic("$resourcesPath/drawable/music.mp3")
        /*music = MediaPlayer.create(androidConText.applicationContext, R.raw.music)
        music?.let { mediaPlayer ->
            mediaPlayer.setVolume(0.8f, 0.8f)
            mediaPlayer.setOnCompletionListener {
                it.start()
            }
        }*/
    }
    /*
        fun stop() {
            /*music?.release()
            music = null
            soundPool?.release()
            soundPool = null*/
            backgroundImage.reset()
        }

        fun pause() {
            pauseMusic()
            //soundPool?.autoPause()
            if (currentState == GameplayState.Playing) {
                currentState = GameplayState.Paused
            }
        }

        fun resume() {
            if (currentState == GameplayState.Playing) {
                playMusic()
                //soundPool?.autoResume()
            }
        }
     */
}
