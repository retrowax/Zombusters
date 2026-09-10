package com.steelvectors.app.android

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import com.steelvectors.app.MainApp
import com.steelvectors.app.ui.theme.SkyVectorTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()

        setContent {
            SkyVectorTheme {
                MainApp()
            }
        }
    }
}
