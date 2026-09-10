# Zombusters – Migration Status

## Checklist

- [x] KMP baseline compiling
- [x] Project renamed (SteelVectors → Zombusters, package com.retrowax.zombusters)
- [x] Virtual resolution 1280×720 centralized (KorgeWrapper.kt)
- [x] Legacy model types ported (enums, data classes — Enums.kt, LevelData.kt, GameConstants.kt)
- [x] Level XML parser implemented (LevelParser.kt — all 10 levels)
- [ ] Level data tests passing
- [ ] Steering AI ported
- [ ] Steering AI unit tests
- [ ] Player model (Avatar)
- [ ] Enemy models (Zombie, Rat, Wolf, Minotaur, Tank)
- [x] Asset inventory complete (ASSET_MIGRATION.md — music OGGs copied, sprites/SFX MISSING)
- [ ] Music playing
- [ ] Level 1 environment rendered
- [ ] Player 1 rendered at correct spawn position
- [ ] Player 1 movement working
- [ ] Wall/furniture collision working
- [ ] Camera/viewport scaling correct
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
- [ ] Logo/title screen
- [ ] Main menu screen
- [ ] Select player screen
- [ ] Storytelling screen
- [ ] Options screen
- [ ] Credits / how-to-play
- [ ] Sound effects
- [ ] Settings persistence
- [ ] Localization (EN/DE/ES/FR/IT)
- [ ] Android build passing
- [ ] iOS build passing
- [ ] Steam integration (DEFERRED)
- [ ] Legacy parity review

## Milestones

### Milestone 1 — First Vertical Slice
**Status:** Not started  
**Criteria:**
1. Desktop app launches without crash
2. No SteelVectors branding visible
3. Zombusters title/logo displayed
4. Game uses 1280×720 logical space
5. Selecting Start loads Level 1
6. Level 1 data loaded from XML
7. Player 1 at correct spawn position
8. Player can move
9. Wall/furniture collision works
10. Rendering scales with window resize
11. Core game code in commonMain
12. Build passes

### Milestone 2 — Level 1 Combat Loop
**Status:** Not started  
**Criteria:** Enemy spawning, AI, combat, weapons, damage, death, power-ups, score, wave completion, game over

## Known Limitations / Deferred
- Steam integration: DEFERRED
- Bloom post-processing: DEFERRED  
- Perlin noise effect: DEFERRED
- Online leaderboard: DEFERRED
- XNB sound effects: MISSING (extraction required)
- XNB textures: MISSING (extraction required) — placeholder debug views in use where needed
