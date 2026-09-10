package com.retrowax.zombusters

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.retrowax.zombusters.platform.KorgeView
import com.retrowax.zombusters.ui.theme.ZombustersTheme

@Composable
fun MainApp() {
    ZombustersTheme {
        KorgeView(
            modifier = Modifier.fillMaxSize()
                .background(MaterialTheme.colorScheme.background)
        ) { }
    }
}
