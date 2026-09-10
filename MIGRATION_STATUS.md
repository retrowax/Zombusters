# Zombusters Migration Status

## Phase: First Playable Vertical Slice

### Completed ✅

| # | Task | Status |
|---|------|--------|
| 1 | Verify current state — key files read | ✅ Done |
| 2 | Project rename — proguard-rules.pro, iOS bundle ID, entitlements | ✅ Done |
| 3 | Remove SteelVectors/1942 game assets (aircraft, old logos, backgrounds) | ✅ Done |
| 4 | Fix duplicate GAME_WIDTH/GAME_HEIGHT — single source in GameConstants.kt | ✅ Done |
| 5 | LevelParser unit tests verifying Level 1 and Level 10 known XML values | ✅ 9 tests, all passing |
| 6 | zombusters_logo.png copied from legacy/media to commonMain drawable | ✅ Done |
| 7 | MainMenuScene — title, START GAME / EXIT, keyboard + mouse navigation | ✅ Done |
| 8 | GameplayWorld domain class — LevelDef + player1 + walls/furnitures | ✅ Done |
| 9 | Avatar domain model — position, status, lives, hp, score, timers | ✅ Done |
| 10 | GameInput — InputState with normalized diagonal movement | ✅ Done |
| 11 | LevelDebugView — cpuGraphics rendering walls/obstacle circles/spawn zones | ✅ Done (inside GameplayScene) |
| 12 | CollisionSystem — pure Kotlin, wall segment ≤ 5f, obstacle rangeDistance | ✅ Done + 9 tests passing |
| 13 | Level 1 loads via LevelParser; P1 at XML spawn (955,260); WASD/arrows | ✅ Done |
| 14 | Virtual 1280×720 via KorGE virtualSize — KorGE handles letterbox | ✅ Done |
| 15 | Music — menu: NuitNoire_OpeningThePortal.ogg, gameplay: BradSucks_BadAttraction.ogg | ✅ Done |
| 16 | Delete obsolete Collisionable.kt and UpdatableView.kt | ✅ Done |
| 17 | Enemies/combat/weapons/HUD/4-player/Steam explicitly deferred | ✅ Deferred |
| 18 | JVM desktop game built and launched (GLCanvasKorge starts) | ✅ Done |
| 19 | MIGRATION_STATUS.md and ASSET_MIGRATION.md updated | ✅ This file |

### Build Results
- `./gradlew :shared:compileKotlinJvm` → **BUILD SUCCESSFUL**
- `./gradlew :composeApp:compileKotlinDesktop` → **BUILD SUCCESSFUL**
- `./gradlew :shared:jvmTest` → **BUILD SUCCESSFUL — 18 tests, 0 failures**
- `./gradlew :composeApp:run` → Game window launches (GLCanvasKorge starts)

### Known Runtime Issues
- `IllegalArgumentException` in `AwtFrameTools.registerGestureListeners` — KorGE 7.0.0-SNAPSHOT internal AWT issue on macOS, not caused by game code. Non-critical; window opens.

### Security Notes
- GoogleService-Info.plist: left unchanged (belongs to old template, Firebase not used in new code)
- legacy/ directory: read-only, untouched
- No API keys or secrets migrated

---

## Architecture

```
commonMain/
  game/
    engine/
      CollisionSystem.kt       ← pure Kotlin collision math
    input/
      GameInput.kt             ← InputState + normalized diagonal movement
    model/
      Avatar.kt                ← domain model (no KorGE View)
      GameConstants.kt         ← authoritative GAME_WIDTH/HEIGHT + all constants
      Enums.kt                 ← all game enums
    scenes/
      MainMenuScene.kt         ← KorGE Scene — title, START/EXIT, music
      GameplayScene.kt         ← KorGE Scene — Level 1, player movement, debug geometry
    world/
      GameplayWorld.kt         ← domain — LevelDef, player1, walls, furnitures
      LevelParser.kt           ← XML parser for level data
      LevelData.kt             ← data classes: LevelDef, LevelWall, Furniture, etc.
platform/
  KorgeWrapper.kt              ← entry point — MainMenuScene, 1280×720 virtual size
```

---

## Next Phase Recommendations

1. **Enemy spawning** — spawn zombies at zone positions, basic AI steering toward player
2. **Combat** — pistol shooting, hit detection via CollisionSystem distance check
3. **HUD** — health bar, lives display, score counter using real XML spawn positions
4. **Sprite rendering** — replace cyan placeholder rect with actual character sprites
5. **Camera** — scroll/follow player; level is larger than 1280×720 viewport
6. **Level progression** — wave clearing, boss/end condition
