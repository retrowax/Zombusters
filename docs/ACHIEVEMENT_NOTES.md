# Achievement Notes

## Original Legacy Achievements (`AchievementsManager.cs`, `Achievements.xml`)

### The 4 achievements
| Name | Trigger (reconstructed from AchievementsManager.cs) |
|---|---|
| **Dodger** | Complete a level without taking damage |
| **Unstoppable** | Reach a certain kill streak without dying |
| **QuickDead** | Kill a large number of enemies quickly |
| **EagleEye** | Achieve a high accuracy rate (bullets fired vs. hits) |

### Legacy storage
- Enum: `enum Achievements { Dodger, Unstoppable, QuickDead, EagleEye }`
- Stored as: `playerachievements.xml` per player in `IsolatedStorage`
- Loaded with `XmlSerializer`
- Delivery platform: **Xbox 360 / Xbox LIVE** — the entire delivery layer is wrapped in:
  `#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP && !WINDOWS_UAP`
  This means achievement delivery was **dead code** on most platforms even in the legacy release.

---

## KMP Port Design

### Separation of concerns
The achievement system is split into two layers:
1. **Game logic layer** — tracks in-game stats and decides when an achievement is earned (fully implemented, platform-agnostic)
2. **Delivery layer** — notifies the platform (Steam, Apple Game Center, Google Play, etc.) — intentionally NOT implemented yet

### What to implement now (offline tracking only)
```kotlin
interface AchievementService {
    fun unlock(achievement: Achievement)
    fun isUnlocked(achievement: Achievement): Boolean
}

enum class Achievement {
    DODGER,       // Complete a level without taking damage
    UNSTOPPABLE,  // 25+ kill streak without dying
    QUICK_DEAD,   // Kill 10+ enemies in 10 seconds
    EAGLE_EYE     // 80%+ bullet accuracy in a level
}
```

### `LocalAchievementService` (offline)
- Backed by `multiplatform-settings`
- Stores each achievement as a boolean key
- Shown in-game with a toast/overlay notification (non-blocking)
- No network calls

### Platform delivery (deferred)
- Steam: `SteamAchievementService` wrapping `SteamUserStats.SetAchievement()` — deferred to platform-services phase
- Google Play / Apple Game Center: `PlatformAchievementService` — deferred

### Stat tracking required
To implement these achievements the gameplay loop needs to track:
- `damageTakenThisLevel: Int` (for Dodger)
- `currentKillStreak: Int`, `maxKillStreak: Int` (for Unstoppable)
- `bulletsFireThisLevel: Int`, `bulletsHitThisLevel: Int` (for EagleEye)
- Kill timestamps ring buffer (for QuickDead)

These stats should be per-player in a multiplayer session.

### Current status
- Achievement enum and interface: **NOT YET IMPLEMENTED** in KMP port
- Stat tracking: **NOT YET IMPLEMENTED**
- `LocalAchievementService`: **NOT YET IMPLEMENTED**
- Platform delivery: **DEFERRED**
