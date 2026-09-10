package com.steelvectors.app

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.steelvectors.app.platform.KorgeView
import com.steelvectors.app.ui.theme.SkyVectorTheme

@Composable
fun MainApp() {
    SkyVectorTheme {
        KorgeView(
            modifier = Modifier.fillMaxSize()
                .background(MaterialTheme.colorScheme.background)
        ) { }
    }
}
