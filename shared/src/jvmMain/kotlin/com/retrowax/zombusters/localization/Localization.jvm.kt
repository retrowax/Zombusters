package com.retrowax.zombusters.localization

import androidx.compose.runtime.Composable
import com.retrowax.zombusters.game.persistence.GameSettings

actual fun getCurrentLanguage(): AvailableLanguages {
    return try {
        AvailableLanguages.valueOf(GameSettings.language.uppercase())
    } catch (_: Exception) {
        AvailableLanguages.EN
    }
}

@Composable
actual fun SetLanguage(language: AvailableLanguages) {
}
