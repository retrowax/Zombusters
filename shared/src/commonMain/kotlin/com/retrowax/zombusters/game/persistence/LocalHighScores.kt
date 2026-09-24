package com.retrowax.zombusters.game.persistence

import com.russhwolf.settings.Settings
import com.russhwolf.settings.set

data class HighScoreEntry(val name: String, val score: Int)

object LocalHighScores {
    private const val KEY = "highScores"
    private const val MAX_ENTRIES = 10
    private val settings = Settings()

    fun load(): List<HighScoreEntry> {
        val raw = settings.getStringOrNull(KEY) ?: return emptyList()
        val parts = raw.split("|")
        val result = mutableListOf<HighScoreEntry>()
        var i = 0
        while (i + 1 < parts.size) {
            val name = parts[i]
            val score = parts[i + 1].toIntOrNull() ?: 0
            if (name.isNotEmpty()) result.add(HighScoreEntry(name, score))
            i += 2
        }
        return result
    }

    fun submit(name: String, score: Int): List<HighScoreEntry> {
        val trimmedName = name.trim().take(10).ifEmpty { "???" }
        val updated = (load() + HighScoreEntry(trimmedName, score))
            .sortedByDescending { it.score }
            .take(MAX_ENTRIES)
        save(updated)
        return updated
    }

    fun isHighScore(score: Int): Boolean {
        val current = load()
        return current.size < MAX_ENTRIES || score > (current.lastOrNull()?.score ?: 0)
    }

    private fun save(entries: List<HighScoreEntry>) {
        val raw = entries.joinToString("|") { "${it.name}|${it.score}" }
        settings[KEY] = raw
    }

    fun clear() {
        settings.remove(KEY)
    }
}
