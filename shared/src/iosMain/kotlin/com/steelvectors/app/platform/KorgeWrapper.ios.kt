package com.steelvectors.app.platform

import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.ExperimentalComposeUiApi
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalWindowInfo
import androidx.compose.ui.viewinterop.UIKitView
import com.steelvectors.app.feature.game.MainGameScreen
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.korge.BaseKorgeIosUIViewProvider
import platform.UIKit.UIScreen

private const val DRAWABLE_RESOURCES_MAIN_PATH =
    "compose-resources/composeResources/com.steelvectors.app.resources/drawable"
private const val FONT_RESOURCES_MAIN_PATH =
    "compose-resources/composeResources/com.steelvectors.app.resources/font"

@OptIn(ExperimentalComposeUiApi::class)
@Composable
actual fun KorgeView(modifier: Modifier, exit: () -> Unit) {
    val screenHeight = LocalWindowInfo.current.containerSize.height.pxToPoint()
    val screenWidth = LocalWindowInfo.current.containerSize.width.pxToPoint()
    val buttonBackgroundColor = MaterialTheme.colorScheme.secondary.toKorgeColor()

    val mainScene = MainGameScreen(
        screenWidth = screenWidth.toFloat(),
        screenHeight = screenHeight.toFloat(),
        buttonBackgroundColor = buttonBackgroundColor,
        exit = exit,
        drawableResourcesPath = DRAWABLE_RESOURCES_MAIN_PATH,
        fontResourcesPath = FONT_RESOURCES_MAIN_PATH
    )

    val viewInfo = SteelVectorsKorgeIosUIViewProvider().createViewInfo(
        mainScene,
        screenWidth.toInt(),
        screenHeight.toInt()
    )

    UIKitView(
        modifier = modifier,
        factory = {
            viewInfo.view
        }
    )
}

@OptIn(ExperimentalStdlibApi::class)
private fun Color.toKorgeColor(): RGBA {
    val hexColor = this.toArgb().toHexString().drop(2)
    return Colors[hexColor]
}

class SteelVectorsKorgeIosUIViewProvider : BaseKorgeIosUIViewProvider()

fun Int.pxToPoint(): Double = this.toDouble() / UIScreen.mainScreen.scale
