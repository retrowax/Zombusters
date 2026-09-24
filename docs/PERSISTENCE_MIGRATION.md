# Persistence Migration Notes

## Original Legacy Behavior (legacy/ZombustersWindows)

### Storage Layer (`StorageDataSource.cs`)
- Platform: `IsolatedStorage` (Windows Phone / Xbox 360 / Windows)
- Serialization: `XmlSerializer` for options + save data; binary `BinaryWriter/BinaryReader` for leaderboard
- Files:
  - `options.xml` — game settings
  - `savegame.sav` — campaign progress (XML despite extension)
  - `leaderboard.txt` — binary-encoded high score list (despite .txt extension)

### `OptionsState` (`Player.cs` `LoadOptions`/`SaveOptions`)
| Field | Default | Range |
|---|---|---|
| FXLevel | 0.7f | 0.0–1.0 float (continuous) |
| MusicLevel | 0.6f | 0.0–1.0 float (continuous) |
| Player | InputMode.Keyboard | enum |
| FullScreenMode | false | bool |

**Note:** Legacy used continuous float sliders; the KMP OptionsScene uses discrete 0–10 integer steps
that map to approximately the same range (7 ≈ 0.7f, 6 ≈ 0.6f).

### `SaveGameData` (`Player.cs` `SaveGame`)
- `PlayerName` — string
- `Level` (int) — current campaign level; only saved when `levelsUnlocked < currentLevelNumber`

### Auto-save triggers
- `Player.Destroy()` — saves leaderboard on last life when `score > 250`
- `Player.SaveGame()` — called at level transitions

### High Scores (`TopScoreEntry.cs`, `TopScoreList.cs`)
- `TopScoreListContainer` with 1 list of up to 10 entries
- `TopScoreEntry` = name (string) + score (int)
- Binary serialization with `BinaryWriter`

---

## KMP Port Implementation

### Library
`com.russhwolf:multiplatform-settings-no-arg:1.3.0` — uses platform-native store:
- Android: `SharedPreferences`
- iOS: `NSUserDefaults`
- Desktop JVM: `java.util.prefs.Preferences`

### `GameSettings` (`persistence/GameSettings.kt`)
```kotlin
object GameSettings {
    var fxVolume: Int        // 0–10 (default 7, maps to legacy 0.7f)
    var musicVolume: Int     // 0–10 (default 6, maps to legacy 0.6f)
    var language: String     // "en"|"de"|"es"|"fr"|"it" (default "en")
    var fullscreen: Boolean  // (default false)
    var levelsUnlocked: Int  // 1-based, (default 1)
    fun reset()
}
```

### `LocalHighScores` (`persistence/LocalHighScores.kt`)
```kotlin
object LocalHighScores {
    fun load(): List<HighScoreEntry>
    fun submit(name: String, score: Int): List<HighScoreEntry>
    fun isHighScore(score: Int): Boolean
    fun clear()
}
data class HighScoreEntry(val name: String, val score: Int)
```
- Serialization: pipe-delimited string (`name|score|name|score|…`) in a single key
- Max 10 entries, sorted descending by score
- Score names truncated to 10 characters

### Auto-save
- `OptionsScene`: saves `fxVolume`, `musicVolume` on "Save and Exit"
- `GameplayScene`: saves `levelsUnlocked` when a new level is cleared
  (`GameSettings.levelsUnlocked = updatedSession.currentLevel.coerceAtMost(MAX_CAMPAIGN_LEVELS)`)
- High scores: submitted on game-over (when `score > 0`)

### What's NOT persisted (deferred)
- Player name (no name entry screen yet)
- Per-character save slots (legacy had one shared slot)
- `fullscreen` persistence wired to actual window state (desktop only, deferred)
- `language` write: OptionsScene shows language option but selector not yet connected to `GameSettings.language`

---

## Test Coverage
- `LocalHighScoresTest.kt` — submit/load/isHighScore/clear, max entries, sort order
