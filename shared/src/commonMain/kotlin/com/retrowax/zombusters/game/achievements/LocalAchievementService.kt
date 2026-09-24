package com.retrowax.zombusters.game.achievements

import com.russhwolf.settings.Settings
import com.russhwolf.settings.set

object LocalAchievementService : AchievementService {
    private val settings = Settings()

    override fun unlock(achievement: Achievement) {
        settings[achievement.key] = true
    }

    override fun isUnlocked(achievement: Achievement): Boolean =
        settings.getBooleanOrNull(achievement.key) ?: false

    override fun reset() {
        Achievement.entries.forEach { settings.remove(it.key) }
    }
}
