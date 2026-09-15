package com.retrowax.zombusters.platform

import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalConfiguration
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.io.async.launch
import korlibs.korge.android.KorgeAndroidView
import kotlinx.coroutines.Dispatchers

private const val DRAWABLE_RESOURCES_MAIN_PATH =
    "composeResources/com.retrowax.zombusters.resources/drawable"
private const val FONT_RESOURCES_MAIN_PATH = "composeResources/com.retrowax.zombusters.resources/font"
private const val FILES_RESOURCES_MAIN_PATH = "composeResources/com.retrowax.zombusters.resources/files"

@Composable
actual fun KorgeView(modifier: Modifier, exit: () -> Unit) {
    val configuration = LocalConfiguration.current
    val windowHeight = configuration.screenHeightDp.dp
    val windowWidth = configuration.screenWidthDp.dp
    val buttonBackgroundColor = MaterialTheme.colorScheme.secondary.toKorgeColor()
    val korge = KorgeWrapper(
        windowWidth = windowWidth,
        windowHeight = windowHeight,
        buttonBackgroundColor = buttonBackgroundColor,
        exit = exit,
        drawableResourcesPath = DRAWABLE_RESOURCES_MAIN_PATH,
        fontResourcesPath = FONT_RESOURCES_MAIN_PATH,
        filesResourcesPath = FILES_RESOURCES_MAIN_PATH
    )
    AndroidView(factory = { context ->
        val v = KorgeAndroidView(context)
        launch(Dispatchers.IO) { v.loadModule(korge.getKorgeConfig()) }
        v
    }, modifier)
}

@OptIn(ExperimentalStdlibApi::class)
private fun Color.toKorgeColor(): RGBA {
    val hexColor = this.toArgb().toHexString().drop(2)
    return Colors[hexColor]
}
