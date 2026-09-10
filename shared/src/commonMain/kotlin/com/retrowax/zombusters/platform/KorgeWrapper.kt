package com.retrowax.zombusters.platform

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.Dp
import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.scenes.MainMenuScene
import korlibs.image.color.RGBA
import korlibs.korge.Korge
import korlibs.math.geom.Size

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
        mainSceneClass = MainMenuScene::class,
        virtualSize = Size(GAME_WIDTH, GAME_HEIGHT),
        windowSize = Size(windowWidth.value, windowHeight.value),
        configInjector = {
            mapPrototype {
                MainMenuScene(
                    exit = exit,
                    drawableResourcesPath = drawableResourcesPath,
                    fontResourcesPath = fontResourcesPath,
                    filesResourcesPath = filesResourcesPath
                )
            }
        })

    fun getKorgeConfig(): Korge = korgeModule
}
