package com.retrowax.zombusters.platform

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.Dp
import com.retrowax.zombusters.game.scenes.MainGameScene
import korlibs.image.color.RGBA
import korlibs.korge.Korge
import korlibs.math.geom.Size

const val GAME_WIDTH = 1280f
const val GAME_HEIGHT = 720f

@Composable
expect fun KorgeView(modifier: Modifier = Modifier, exit: () -> Unit)

class KorgeWrapper(
    windowWidth: Dp,
    windowHeight: Dp,
    buttonBackgroundColor: RGBA,
    exit: () -> Unit,
    drawableResourcesPath: String,
    fontResourcesPath: String,
    filesResourcesPath: String = ""
) {
    private val korgeModule = Korge(
        mainSceneClass = MainGameScene::class,
        virtualSize = Size(GAME_WIDTH, GAME_HEIGHT),
        windowSize = Size(windowWidth.value, windowHeight.value),
        configInjector = {
            mapPrototype {
                MainGameScene(
                    buttonBackgroundColor = buttonBackgroundColor,
                    exit = exit,
                    drawableResourcesPath = drawableResourcesPath,
                    fontResourcesPath = fontResourcesPath,
                    filesResourcesPath = filesResourcesPath
                )
            }
        })

    fun getKorgeConfig(): Korge = korgeModule
}
