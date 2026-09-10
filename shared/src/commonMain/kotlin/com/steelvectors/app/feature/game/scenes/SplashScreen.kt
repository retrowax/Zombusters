package com.steelvectors.app.feature.game.scenes

import korlibs.image.format.readBitmap
import korlibs.io.file.std.resourcesVfs
import korlibs.korge.scene.Scene
import korlibs.korge.view.Image
import korlibs.korge.view.SContainer
import korlibs.korge.view.image
import korlibs.time.seconds
import kotlinx.coroutines.delay

private const val RESOURCES_MAIN_PATH = "composeResources/com.steelvectors.app.resources"

class SplashScreen(private val screenWidth: Float, private val screenHeight: Float) : Scene() {

    private lateinit var logoFullScreen: Image

    override suspend fun SContainer.sceneInit() {
        logoFullScreen =
            image(resourcesVfs["$RESOURCES_MAIN_PATH/drawable/logo_steel_vectors.png"].readBitmap())
        logoFullScreen.scaledWidth = screenWidth.toDouble()
        logoFullScreen.scaledHeight = screenHeight.toDouble()
        addChildAt(logoFullScreen, 0)
    }

    override suspend fun SContainer.sceneMain() {
        delay(1.seconds)
        sceneContainer.changeTo<MenuScreen>(screenWidth, screenHeight)
    }
}
