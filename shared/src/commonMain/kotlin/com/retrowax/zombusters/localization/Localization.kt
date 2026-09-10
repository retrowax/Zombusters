package com.retrowax.zombusters.localization

import androidx.compose.runtime.Composable
import com.retrowax.zombusters.localization.translations.ChineseLocalization
import com.retrowax.zombusters.localization.translations.DeutschLocalization
import com.retrowax.zombusters.localization.translations.EnglishLocalization
import com.retrowax.zombusters.localization.translations.FrenchLocalization
import com.retrowax.zombusters.localization.translations.ItalianLocalization
import com.retrowax.zombusters.localization.translations.RussianLocalization
import com.retrowax.zombusters.localization.translations.SpanishLocalization

interface Localization {
    val appName: String
    val backLabel: String
    val loading: String
    val gameLevel: String
    val gameGameOver: String
    val gamePause: String
    val gameUnPause: String
    val gameStartGame: String
    val gameExit: String
    val gameRestart: String
    val gameFinalScore: String
}

enum class AvailableLanguages {
    DE,
    EN,
    ES,
    IT,
    FR,
    RU,
    CN;

    companion object {
        val languages = listOf(EN, ES, IT, FR, DE, RU, CN)
    }
}

expect fun getCurrentLanguage(): AvailableLanguages

@Composable
expect fun SetLanguage(language: AvailableLanguages)

fun getCurrentLocalization() = when (getCurrentLanguage()) {
    AvailableLanguages.EN -> EnglishLocalization
    AvailableLanguages.ES -> SpanishLocalization
    AvailableLanguages.IT -> ItalianLocalization
    AvailableLanguages.FR -> FrenchLocalization
    AvailableLanguages.DE -> DeutschLocalization
    AvailableLanguages.RU -> RussianLocalization
    AvailableLanguages.CN -> ChineseLocalization
}
