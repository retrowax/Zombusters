package com.retrowax.zombusters.android

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.retrowax.zombusters.MainApp
import com.retrowax.zombusters.ui.theme.ZombustersTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            ZombustersTheme {
                MainApp()
            }
        }
    }
}
