package com.retrowax.zombusters.game.achievements

interface AchievementService {
    fun unlock(achievement: Achievement)
    fun isUnlocked(achievement: Achievement): Boolean
    fun reset()
}
