# Zombusters – Migration Status

## Checklist

- [x] KMP baseline compiling
- [x] Project renamed (SteelVectors → Zombusters, package com.retrowax.zombusters)
  - Note: iosApp/SteelVectors.xcodeproj directory name still needs Xcode rename; bundle ID is correct
- [x] Virtual resolution 1280×720 centralized (KorgeWrapper.kt — no duplicate constants)
- [x] Legacy model types ported (enums, data classes — Enums.kt, LevelData.kt, GameConstants.kt)
- [x] Level XML parser implemented (LevelParser.kt — all 10 levels)
- [x] Level data tests passing (LevelParserTest.kt — Level 1 spawns, waves, furniture, walls; Level 10 spawns)
- [x] Steering AI ported (Vec2, SteeringEntity, ArriveSteer, PursuitSteer, ObstacleSteer, SteeringSystem — pure commonMain)
- [x] Steering AI unit tests (10 tests: Vec2 math, Arrive, Pursuit, separation)
- [x] Player model (Avatar) — Avatar.kt with position, status, lives, hp, respawn/immune logic
- [x] Enemy models (Zombie, Rat, Wolf — BaseEnemy + concrete types, pure domain, no KorGE)
- [x] Enemy factory (EnemyFactory — creates positioned enemies from spawn zones with legacy defaults)
- [x] Wave system (WaveSystem — SubLevel progression, advance/reset, 6 tests)
- [x] Spawn system (SpawnSystem — spawns full waves by EnemiesCount, random zone selection)
- [x] Enemy system (EnemySystem — per-frame update loop, contact detection)
- [x] Enemy rendering (ZombieView/RatView/WolfView via container+sprite in GameplayScene; depth-sorted by zIndex)
- [x] Enemy/player contact detection (EnemySystem.enemiesInContactRange, contact radius 30px)
- [x] Debug AI overlay extension (F1 shows walls + obstacles + spawn zones; K kills active wave)
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

### Milestone 1.5 — Enemy Simulation Foundation
**Status:** COMPLETE (JVM build verified, 34 tests passing)
**Criteria:**
1. [x] Steering math ported: Vec2, SteeringEntity, ArriveSteer, PursuitSteer, ObstacleSteer, SteeringSystem
2. [x] Domain enemies: BaseEnemy (abstract), Zombie, Rat, Wolf — no KorGE inheritance
3. [x] EnemyFactory creates enemies with legacy constants from spawn zones
4. [x] SpawnSystem spawns full wave from EnemiesCount + spawn zones
5. [x] WaveSystem tracks sublevel progression (advance/reset/isComplete)
6. [x] EnemySystem: per-frame update loop + contact detection
7. [x] Enemy rendering in GameplayScene with original sprites + depth sorting
8. [x] F1 debug overlay: walls + obstacles + spawn zones
9. [x] K key: kill active wave for lifecycle testing
10. [x] 34 tests passing (10 steering math + 6 wave system + 18 existing)

### Milestone 2 — Level 1 Combat Loop
**Status:** Enemy foundation complete; combat deferred
**Criteria:** Enemy spawning, AI pursuit, combat, weapons, damage, death, power-ups, score, wave completion, game over
**Completed so far (Step 4):**
- Enemies spawn and pursue player with legacy Pursue+ObstacleAvoidance steering
- Wave/sublevel progression (WaveSystem)
- Enemy rendering with original extracted sprites (walk/run/idle animations)
- Contact detection — contact range 30px flagged; no damage system yet
- Debug: F1 shows AI overlay, K key kills active wave for lifecycle testing
**Still needed:** Weapons, bullets, damage, player death/respawn, score, power-ups, game-over flow

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
