package com.retrowax.zombusters.game.achievements

import kotlin.test.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class AchievementTrackerTest {

    private fun makeService(): AchievementService = object : AchievementService {
        private val unlocked = mutableSetOf<Achievement>()
        override fun unlock(a: Achievement) { unlocked.add(a) }
        override fun isUnlocked(a: Achievement) = a in unlocked
        override fun reset() = unlocked.clear()
    }

    @Test
    fun dodger_unlockedWhenNoLevelDamageTaken() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        tracker.onLevelComplete()
        assertTrue(svc.isUnlocked(Achievement.DODGER))
    }

    @Test
    fun dodger_notUnlockedWhenDamageTaken() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        tracker.onDamageTaken()
        tracker.onLevelComplete()
        assertFalse(svc.isUnlocked(Achievement.DODGER))
    }

    @Test
    fun unstoppable_unlockedAt25KillStreak() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        repeat(25) { tracker.onEnemyKilled(it.toFloat()) }
        assertTrue(svc.isUnlocked(Achievement.UNSTOPPABLE))
    }

    @Test
    fun unstoppable_resetByDamage() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        repeat(24) { tracker.onEnemyKilled(it.toFloat()) }
        tracker.onDamageTaken()
        repeat(24) { tracker.onEnemyKilled(it.toFloat() + 30f) }
        assertFalse(svc.isUnlocked(Achievement.UNSTOPPABLE))
    }

    @Test
    fun quickDead_unlockedWith10KillsIn10Sec() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        repeat(10) { tracker.onEnemyKilled(it * 0.5f) }
        assertTrue(svc.isUnlocked(Achievement.QUICK_DEAD))
    }

    @Test
    fun quickDead_notUnlockedWhen10KillsSpreadOver20Sec() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        repeat(10) { tracker.onEnemyKilled(it * 2.5f) }
        assertFalse(svc.isUnlocked(Achievement.QUICK_DEAD))
    }

    @Test
    fun eagleEye_unlockedWith80PctAccuracy() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        repeat(20) { tracker.onBulletFired() }
        repeat(16) { tracker.onBulletHit() }
        tracker.onLevelComplete()
        assertTrue(svc.isUnlocked(Achievement.EAGLE_EYE))
    }

    @Test
    fun eagleEye_notUnlockedBelow80PctAccuracy() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        repeat(20) { tracker.onBulletFired() }
        repeat(15) { tracker.onBulletHit() }
        tracker.onLevelComplete()
        assertFalse(svc.isUnlocked(Achievement.EAGLE_EYE))
    }

    @Test
    fun eagleEye_requiresMinimumShots() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        repeat(5) { tracker.onBulletFired() }
        repeat(5) { tracker.onBulletHit() }
        tracker.onLevelComplete()
        assertFalse(svc.isUnlocked(Achievement.EAGLE_EYE))
    }

    @Test
    fun resetForLevel_clearsAllStats() {
        val svc = makeService()
        val tracker = AchievementTracker(svc)
        tracker.onDamageTaken()
        repeat(5) { tracker.onEnemyKilled(it.toFloat()) }
        repeat(10) { tracker.onBulletFired() }
        tracker.resetForLevel()
        tracker.onLevelComplete()
        assertTrue(svc.isUnlocked(Achievement.DODGER))
    }
}
