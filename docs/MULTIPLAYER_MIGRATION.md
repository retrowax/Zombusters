# Multiplayer Migration Notes

## Original Legacy Behavior (legacy/ZombustersWindows)

### Player Model (`MyGame.cs`, `Player.cs`)
- `MAX_PLAYERS = 4` — constant
- `Player[] players = new Player[MAX_PLAYERS]` — fixed array
- Player colors: Blue (P1), Red (P2), Green (P3), Yellow (P4)
- Player names: `Strings.PlayerOneString` … `PlayerFourString`
- Each `Player` holds: `IsPlaying`, `playerIndex`, `inputMode`, `neutralInput`, `characterSelected`, `levelsUnlocked`, `Avatar`

### Joining (`SelectPlayerScreen.cs`)
- P1 always has a slot; starts with keyboard or gamepad
- P2–P4 join by pressing `Buttons.Start` (gamepad) or `Keys.Enter` (keyboard, only if keyboard not already in use)
- Character cycle: Left/Right arrows, **no duplicate character restriction** in original code
- Level selection: Up/Down arrows, capped to `player.levelsUnlocked`
- `CanStartGame()`: all `IsPlaying && isReady` players must be ready before the host can start

### Input Assignment (`GamePlayScreen.cs`, `InputManager.cs`)
- Each player has an explicit `PlayerIndex` (One/Two/Three/Four for XInput)
- Keyboard-only player gets P1 slot when no gamepad is present
- **Legacy bug (fixed in KMP port):** `accumMove` and `accumFire` are class-level fields on `GamePlayScreen`, not per-player. This means in the legacy code, multiple players' inputs could bleed into each other each frame. The KMP port gives each player their own `GameInput` instance.

### Spawn Positions (`Level.cs`, `LevelsDef.xml`)
- `LevelsDef.xml` attributes: `P1SpawnPos`, `P2SpawnPos`, `P3SpawnPos`, `P4SpawnPos`
- Parsed in a loop `for (i = 1; i < 5; i++)`
- KMP `LevelParser.kt` reads these into `LevelDef.spawnPositionFor(playerIndex: Int)`

### Enemy Targeting
- `numplayersIngame`: list of active player indices, passed to `enemies.InitializeEnemy`
- Enemies pursue the nearest active player
- No "last attacker" attribution for kills — first player whose bullet lands gets the score

### Damage & Death
- Per-player HP and lives (completely independent)
- `RespawnTime = 2.0f` seconds (from `Avatar.cs`) — player reappears at spawn point with full HP
- `ImmuneTime = 8.0f` seconds — player is immune to damage after respawn
- Game-over condition: when **any individual player's** `avatar.lives == 0` → `GamePlayStatus = GameplayState.GameOver`
  - **Note:** Not a shared lives pool. One player dying out does not end the game for others.
- The original lobby/session model does end the whole game when any player is eliminated.

### Difficulty Scaling
- Difficulty is **per-level** (1–10), NOT adjusted by player count
- Zombie/rat/wolf HP and speed scale with level difficulty rating, same for 1 or 4 players

### Pause
- ANY player's Start button OR global ESC triggers pause for all players

### Power-ups
- Power-up ownership in legacy: whichever player walks over it gets it
- Score attribution: each player has their own `avatar.score`
- Power-up spawn position: uses the killing player's `avatar.position`

---

## KMP Port Implementation

### Domain Model (`GameplayWorld.kt`)
```kotlin
class GameplayWorld(val levelDef: LevelDef, numPlayers: Int = 1) {
    val players: List<Avatar>            // 1–4, indexed by player slot
    val playerSteeringEntities: List<SteeringEntity>
    val combatStates: List<CombatState>  // per-player bullet tracking
    // Backward-compat aliases
    val player1: Avatar get() = players[0]
    val combatState: CombatState get() = combatStates[0]
}
```

### Multi-player Systems
- `DamageSystem.processBulletCollisionsMulti(combatStates, enemies, players, totalSec)`
  — iterates each player's CombatState, awards score to the shooter
- `DamageSystem.processEnemyContactMulti(enemies, players, totalSec)`
  — independent damage per active player
- `EnemySystem` targets nearest active player (by steering entity proximity)
- `PowerUpSystem.update(totalSec, player)` — called once per player

### Input Architecture
- Each player slot gets its own `GameInput` instance
- P1: keyboard+mouse or first gamepad
- P2–P4: additional gamepads (desktop) or not supported on touch-only devices
- **Mobile policy:** Single touch = single player. Physical controllers allowed if platform reports them.

### Missing (Step 8 deferral)
- SelectPlayerScene join flow: currently single-player only; 2–4 player join via gamepad not yet wired
- GameplayScene still uses `world.player1` for all logic — multi-player loop deferred
- Multiplayer HUD: per-player HP/lives/score not yet rendered

---

## Test Coverage
- `AvatarCombatTest.kt` — per-avatar damage, respawn, immune state machine
- `GameSessionTest.kt` — campaign progression, level unlock
- `PowerUpTest.kt` — power-up acquisition and ownership
