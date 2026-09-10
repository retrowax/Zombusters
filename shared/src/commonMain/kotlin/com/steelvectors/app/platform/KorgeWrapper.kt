package com.steelvectors.app.platform

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.Dp
import com.steelvectors.app.feature.game.MainGameScreen
import korlibs.image.color.RGBA
import korlibs.korge.Korge
import korlibs.math.geom.Size

@Composable
expect fun KorgeView(modifier: Modifier = Modifier, exit: () -> Unit)

class KorgeWrapper(
    screenWidth: Dp,
    screenHeight: Dp,
    buttonBackgroundColor: RGBA,
    exit: () -> Unit,
    drawableResourcesPath: String,
    fontResourcesPath: String
) {
    private val korgeModule = Korge(
        mainSceneClass = MainGameScreen::class,
        virtualSize = Size(screenWidth.value, screenHeight.value),
        windowSize = Size(screenWidth.value, screenHeight.value),
        configInjector = {
            //mapPrototype { SplashScreen(screenWidth.value, screenHeight.value) }
            //mapPrototype { MenuScreen(screenWidth.value, screenHeight.value, exit) }
            mapPrototype {
                MainGameScreen(
                    screenWidth = screenWidth.value,
                    screenHeight = screenHeight.value,
                    buttonBackgroundColor = buttonBackgroundColor,
                    exit = exit,
                    drawableResourcesPath = drawableResourcesPath,
                    fontResourcesPath = fontResourcesPath
                )
            }
        })

    fun getKorgeConfig(): Korge {
        return korgeModule
    }
}
