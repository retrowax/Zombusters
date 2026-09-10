# Zombusters – Asset Migration

## Status Legend
- `AVAILABLE_RAW` — source file exists in legacy, ready to copy
- `EXTRACTED` — extracted from XNB
- `CONVERTED` — converted to target format
- `MISSING` — source is XNB only, not yet extracted
- `NOT_NEEDED` — asset not used in Zombusters
- `DEFERRED` — extraction deferred, placeholder in use

---

## Music (OGG/MP3 — AVAILABLE_RAW)

All music tracks exist as both `.ogg` and `.mp3` under `legacy/ZombustersWindows/ZombustersWindows/Content/Music/`.
Prefer `.ogg` for KorGE resources.

| Legacy Key | File | Status | Target Path |
|-----------|------|--------|------------|
| Music/BareWires_DancingOnADime | BareWires_DancingOnADime.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/BlackMath_HighDive | BlackMath_HighDive.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/BlackMath_SuckCity | BlackMath_SuckCity.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/BradSucks_BadAttraction | BradSucks_BadAttraction.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/BradSucks_UnderstoodByYourDad | BradSucks_UnderstoodByYourDad.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/Kraus_Wolfram | Kraus_Wolfram.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/LondonToTokyo_HighGround | LondonToTokyo_HighGround.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/NuitNoire_OpeningThePortal | NuitNoire_OpeningThePortal.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/TheBlackBug_IDontLikeYou | TheBlackBug_IDontLikeYou.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/TheItchyCreeps_ITHOTM | TheItchyCreeps_ITHOTM.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/ThisCo_AsYouKnow | ThisCo_AsYouKnow.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |
| Music/ThisCo_TakeItAway | ThisCo_TakeItAway.ogg | CONVERTED | shared/src/commonMain/composeResources/files/music/ |

---

## Sound Effects (XNB — MISSING)

All sound effects are compiled XNB files. Source audio not found in raw form.

| Legacy Content Key | Semantic Use | XNB Path | Status |
|-------------------|-------------|---------|--------|
| SoundFX/pistol_shot | Pistol fire | Content/SoundFX/pistol_shot.xnb | MISSING |
| SoundFX/shotgun | Shotgun fire | Content/SoundFX/shotgun.xnb | MISSING |
| SoundFX/explosion1 | Explosion | Content/SoundFX/explosion1.xnb | MISSING |
| SoundFX/zombiedying | Zombie death | Content/SoundFX/zombiedying.xnb | MISSING |
| SoundFX/womanscream | Jade death | Content/SoundFX/womanscream.xnb | MISSING |
| SoundFX/critedu | Male player death | Content/SoundFX/critedu.xnb | MISSING |
| SoundFX/minotaurdeath | Minotaur death | Content/SoundFX/minotaurdeath.xnb | MISSING |
| SoundFX/ratdeath | Rat death | Content/SoundFX/ratdeath.xnb | MISSING |
| SoundFX/wolfdeath | Wolf death | Content/SoundFX/wolfdeath.xnb | MISSING |
| SoundFX/flamethrower | Flamethrower loop | Content/SoundFX/flamethrower.xnb | MISSING |
| SoundFX/machinegun | Machinegun loop | Content/SoundFX/machinegun.xnb | MISSING |
| SoundFX/LogoSplashSound | Logo screen | Content/SoundFX/LogoSplashSound.xnb | MISSING |

---

## Textures / Sprites (XNB — MISSING unless found as raw)

All game graphics are compiled XNB files under `Content/`. KorGE cannot use XNB directly.

### Player Characters
| Legacy Key | Semantic Use | Status |
|-----------|-------------|--------|
| InGame/girl/girl_anim_* | Jade animations (idle, run, shoot, death) | MISSING |
| InGame/egon/egon_* | Egon animations | MISSING |
| InGame/peter/peter_* | Ray/Peter animations | MISSING |

### Enemies
| Legacy Key | Semantic Use | Status |
|-----------|-------------|--------|
| InGame/zombie/* | Zombie walk/burn/death (48×55, 48×56, 50×57) | MISSING |
| InGame/rat/* | Rat attack/death/hit/idle/run (48×48) | MISSING |
| InGame/wolf/* | Wolf animations (80×48) | MISSING |
| InGame/minotaur/* | Minotaur animations (128×80) | MISSING |
| InGame/tank/* | Tank animations | MISSING |

### Environment / Level
| Legacy Key | Semantic Use | Status |
|-----------|-------------|--------|
| InGame/level*/background | Level background images | MISSING |
| InGame/furniture/basura_* | Trash/barrel obstacles | MISSING |
| InGame/furniture/arbol_* | Tree obstacles | MISSING |
| InGame/furniture/banco_* | Bench obstacles | MISSING |
| InGame/furniture/farola_* | Streetlight obstacles | MISSING |
| InGame/furniture/coche_* | Car obstacles | MISSING |
| InGame/furniture/coche_ardiendo_* | Burning car obstacles | MISSING |
| InGame/furniture/puente_* | Bridge obstacles | MISSING |

### UI / HUD
| Legacy Key | Semantic Use | Status |
|-----------|-------------|--------|
| Menus/zombusters_logo | Main title logo | MISSING |
| Menus/start_screen_bg | Start screen background | MISSING |
| Menus/hud_* | HUD elements | MISSING |
| Menus/life_icon | Life indicator | MISSING |
| Menus/gun_icons | Gun type indicators | MISSING |

### Effects
| Legacy Key | Semantic Use | Status |
|-----------|-------------|--------|
| InGame/effects/explosion* | Explosion frames | MISSING |
| InGame/effects/bullet_* | Bullet sprites | MISSING |
| InGame/effects/pellet_* | Pellet sprites | MISSING |
| InGame/effects/flame_* | Flamethrower frames | MISSING |
| InGame/effects/grenade_* | Grenade sprite | MISSING |

---

## Fonts (XNB — MISSING)
| Legacy Key | Use | Status |
|-----------|-----|--------|
| Fonts/game_font | In-game text | MISSING |
| Fonts/menu_font | Menu text | MISSING |

Note: Template already includes Poppins TTF fonts — may be usable as substitutes.

---

## Level Data (XML — AVAILABLE_RAW)
All level XML files are plain text and directly usable.

| File | Status |
|------|--------|
| Content/LevelsDef.xml | AVAILABLE_RAW |
| Content/AnimationDef.xml | AVAILABLE_RAW |
| LevelXMLs/Level1/Level1_enemies.xml | AVAILABLE_RAW |
| LevelXMLs/Level1/Level1_furnitures.xml | AVAILABLE_RAW |
| LevelXMLs/Level1/Level1_walls.xml | AVAILABLE_RAW |
| LevelXMLs/Level2/* | AVAILABLE_RAW |
| LevelXMLs/Level3/* | AVAILABLE_RAW |
| LevelXMLs/Level4/* | AVAILABLE_RAW |
| LevelXMLs/Level5/* | AVAILABLE_RAW |
| LevelXMLs/Level6/* | AVAILABLE_RAW |
| LevelXMLs/Level7/* | AVAILABLE_RAW |
| LevelXMLs/Level8/* | AVAILABLE_RAW |
| LevelXMLs/Level9/* | AVAILABLE_RAW |
| LevelXMLs/Level10/* | AVAILABLE_RAW |

---

## XNB Extraction Notes

The legacy `Content/` directory contains 303 compiled `.xnb` files (MonoGame/XNA format).

Extraction tools that may work in this environment:
- `MonoGame.Content.Builder` (mgcb) — can sometimes decompile textures
- `xnb_node` (Node.js) — supports Texture2D → PNG
- `XNBExtractor` — Java tool

**Action required before Phase 6 (vertical slice):**
1. Attempt XNB extraction with available tools
2. Update status of each entry above from MISSING to EXTRACTED/CONVERTED
3. Place extracted PNGs in `shared/src/commonMain/composeResources/drawable/`
4. Preserve transparency, dimensions, and sprite sheet layout exactly

Until extraction is complete, development uses explicit placeholder views (colored rectangles with labels).
