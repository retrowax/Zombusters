package com.retrowax.zombusters.game.achievements

private const val UNSTOPPABLE_KILL_STREAK = 25
private const val QUICK_DEAD_KILLS = 10
private const val QUICK_DEAD_WINDOW_SEC = 10f
private const val EAGLE_EYE_MIN_ACCURACY = 0.80f
private const val EAGLE_EYE_MIN_SHOTS = 20  // require at least 20 shots to qualify

class AchievementTracker(
    private val service: AchievementService = LocalAchievementService
) {
    private var damageTaken = 0
    private var killStreak = 0

    // Ring buffer of kill timestamps for QuickDead
    private val recentKillTimes = ArrayDeque<Float>(QUICK_DEAD_KILLS + 1)

    var bulletsShot = 0
        private set
    var bulletsHit = 0
        private set

    fun onDamageTaken() {
        damageTaken++
        killStreak = 0
    }

    fun onPlayerDied() {
        killStreak = 0
    }

    fun onEnemyKilled(totalSec: Float) {
        killStreak++
        if (killStreak >= UNSTOPPABLE_KILL_STREAK && !service.isUnlocked(Achievement.UNSTOPPABLE)) {
            service.unlock(Achievement.UNSTOPPABLE)
        }

        recentKillTimes.addLast(totalSec)
        while (recentKillTimes.isNotEmpty() && totalSec - recentKillTimes.first() > QUICK_DEAD_WINDOW_SEC) {
            recentKillTimes.removeFirst()
        }
        if (recentKillTimes.size >= QUICK_DEAD_KILLS && !service.isUnlocked(Achievement.QUICK_DEAD)) {
            service.unlock(Achievement.QUICK_DEAD)
        }
    }

    fun onBulletFired() { bulletsShot++ }
    fun onBulletHit() { bulletsHit++ }

    fun onLevelComplete() {
        if (damageTaken == 0 && !service.isUnlocked(Achievement.DODGER)) {
            service.unlock(Achievement.DODGER)
        }
        if (bulletsShot >= EAGLE_EYE_MIN_SHOTS) {
            val accuracy = bulletsHit.toFloat() / bulletsShot
            if (accuracy >= EAGLE_EYE_MIN_ACCURACY && !service.isUnlocked(Achievement.EAGLE_EYE)) {
                service.unlock(Achievement.EAGLE_EYE)
            }
        }
    }

    fun resetForLevel() {
        damageTaken = 0
        killStreak = 0
        recentKillTimes.clear()
        bulletsShot = 0
        bulletsHit = 0
    }
}
