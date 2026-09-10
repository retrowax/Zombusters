# Zombusters Asset Migration

## Assets in commonMain/composeResources/drawable/

### KEPT (reusable for Zombusters)
| File | Notes |
|------|-------|
| arrow.png | UI arrow |
| bullet1.png | Projectile sprite |
| bullet3.png | Projectile sprite |
| bullet3_button.png | UI button |
| explosion_01 - 11.png | 11-frame explosion animation |
| heart.png | Health indicator |
| heart_power_up.png | Power-up sprite |
| life_icon.png | Lives HUD icon |
| pumpkin_icon.png | Zombusters theming |
| score_frame.png | HUD score frame |
| star_icon.png | Score/rating |
| zombusters_logo.png | **Copied from legacy/media/** — 350×350 Zombusters logo |
| bg_splash.xml | Splash background XML vector |
| horror_video_game_pana.xml | Illustration vector |
| ic_launcher_background.xml | App launcher background |
| mklogo.xml | Logo vector |

### REMOVED (SteelVectors / 1942 aircraft)
| File | Reason |
|------|--------|
| BF_109_Spritesheet.png | 1942 enemy aircraft — not Zombusters |
| FW_190_Spritesheet.png | 1942 enemy aircraft |
| Huricane_Spritesheet.png | 1942 player aircraft |
| La-5_Spritesheet.png | 1942 aircraft |
| ME_262_Spritesheet.png | 1942 aircraft |
| P-47_Spritesheet.png | 1942 aircraft |
| P-51_Spritesheet.png | 1942 aircraft |
| P_38_Spritesheet.png | 1942 aircraft |
| Spitfire_Spritesheet.png | 1942 aircraft |
| Yak_3_Spritesheet.png | 1942 aircraft |
| Zero _Spritesheet.png | 1942 aircraft |
| vertical_thrust_01-04.png | Aircraft thrust sprites |
| logo_steel_vectors.png | SteelVectors brand — wrong project |
| logo_steel_vectors_feature.png | SteelVectors brand |
| logo_steel_vectors_menu.png | SteelVectors brand |
| logo_steel_vectors512x512.png | SteelVectors brand |
| shot_power_up.png | 1942 power-up |
| lucida_sans1.png | Old font texture |
| lucida_sans2.png | Old font texture |
| large_blue_01/02.png | 1942 background tile |
| large_green_01.png | 1942 background tile |
| large_grey_01/02.png | 1942 background tile |
| large_purple_01.png | 1942 background tile |
| large_red_01.png | 1942 background tile |
| red_01-06.png | 1942 small tiles |
| green_04.png | 1942 small tile |
| game_background.png | 1942 full background |
| clouds.png | 1942 cloud layer |
| background_green.png | 1942 background |
| background_brown.png | 1942 background |

## Assets in commonMain/composeResources/files/music/
All 12 music tracks kept (Creative Commons licensed, suitable for gameplay):
- NuitNoire_OpeningThePortal.ogg — used for main menu
- BradSucks_BadAttraction.ogg — used for gameplay
- 10 additional tracks available for future levels

## Legacy Assets (READ-ONLY, not migrated)
Assets under legacy/media/ remain available as read-only source:
- Zombie spritesheets, character sprites, furniture textures
- These should be evaluated and copied to drawable/ as needed per feature
- zombusters_logo.png was the first one copied

## Not Yet Migrated (next phase)
- Character sprites (Jade, Egon, Ray, Peter)
- Zombie/enemy spritesheets
- Furniture textures (cars, trees, benches, streetlights)
- Weapon/projectile sprites specific to Zombusters
- Font files for Zombusters HUD
