# Zombusters – KMP Migration Plan

## Source
Legacy MonoGame/C# implementation under `legacy/` (read-only reference).

## Target
Kotlin Multiplatform + KorGE 7.0.0-SNAPSHOT  
Package: `com.retrowax.zombusters`  
Initial targets: JVM Desktop, Android, iOS

## Virtual Resolution
Fixed logical canvas: **1280 × 720** (matches legacy `MyGame.cs`)  
Scale with letterbox to actual display.

---

## Classification Legend

| Code | Meaning |
|------|---------|
| A | Pure gameplay/domain — port to commonMain |
| B | Engine-dependent behavior — reimplement in KorGE |
| C | Platform service — abstract behind KMP expect/actual |
| D | Obsolete infrastructure — replaced by KorGE/KMP |
| E | Deferred optional integration |
| F | Generated code — do not translate |

---

## Legacy File Classification

### A — Port to commonMain

| File(s) | Notes |
|---------|-------|
| `ContentManager/Angles.cs` | Pure angle helpers |
| `ContentManager/EnemiesCount.cs` | Data: counts per wave |
| `ContentManager/Furniture.cs` | Data: furniture obstacle |
| `ContentManager/FurnitureComparer.cs` | Sorting by Y |
| `ContentManager/FurnitureOrientation.cs` | Enum |
| `ContentManager/FurnitureType.cs` | Enum: Basura,Arbol,Banco,Farola,Coche,CocheArdiendo,Puente |
| `ContentManager/Level.cs` | Data: spawn positions, zones |
| `ContentManager/LevelType.cs` | Enum: One–Ten, EndGame, EndDemo |
| `ContentManager/PowerUp.cs` | Data + active/dying timing |
| `ContentManager/PowerUpType.cs` | Enum |
| `ContentManager/SubLevel.cs` | Data: wave definition |
| `ContentManager/SubLevelType.cs` | Enum: One–Ten |
| `ContentManager/Wall.cs` | Data: line segment |
| `GameObjects/Avatar.cs` | Player simulation: speed=200px/s, crashRadius=20, lives=3, hp=100 |
| `GameObjects/AvatarState.cs` | Enum |
| `GameObjects/GunType.cs` | Enum: pistol,shotgun,grenade,flamethrower,machinegun |
| `GameObjects/ObjectStatus.cs` | Enum: Inactive,Active,Dying,Immune |
| `GameObjects/ShotgunShell.cs` | Projectile data |
| `GameObjects/Explosion.cs` | Timed explosion state |
| `GameObjects/Player.cs` | Strip XNA storage, keep game data |
| `GameObjects/Enemies/BaseEnemy.cs` | Enemy base: HP, status, score |
| `GameObjects/Enemies/EnemyType.cs` | Enum: Zombie,Tank,Rat,Wolf,Minotaur |
| `GameObjects/Enemies/Enemies.cs` | Enemy collection manager |
| `GameObjects/Enemies/Zombie.cs` | maxVelocity=1.5, score=10 |
| `GameObjects/Enemies/Rat.cs` | maxVelocity=1.5, score=15 |
| `GameObjects/Enemies/Wolf.cs` | maxVelocity=1.5, scale=1.1, score=30 |
| `GameObjects/Enemies/Minotaur.cs` | maxVelocity=1.5, scale=1.3, score=80 |
| `GameObjects/Enemies/Tank.cs` | maxVelocity=1.0 |
| `SteeringBehaviors/Arrive.cs` | Pure math |
| `SteeringBehaviors/Circle.cs` | Geometry helper |
| `SteeringBehaviors/GameWorld.cs` | World container for steering |
| `SteeringBehaviors/Geometry.cs` | Pure math |
| `SteeringBehaviors/ObstacleAvoidance.cs` | minDetectionBox=15 |
| `SteeringBehaviors/Pursuit.cs` | Pure math |
| `SteeringBehaviors/Randomizer.cs` | Seeded random |
| `SteeringBehaviors/Steering.cs` | Steering output |
| `SteeringBehaviors/SteeringBehaviors.cs` | Weights: obstacleAvoidance=15, pursuit=0.22 |
| `SteeringBehaviors/SteeringEntity.cs` | Agent base |
| `SteeringBehaviors/VectorHelper.cs` | Pure math |
| `SteeringBehaviors/Wall.cs` (steering) | Separate from ContentManager/Wall.cs |
| `SubsystemManagers/GameplayHelper.cs` | bulletSpeed=400, pelletSpeed=700, collisionDist=30 |
| `SubsystemManagers/RotatedRectangle.cs` | Collision math |
| `GameStateManagement/GameplayState.cs` | Adapt enum |
| `GameStateManagement/OptionsState.cs` | Settings data |
| `Localization/*.resx` | Extract 5 languages (EN,DE,ES,FR,IT) |

### B — Reimplement in KorGE

| File(s) | Replacement Strategy |
|---------|---------------------|
| `SubsystemManagers/AnimationManager.cs` | KorGE sprite animation |
| `SubsystemManagers/AudioManager.cs` | KorGE multiplatform audio |
| `SubsystemManagers/ResolutionManager.cs` | KorGE virtualSize=1280×720 |
| `MainScreens/GamePlayScreen.cs` | KorGE Scene |
| `MainScreens/LogoScreen.cs` | KorGE Scene |
| `MainScreens/StartScreen.cs` | KorGE Scene |
| `MainScreens/SelectPlayerScreen.cs` | KorGE Scene |
| `MainScreens/StoryTellingScreen.cs` | KorGE Scene |
| `MainScreens/GamePlayMenu.cs` | KorGE overlay container |
| `MainScreens/GameOverMenu.cs` | KorGE overlay container |
| `MainScreens/HowToPlayScreen.cs` | KorGE Scene |
| `MainScreens/OptionsScreen.cs` | KorGE Scene |
| `MenuScreens/CreditsScreen.cs` | KorGE Scene |
| `Components/MusicComponent.cs` | KorGE audio |
| `SubsystemManagers/InputManager.cs` | New `GameInput` interface |
| `SubsystemManagers/VirtualThumbsticks.cs` | Touch `GameInput` implementation |

### C — Abstract behind KMP expect/actual

| File(s) | Notes |
|---------|-------|
| `SubsystemManagers/StorageDataSource.cs` | multiplatform-settings |

### D — Obsolete (replaced by KMP/KorGE)

| File(s) | Reason |
|---------|--------|
| `GameStateManagement/ScreenManager.cs` | KorGE Scene system |
| `GameStateManagement/GameScreen.cs` | KorGE Scene |
| `GameStateManagement/InputState.cs` | New GameInput abstraction |
| `GameStateManagement/NeutralInput.cs` | New GameInput abstraction |
| `GameStateManagement/IScreenFactory.cs` | KorGE handles this |
| `SubsystemManagers/GamerManager.cs` | Xbox Live — obsolete |
| `SubsystemManagers/NetworkSessionManager.cs` | Xbox Live — obsolete |
| `SubsystemManagers/ScrollingTextManager.cs` | Not needed |
| `SteeringBehaviors/Primitive2D.cs` | Debug rendering only |
| `Components/DebugComponent.cs` | Debug only |
| `Components/FrameRateCounter.cs` | KorGE handles |
| `MenuScreens/LobbyScreen.cs` | Xbox Live — obsolete |
| `MenuScreens/MatchmakingMenuScreen.cs` | Xbox Live — obsolete |
| `MenuScreens/CreateFindMenuScreen.cs` | Xbox Live — obsolete |
| `SupportScreens/NetworkBusyScreen.cs` | Xbox Live — obsolete |

### E — Deferred

| File(s) | Reason |
|---------|--------|
| `SubsystemManagers/AchievementsManager.cs` | Steam integration |
| `MenuScreens/AchievementsScreen.cs` | Steam integration |
| `MenuScreens/LeaderBoardScreen.cs` | Steam integration |
| `OnlineDataSyncManager/*` | Online service |
| `BloomPostProcess/*` | Visual enhancement, not blocker |
| `PerlinNoisePostProcess/*` | Visual enhancement, not blocker |
| `MainScreens/LoadInScreen.cs` | Simple transition, deferred |
| `MenuScreens/DemoEndingScreen.cs` | Demo mode |
| Steam/Bugsnag/GameAnalytics | No credentials in new project |

### F — Generated (do not translate)

| File(s) | Reason |
|---------|--------|
| `Localization/Strings.*.Designer.cs` | Auto-generated from resx |

---

## Migration Phases

### Phase 1 — Foundation (current)
- [x] Audit both codebases
- [ ] Create migration documents
- [ ] Rename project identity SteelVectors → Zombusters
- [ ] Set virtual resolution 1280×720
- [ ] Port pure enums and data types

### Phase 2 — Data Layer
- [ ] Level XML parser (commonMain)
- [ ] Level data tests
- [ ] Animation definition XML parser
- [ ] Furniture/Wall data loading

### Phase 3 — Steering & AI
- [ ] SteeringEntity, Arrive, Pursuit, ObstacleAvoidance
- [ ] GameWorld, Geometry, VectorHelper
- [ ] Steering behavior unit tests

### Phase 4 — Core Game Objects
- [ ] Avatar / Player
- [ ] BaseEnemy / Enemy types
- [ ] ShotgunShell / projectiles
- [ ] PowerUp
- [ ] Explosion
- [ ] GameplayHelper (damage, collision constants)
- [ ] RotatedRectangle

### Phase 5 — Asset Recovery
- [ ] Music OGG/MP3 (available raw)
- [ ] Sprite XNB extraction audit
- [ ] Sound FX XNB extraction audit
- [ ] ASSET_MIGRATION.md completed

### Phase 6 — First Playable Vertical Slice
- [ ] GameInput abstraction
- [ ] Level 1 XML loaded
- [ ] Level 1 rendered (walls + furniture)
- [ ] Player rendered at spawn position
- [ ] Player movement
- [ ] Camera/viewport scaling
- [ ] Basic collision with walls

### Phase 7 — Combat & Enemies
- [ ] Enemy spawning from XML data
- [ ] Steering AI active
- [ ] Weapon firing
- [ ] Collision detection
- [ ] Damage and death
- [ ] PowerUps

### Phase 8 — Game Loop
- [ ] Wave/sublevel progression
- [ ] Score system
- [ ] Level completion
- [ ] Game over flow
- [ ] Pause menu

### Phase 9 — Audio
- [ ] Music playback
- [ ] Sound effects

### Phase 10 — Screens
- [ ] Logo / Title
- [ ] Main menu
- [ ] Select player
- [ ] Options
- [ ] Credits / How-to-play
- [ ] Storytelling

### Phase 11 — Platform
- [ ] Android build
- [ ] iOS build
- [ ] Settings persistence

### Phase 12 — Deferred
- [ ] Steam integration
- [ ] Achievements
- [ ] Leaderboard
- [ ] Bloom post-processing
- [ ] Perlin noise post-processing

---

## Key Constants (from legacy)

| Constant | Value | Source |
|----------|-------|--------|
| GAME_WIDTH | 1280 | MyGame.cs |
| GAME_HEIGHT | 720 | MyGame.cs |
| MAX_PLAYERS | 4 | MyGame.cs |
| Avatar.pixelsPerSecond | 200 | Avatar.cs |
| Avatar.crashRadius | 20f | Avatar.cs |
| Avatar.respawnTime | 2.0s | Avatar.cs |
| Avatar.immuneTime | 8.0s | Avatar.cs |
| Avatar.maxVelocity | 1.0f | Avatar.cs |
| Avatar.maxForce | 0.15f | Avatar.cs |
| Avatar.rateOfFire | 2/sec | Avatar.cs |
| Avatar.width | 28 | Avatar.cs |
| Avatar.height | 50 | Avatar.cs |
| Avatar.boundingRadius | 10.0f | Avatar.cs |
| Avatar.lives | 3 | Avatar.cs |
| Avatar.hp | 100 | Avatar.cs |
| Zombie.maxVelocity | 1.5f | Zombie.cs |
| Rat.maxVelocity | 1.5f | Rat.cs |
| Wolf.maxVelocity | 1.5f | Wolf.cs |
| Wolf.scale | 1.1f | Wolf.cs |
| Minotaur.maxVelocity | 1.5f | Minotaur.cs |
| Minotaur.scale | 1.3f | Minotaur.cs |
| Tank.maxVelocity | 1.0f | Tank.cs |
| ZOMBIE_SCORE | 10 | Enemies.cs |
| RAT_SCORE | 15 | Enemies.cs |
| WOLF_SCORE | 30 | Enemies.cs |
| MINOTAUR_SCORE | 80 | Enemies.cs |
| BULLET_SPEED | 400 | GameplayHelper.cs |
| PELLET_SPEED | 700 | GameplayHelper.cs |
| COLLISION_DISTANCE | 30 | GameplayHelper.cs |
| MACHINEGUN_RATE | 10/sec | GamePlayScreen.cs |
| FLAMETHROWER_RATE | 15/sec | GamePlayScreen.cs |
| SHOTGUN_RATE | 0.8/sec | GamePlayScreen.cs |
| EXTRA_LIFE_THRESHOLD | 8000 pts | Enemies.cs |
| POWERUP_ACTIVE_TIME | 20s | PowerUp.cs |
| POWERUP_DYING_TIME | 1.5s | PowerUp.cs |
| SHOTGUN_AMMO | 25 | PowerUp.cs |
| MACHINEGUN_AMMO | 50 | PowerUp.cs |
| FLAMETHROWER_AMMO | 25 | PowerUp.cs |
| GRENADES_COUNT | 5 | PowerUp.cs |
| HEALTH_RESTORE | 30 | PowerUp.cs |
| SPEED_BUFF_DURATION | 20s | PowerUp.cs |
| IMMUNE_BUFF_DURATION | 20s | PowerUp.cs |
| FX_VOLUME_DEFAULT | 0.7f | Player.cs |
| MUSIC_VOLUME_DEFAULT | 0.6f | AudioManager.cs |
| steeringWeightObstacleAvoidance | 15.0f | SteeringBehaviors.cs |
| steeringWeightPursuit | 0.22f | SteeringBehaviors.cs |
| obstacleAvoidanceBrakingWeight | 0.2f | ObstacleAvoidance.cs |
| flamethrowerRect | 88×43 | Avatar.cs |

---

## Discrepancies Found
- None yet. Update this section as legacy code and comments are found to disagree.

---

## Temporary Omissions
- Steam integration: DEFERRED — no credentials in new project. `// TODO DEFERRED: Steam integration`
- Bloom post-processing: DEFERRED pending gameplay completion
- Perlin noise post-processing: DEFERRED pending gameplay completion  
- Online leaderboard/achievements: DEFERRED
- Demo mode / DemoEndingScreen: DEFERRED
