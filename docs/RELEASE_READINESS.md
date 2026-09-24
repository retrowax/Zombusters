# Release Readiness

## Status as of Step 8

### MULTIPLAYER

| Feature | Status | Notes |
|---|---|---|
| Domain model (4 players) | ✅ | `GameplayWorld` supports 1–4 `Avatar` + `CombatState` instances |
| Spawn positions from LevelsDef.xml | ✅ | `LevelDef.spawnPositionFor(idx)` |
| Per-player damage/lives/score | ✅ | `Avatar` is fully independent |
| Per-player respawn/immune state | ✅ | Legacy 2s respawn / 8s immune times |
| Multi-player bullet collisions | ✅ | `DamageSystem.processBulletCollisionsMulti` |
| Multi-player enemy contact | ✅ | `DamageSystem.processEnemyContactMulti` |
| Enemy targeting nearest player | ✅ | `EnemySystem` uses steering entities for all active players |
| Input slots / per-player `GameInput` | ✅ | Legacy `accumMove`/`accumFire` bug fixed |
| SelectPlayerScene join flow (2–4 players) | ❌ | Single-player only |
| GameplayScene multi-player loop | ❌ | Still uses `world.player1` throughout |
| Multiplayer HUD (per-player) | ❌ | Single-player HUD only |
| Mobile multi-player (touch) | N/A | One touch = one player per mobile policy |

### PERSISTENCE

| Feature | Status | Notes |
|---|---|---|
| FX volume save/load | ✅ | `GameSettings.fxVolume` |
| Music volume save/load | ✅ | `GameSettings.musicVolume` |
| Language setting save | ✅ | `GameSettings.language` |
| Fullscreen preference save | ✅ | `GameSettings.fullscreen` |
| Campaign progress (levels unlocked) | ✅ | `GameSettings.levelsUnlocked` |
| Local high scores (10 entries) | ✅ | `LocalHighScores` |
| OptionsScene wired to persistence | ✅ | Saves on "Save and Exit" |
| High score submit on game over | ❌ | GameOverScene not yet implemented |
| Language selector in OptionsScene | ❌ | Shows option label, no cycle UI |
| Fullscreen wired to window state | ❌ | Desktop deferred |

### LOCALIZATION

| Feature | Status | Notes |
|---|---|---|
| Interface with 35 strings | ✅ | Covers all major screens |
| EN / DE / ES / FR / IT translations | ✅ | All 5 legacy languages |
| RU / ZH translations | ✅ | Bonus languages not in legacy |
| StartScene uses localization | ✅ | |
| MenuScene uses localization | ✅ | |
| ExtrasMenuScene uses localization | ✅ | |
| OptionsScene uses localization | ❌ | Hardcoded EN |
| SelectPlayerScene uses localization | ❌ | Hardcoded EN |
| HowToPlayScene uses localization | ❌ | Hardcoded EN |
| GameplayScene HUD uses localization | ❌ | Hardcoded EN |
| Language persists across sessions | ✅ | Via `GameSettings.language` |
| Language selector interactive | ❌ | |

### ACHIEVEMENTS

| Feature | Status | Notes |
|---|---|---|
| `Achievement` enum | ❌ | Not implemented |
| `AchievementService` interface | ❌ | Not implemented |
| `LocalAchievementService` | ❌ | Not implemented |
| In-game stat tracking | ❌ | Not implemented |
| Steam delivery | ❌ | Deferred to platform-services phase |

### PLATFORMS

| Platform | Status | Notes |
|---|---|---|
| Desktop JVM build | ✅ | `./gradlew :composeApp:run` |
| Android APK (debug) | ✅ | `./gradlew :composeApp:assembleDebug` |
| iOS build | ⚠️ | Xcode project updated, on-device not verified |
| Android: no Firebase/geo API key | ✅ | Removed from AndroidManifest.xml |
| Desktop: window title "Zombusters" | ✅ | |
| iOS: display name "Zombusters" | ✅ | project.pbxproj updated |
| Debug keys gated | ✅ | `GameDebugConfig.ENABLED` |
| Landscape orientation (Android) | ✅ | `sensorLandscape` |
| Landscape orientation (iOS) | ✅ | LandscapeLeft + LandscapeRight |

### QUALITY

| Feature | Status | Notes |
|---|---|---|
| Tests passing | ✅ | 143 tests (JVM) |
| Single-player gameplay loop | ✅ | Full campaign 1–10 levels |
| Touch controls (all menus) | ✅ | All 10 scenes |
| Mouse controls (all menus) | ✅ | All 10 scenes |
| Touch gameplay (virtual sticks) | ✅ | Dual thumbstick |
| Responsive UI (1280×720 virtual) | ✅ | All scenes |
| Two-player desktop runtime test | ❌ | Multiplayer loop not complete |

---

## Known Limitations

- **Level maps 2–10:** Only level01/map.png extracted. Levels 2–10 show placeholder.
- **Thumbstick/pause sprites:** Circle placeholders (XNB extraction pending)
- **Character portraits:** Only Tracy (Jade) sprites extracted; Charles/Ryan/Peter show "NOT AVAILABLE"
- **Sound effects:** XNB extraction pending — no SFX in gameplay
- **Music:** Loads from `resources/zombusters/music/` — packaging dependent on build configuration
- **Campaign end screen:** Falls back to MenuScene after level 10 (no victory screen)
- **Minotaur enemy:** Steering logic present but animations/sprites not fully wired
- **macOS AWT gesture warning:** KorGE 7.0.0-SNAPSHOT internal, non-blocking
- **Achievements:** Not implemented
- **Steam integration:** Deferred

## Parity Table (Legacy vs KMP)

| System | Legacy | KMP | Gap |
|---|---|---|---|
| Single-player gameplay | ✅ Full | ✅ Level 1 full, levels 2–10 placeholder map | Level maps |
| All enemy types | ✅ Zombie/Rat/Wolf/Minotaur | ✅ Zombie/Rat/Wolf, Minotaur partial | Minotaur animations |
| All weapons | ✅ Pistol/MG/Shotgun/Flame/Grenade | ✅ | Parity |
| Power-ups | ✅ Speed/Immune/Ammo | ✅ | Parity |
| Campaign (10 levels) | ✅ | ✅ progression, ⚠️ map assets | Map assets |
| Multiplayer (2–4 players) | ✅ Xbox 360 local | ❌ Not wired in scene | Domain model ready |
| Settings persistence | ✅ XML/IsolatedStorage | ✅ multiplatform-settings | Parity |
| Campaign save | ✅ | ✅ `levelsUnlocked` | Parity |
| High scores | ✅ 10 entries | ✅ 10 entries | Submit UI missing |
| Localization (EN/DE/ES/FR/IT) | ✅ | ✅ (some scenes hardcoded) | Wiring |
| Achievements | ✅ (Xbox LIVE, often dead code) | ❌ | Deferred |
| Touch controls | ✅ Windows Phone | ✅ | Parity |
| Controller support | ✅ Xbox 360 gamepads | ⚠️ Not wired | Deferred |
| Steam | N/A | ❌ | Deferred |
