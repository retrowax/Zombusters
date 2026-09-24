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

    // Main menu
    val menuNewGame: String
    val menuExtras: String
    val menuOptions: String
    val menuQuit: String

    // Select Player screen
    val selectCharacter: String
    val selectStartLevel: String
    val selectCharUnavailable: String
    val selectConfirm: String

    // Options screen
    val optionsSoundFxVolume: String
    val optionsMusicVolume: String
    val optionsLanguage: String
    val optionsFullscreen: String
    val optionsSaveAndExit: String

    // Extras menu
    val extrasHowToPlay: String
    val extrasLeaderboard: String
    val extrasCredits: String

    // Pause menu
    val pauseResume: String
    val pauseHowToPlay: String
    val pauseOptions: String
    val pauseRestartLevel: String
    val pauseQuitToMainMenu: String

    // Game over
    val gameOverRestartWave: String
    val gameOverRestartBeginning: String
    val gameOverReturnToMenu: String

    // HUD labels
    val hudHp: String
    val hudLives: String
    val hudScore: String
    val hudWave: String
    val hudCleared: String
    val hudEnemies: String

    // Input hint text
    val pressAnyKey: String
    val tapToContinue: String
    val stageCleared: String
    val back: String
    val escBack: String
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
