package com.steelvectors.app.platform

import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.awt.SwingPanel
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.platform.LocalWindowInfo
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.korge.glCanvas

private const val DRAWABLE_RESOURCES_MAIN_PATH =
    "composeResources/com.steelvectors.app.resources/drawable"
private const val FONT_RESOURCES_MAIN_PATH =
    "composeResources/com.steelvectors.app.resources/font"

@Composable
actual fun KorgeView(modifier: Modifier, exit: () -> Unit) {
    val windowInfo = LocalWindowInfo.current
    val density = LocalDensity.current
    val buttonBackgroundColor = MaterialTheme.colorScheme.secondary.toKorgeColor()

    val screenWidthDp = with(density) { windowInfo.containerSize.width.toDp() }
    val screenHeightDp = with(density) { windowInfo.containerSize.height.toDp() }

    val canvas = remember {
        val korge = KorgeWrapper(
            screenWidth = screenWidthDp,
            screenHeight = screenHeightDp,
            buttonBackgroundColor = buttonBackgroundColor,
            exit = exit,
            drawableResourcesPath = DRAWABLE_RESOURCES_MAIN_PATH,
            fontResourcesPath = FONT_RESOURCES_MAIN_PATH
        )
        korge.getKorgeConfig().glCanvas {}
    }

    SwingPanel(
        modifier = modifier,
        factory = { canvas }
    )
}

@OptIn(ExperimentalStdlibApi::class)
private fun Color.toKorgeColor(): RGBA {
    val hexColor = this.toArgb().toHexString().drop(2)
    return Colors[hexColor]
}
