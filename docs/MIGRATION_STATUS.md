# Zombusters – Migration Status

## Checklist

- [x] KMP baseline compiling
- [x] Project renamed (SteelVectors → Zombusters, package com.retrowax.zombusters)
  - Note: iosApp/SteelVectors.xcodeproj directory name still needs Xcode rename; bundle ID is correct
- [x] Virtual resolution 1280×720 centralized (KorgeWrapper.kt — no duplicate constants)
- [x] Legacy model types ported (enums, data classes — Enums.kt, LevelData.kt, GameConstants.kt)
- [x] Level XML parser implemented (LevelParser.kt — all 10 levels)
- [x] Level data tests passing (LevelParserTest.kt — Level 1 spawns, waves, furniture, walls; Level 10 spawns)
- [ ] Steering AI ported
- [ ] Steering AI unit tests
- [x] Player model (Avatar) — Avatar.kt with position, status, lives, hp, respawn/immune logic
- [ ] Enemy models (Zombie, Rat, Wolf, Minotaur, Tank)
- [x] Asset inventory corrected (zombusters_logo.png copied; aircraft assets removed)
- [x] Music playing (OGG via KorGE readMusic/playNoCancelForever — menu + gameplay tracks)
- [x] Level 1 environment debug-rendered (cpuGraphics — walls, obstacle circles, spawn zones)
- [x] Player 1 rendered at correct spawn position (955, 260 from Level 1 XML)
- [x] Player 1 movement working (WASD + arrow keys, delta-time, normalized diagonal)
- [x] Wall/furniture collision working (CollisionSystem — wall segment distance, obstacle radius)
- [x] Camera/viewport scaling correct (KorGE virtualSize=1280×720; verified at runtime)
- [ ] Enemies spawning
- [ ] Enemy AI pursuit
- [ ] Weapons/firing
- [ ] Damage system
- [ ] Player death/respawn
- [ ] Power-ups
- [ ] Score system
- [ ] HUD
- [ ] Pause menu
- [ ] Game over flow
- [ ] Level 1 completable
- [ ] All 10 levels loading
- [ ] Multiple player support
- [x] Logo/title screen (ZOMBUSTERS text title on MainMenuScene)
- [x] Main menu screen (MainMenuScene — START GAME / EXIT, keyboard+mouse)
- [ ] Select player screen
- [ ] Storytelling screen
- [ ] Options screen
- [ ] Credits / how-to-play
- [ ] Sound effects
- [ ] Settings persistence
- [ ] Localization (EN/DE/ES/FR/IT)
- [ ] Android build passing (not verified this session)
- [ ] iOS build passing (not verified this session)
- [ ] Steam integration (DEFERRED)
- [ ] Legacy parity review

## Milestones

### Milestone 1 — First Vertical Slice
**Status:** COMPLETE (JVM Desktop verified)
**Criteria:**
1. [x] Desktop app launches without crash
2. [x] No SteelVectors/aircraft game content visible
3. [x] Zombusters title displayed on main menu
4. [x] Game uses 1280×720 logical space
5. [x] START GAME transitions to Level 1
6. [x] Level 1 data loaded from XML (LevelParser)
7. [x] Player 1 at correct spawn (955, 260) from XML
8. [x] Player can move (WASD/arrows, delta-time, normalized diagonal)
9. [x] Wall/furniture collision works (player cannot pass through geometry)
10. [x] Rendering scales with window resize (KorGE virtual viewport)
11. [x] Core game code in commonMain
12. [x] Build passes (shared:compileKotlinJvm + composeApp:compileKotlinDesktop SUCCESSFUL)

### Milestone 2 — Level 1 Combat Loop
**Status:** Not started
**Criteria:** Enemy spawning, AI pursuit, combat, weapons, damage, death, power-ups, score, wave completion, game over

## Known Limitations
- `iosApp/SteelVectors.xcodeproj` directory name not yet renamed (requires Xcode)
- `GoogleService-Info.plist` still contains SteelVectors Firebase config — needs user-provided Zombusters config or removal
- XNB sound effects: MISSING (extraction required)
- XNB textures (character sprites): MISSING (extraction required) — debug placeholder in use
- AWT gesture listener warning on macOS JVM (KorGE 7.0.0-SNAPSHOT internal) — non-blocking
- Steam integration: DEFERRED
- Bloom post-processing: DEFERRED
- Perlin noise effect: DEFERRED
- Online leaderboard: DEFERRED
