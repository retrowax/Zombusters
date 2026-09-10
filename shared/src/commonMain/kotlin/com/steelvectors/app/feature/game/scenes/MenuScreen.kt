package com.steelvectors.app.feature.game.scenes

import com.steelvectors.app.feature.game.MainGameScreen
import korlibs.image.color.MaterialColors
import korlibs.image.format.readBitmap
import korlibs.io.file.std.resourcesVfs
import korlibs.korge.input.onClick
import korlibs.korge.scene.Scene
import korlibs.korge.ui.uiButton
import korlibs.korge.view.Image
import korlibs.korge.view.SContainer
import korlibs.korge.view.Text
import korlibs.korge.view.image
import korlibs.korge.view.position
import korlibs.korge.view.text
import korlibs.math.geom.RectCorners
import kotlinx.coroutines.launch

private const val MENU_BUTTON_WIDTH = 250.0
private const val MENU_BUTTON_HEIGHT = 50.0
private const val RESOURCES_MAIN_PATH = "composeResources/com.steelvectors.app.resources"

class MenuScreen(
    private val screenWidth: Float,
    private val screenHeight: Float,
    private val exit: () -> Unit
) : Scene() {

    private lateinit var logoFullScreen: Image
    private lateinit var poweredByKorge: Text

    private val xMenuPosition = (screenWidth / 2) - (MENU_BUTTON_WIDTH / 2)
    private val yMenuPosition = (screenHeight / 2) + 250

    override suspend fun SContainer.sceneInit() {
        logoFullScreen =
            image(resourcesVfs["$RESOURCES_MAIN_PATH/drawable/logo_steel_vectors.png"].readBitmap())
        logoFullScreen.scaledWidth = screenWidth.toDouble()
        logoFullScreen.scaledHeight = screenHeight.toDouble()
        addChild(logoFullScreen)

        poweredByKorge = text("powered by KorGE").also {
            it.position(screenWidth / 2 - it.size.width / 2, screenHeight - 25)
        }
        addChild(poweredByKorge)
    }

    override suspend fun SContainer.sceneMain() {
        uiButton("START GAME").also {
            it.position(xMenuPosition, yMenuPosition)
            it.width = MENU_BUTTON_WIDTH
            it.height = MENU_BUTTON_HEIGHT
            it.bgColorOut = MaterialColors.AMBER_500
            it.bgColorOver = MaterialColors.AMBER_800
            it.textColor = MaterialColors.BLUE_900
            it.background.radius = RectCorners(16f, 16f, 16f, 16f)
        }.onClick {
            startGame()
        }

        uiButton("EXIT").also {
            it.position(xMenuPosition, yMenuPosition + 70)
            it.width = MENU_BUTTON_WIDTH
            it.height = MENU_BUTTON_HEIGHT
            it.bgColorOut = MaterialColors.AMBER_500
            it.bgColorOver = MaterialColors.AMBER_800
            it.textColor = MaterialColors.BLUE_900
            it.background.radius = RectCorners(16f, 16f, 16f, 16f)
        }.onClick {
            exit.invoke()
        }
    }

    private fun startGame() = launch {
        sceneContainer.changeTo<MainGameScreen>(screenWidth, screenHeight)
    }
}
