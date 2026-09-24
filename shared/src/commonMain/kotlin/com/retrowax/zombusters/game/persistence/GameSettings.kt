package com.retrowax.zombusters.game.persistence

import com.russhwolf.settings.Settings
import com.russhwolf.settings.set

// Persisted game settings backed by platform-native store.
// multiplatform-settings-no-arg 1.3.0 — Settings() uses platform default.
object GameSettings {
    private val settings = Settings()

    // Audio (0–10 integer slider values, matching OptionsScene)
    var fxVolume: Int
        get() = settings.getIntOrNull("fxVolume") ?: 7
        set(v) { settings["fxVolume"] = v }

    var musicVolume: Int
        get() = settings.getIntOrNull("musicVolume") ?: 6
        set(v) { settings["musicVolume"] = v }

    // Language code: "en" | "de" | "es" | "fr" | "it"
    var language: String
        get() = settings.getStringOrNull("language") ?: "en"
        set(v) { settings["language"] = v }

    // Fullscreen preference (desktop only)
    var fullscreen: Boolean
        get() = settings.getBooleanOrNull("fullscreen") ?: false
        set(v) { settings["fullscreen"] = v }

    // Campaign progress — highest level unlocked (1-based)
    var levelsUnlocked: Int
        get() = settings.getIntOrNull("levelsUnlocked") ?: 1
        set(v) { settings["levelsUnlocked"] = v.coerceAtLeast(1) }

    fun reset() {
        settings.clear()
    }
}
