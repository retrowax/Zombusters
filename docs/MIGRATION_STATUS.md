# Zombusters – Migration Status

Last updated: Step 8 (2026-09-24)

## Build Status
- `./gradlew :shared:compileKotlinJvm :shared:jvmTest` → **BUILD SUCCESSFUL, 169 tests passing**
- `./gradlew :composeApp:assembleDebug` → **BUILD SUCCESSFUL** (Android APK)
- JVM desktop: `./gradlew :composeApp:run` → launches and runs
- iOS: Xcode project updated, on-device not verified this session

---

## Checklist

### Platform & Infrastructure
- [x] KMP baseline compiling
- [x] Project renamed (SteelVectors → Zombusters, package com.retrowax.zombusters)
- [x] Virtual resolution 1280×720 centralized
- [x] Android: no Firebase/geo API key remnants in AndroidManifest.xml
- [x] Android: landscape orientation (sensorLandscape)
- [x] Android APK debug build verified
- [x] iOS: display name "Zombusters" in project.pbxproj
- [x] iOS: landscape-only (LandscapeLeft + LandscapeRight)
- [x] Desktop: window title "Zombusters"
- [x] Debug keys (F1/F2/K/G/H) gated behind GameDebugConfig.ENABLED
- [ ] iOS on-device verified

### Game Core
- [x] Legacy model types ported (enums, data classes — Enums.kt, LevelData.kt, GameConstants.kt)
- [x] Level XML parser (LevelParser.kt — all 10 levels, P1–P4 spawn positions)
- [x] Player model (Avatar.kt — position, status, lives, hp, respawn/immune 2s/8s state machine)
- [x] Enemy models (Zombie, Rat, Wolf, Minotaur — BaseEnemy + concrete types)
- [x] Steering AI (Vec2, SteeringEntity, ArriveSteer, PursuitSteer, ObstacleSteer)
- [x] Wave system (WaveSystem — 10 sub-levels per level, advance/reset)
- [x] Spawn system (SpawnSystem — zones, EnemiesCount)
- [x] Enemy system (EnemySystem — per-frame update, multi-player targeting nearest living player)
- [x] Collision system (CollisionSystem — wall segment distance, obstacle radius)
- [x] Combat system (CombatSystem — tryFire per gun type, prune out-of-bounds)
- [x] Damage system (DamageSystem — bullet collisions, enemy contact, multi-player variants)
- [x] Power-up system (PowerUpSystem — speed/immune/ammo/life spawns, multi-player first-in-range)
- [x] Score system (Avatar.score, extra life at 8000 milestones)
- [x] Multi-player domain (GameplayWorld: 1–4 Avatar + CombatState + SteeringEntity)
- [x] Projectile ownership (Projectile.shooterIndex / ShotgunShell.shooterIndex)
- [x] Multi-player bullet collision attribution (processBulletCollisionsMulti)
- [x] Multi-player enemy contact (processEnemyContactMulti)

### Scenes / UI
- [x] LogoScene (splash with auto-advance)
- [x] StartScene (press-any-key, localized, touch/mouse)
- [x] MenuScene (NEW GAME / EXTRAS / OPTIONS / QUIT, localized, touch/mouse)
- [x] ExtrasMenuScene (HOW TO PLAY / LEADERBOARD / CREDITS, localized, touch/mouse)
- [x] HowToPlayScene (controls list, touch/mouse to go back)
- [x] CreditsScene (scrolling credits, skip-to-end on tap/click, touch/mouse)
- [x] OptionsScene (FX vol, music vol, language cycle, fullscreen toggle, save wired to persistence, touch/mouse)
- [x] SelectPlayerScene (character select, level select, touch/mouse)
- [x] GameplayScene (full gameplay loop — single-player via world.player1 alias, touch dual-sticks, pause menu)
- [ ] GameplayScene multi-player loop (2–4 players wired; domain ready but scene not yet updated)
- [ ] Multiplayer HUD (per-player HP/lives/score)
- [ ] Game over scene (with high-score submit)
- [ ] Victory/campaign-complete screen

### Persistence
- [x] GameSettings (fxVolume, musicVolume, language, fullscreen, levelsUnlocked) via multiplatform-settings
- [x] LocalHighScores (10 entries, sorted by score, pipe-delimited)
- [x] OptionsScene saves on "Save and Exit" (all 5 settings)
- [x] GameplayScene saves levelsUnlocked on stage-clear
- [x] SelectPlayerScene reads levelsUnlocked from GameSettings
- [ ] High score submit flow (requires game-over screen)
- [ ] Player name entry

### Localization
- [x] Localization interface with 35 strings covering all major screens
- [x] EN / DE / ES / FR / IT translations (5 legacy languages)
- [x] RU / ZH translations (bonus)
- [x] StartScene, MenuScene, ExtrasMenuScene use getCurrentLocalization()
- [x] Language setting persisted in GameSettings.language
- [x] Language cycle UI in OptionsScene
- [ ] OptionsScene / SelectPlayerScene / HowToPlayScene / GameplayScene HUD wired to localization

### Achievements
- [x] Achievement enum (Dodger, Unstoppable, QuickDead, EagleEye)
- [x] AchievementService interface
- [x] LocalAchievementService (multiplatform-settings backed)
- [x] AchievementTracker (in-game stat tracking, per-level reset)
- [ ] AchievementTracker wired into GameplayScene
- [ ] Achievement notification UI
- [ ] Steam delivery (DEFERRED)

### Audio
- [x] Music playing (OGG via KorGE readMusic/playNoCancelForever)
- [ ] Sound effects (XNB extraction required)

---

## Test Suites (169 tests)

| Suite | Tests |
|---|---|
| AchievementTrackerTest | 9 |
| FacingDirectionTest | 12 |
| ProjectilePositionTest | ~6 |
| ShotgunShellTest | ~5 |
| SteeringMathTest | 10 |
| CollisionSystemTest | ~8 |
| VirtualThumbstickTest | 18 |
| AvatarCombatTest | ~12 |
| GameSessionTest | ~10 |
| PowerUpTest | ~8 |
| LocalHighScoresTest | 10 |
| CombatSystemTest | ~8 |
| DamageSystemTest | ~12 |
| MultiplayerDomainTest | 15 |
| WaveSystemTest | 6 |
| LevelParserTest | ~10 |

---

## Known Limitations

- Level maps 2–10: only level01/map.png extracted; others show placeholder
- Character portraits: only Tracy (Jade) sprites extracted; Charles/Ryan/Peter show "NOT AVAILABLE"  
- Thumbstick/pause sprites: circle placeholders (XNB extraction pending)
- Sound effects: XNB extraction pending
- Campaign-end screen: falls back to MenuScene after level 10
- Minotaur: steering logic present, animations not fully wired
- AWT gesture listener warning on macOS (KorGE 7.0.0-SNAPSHOT internal) — non-blocking
- `iosApp/SteelVectors.xcodeproj` directory name (requires Xcode rename)
- `GoogleService-Info.plist` still contains SteelVectors Firebase config
- Steam integration: DEFERRED
- Online leaderboard: DEFERRED
- Remote/network multiplayer: DEFERRED
