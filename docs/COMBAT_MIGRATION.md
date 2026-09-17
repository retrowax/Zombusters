# Combat System Migration — Zombusters XNA → KorGE

## Constants (from legacy Avatar.cs / GameplayHelper.cs)

| Constant | Value | Source |
|---|---|---|
| BULLET_SPEED | 400f px/s | GameplayHelper.cs |
| PELLET_SPEED | 700f px/s | GameplayHelper.cs |
| COLLISION_DISTANCE | 30f px | GameplayHelper.cs |
| AVATAR_CRASH_RADIUS | 20f px | Avatar.cs CrashRadius |
| ENEMY_CONTACT_THRESHOLD | 40f px | CrashRadius(20)+20 |
| AVATAR_RESPAWN_TIME | 2.0f s | Avatar.cs |
| AVATAR_IMMUNE_TIME | 8.0f s | Avatar.cs |
| AVATAR_HP | 100 | Avatar.cs lifecounter |
| AVATAR_LIVES | 3 | Avatar.cs |
| BULLET_MAX | 20 | Avatar.cs bulletmax |
| PELLET_MAX | 50 | Avatar.cs pelletmax |

## Weapon Stats (from GamePlayScreen.cs)

| Weapon | Rate of Fire | Ammo | Notes |
|---|---|---|---|
| Pistol | 2/s | unlimited | Default weapon |
| Machinegun | 10/s | finite | Power-up |
| Shotgun | 0.8/s | finite | 3 pellets per shell |
| Flamethrower | 15/s | finite | Rectangle collision |
| Grenade | — | finite | Deferred |

## Projectile Movement — FindBulletPosition (from GameplayHelper.cs)

Vector4 layout: `(startX, startY, firedAtSeconds, angle)`

t = totalGameSeconds - firedAt

| Direction | X formula | Y formula |
|---|---|---|
| N | startX | startY - SPEED*t |
| NE | startX + sin(angle) + SPEED*t | startY - cos(angle) - SPEED*t |
| E | startX + SPEED*t | startY |
| SE | startX + sin(angle) + SPEED*t | startY + cos(angle) + SPEED*t |
| S | startX | startY + SPEED*t |
| SW | startX - sin(angle) - SPEED*t | startY + cos(angle) + SPEED*t |
| W | startX - SPEED*t | startY |
| NW | startX - sin(angle) - SPEED*t | startY - cos(angle) - SPEED*t |

## Shotgun Pellet Spread — FindShotgunBulletPosition (from GameplayHelper.cs)

PELLET_SPREAD_ANGLE_90 = 0.09f (perpendicular spread factor)
PELLET_SPREAD_ANGLE_45 = 1.2f (diagonal spread factor)

3 pellets per shell (indices 0,1,2). Spread applied at position evaluation time.
- N: pellet 0 = X - SPEED*t*0.09, pellet 1 = center, pellet 2 = X + SPEED*t*0.09
- E: pellet 0 = Y - SPEED*t*0.09, pellet 1 = center, pellet 2 = Y + SPEED*t*0.09
- NE/SE/SW/NW: uses PELLET_SPREAD_ANGLE_45 factor on one axis

## Angle Classification (from Angles.cs, exact float boundaries)

| Direction | Condition |
|---|---|
| N | angle > -0.3925 && < 0.3925 |
| NE | angle > 0.3925 && < 1.1775 |
| E | angle > 1.1775 && < 1.9625 |
| SE | angle > 1.9625 && < 2.7275 (E checked first, so SE starts at 1.9625 effectively) |
| S | angle > 2.7275 || < -2.7275 |
| SW | angle < -1.9625 && > -2.7275 |
| W | angle < -1.1775 && > -1.9625 |
| NW | angle < -0.3925 && > -1.1775 |

## Muzzle Offsets — Pistol character=0 (from GamePlayScreen.PlayerFire)

| Direction | X offset | Y offset |
|---|---|---|
| N | +5 | -57 |
| NE | +27 | -60 |
| E | +35 | -34 |
| SE | +32 | -5 |
| S | +5 | +5 |
| SW | -35 | -5 |
| W | -37 | -34 |
| NW | -32 | -60 |

## Player Lifecycle (from Avatar.cs / Player.cs)

ACTIVE → [lifecounter drops to 0] → DYING → [after RESPAWN_TIME=2s, lives>0] → IMMUNE → [after IMMUNE_TIME=8s] → ACTIVE
If lives=0 after DYING: INACTIVE (Game Over)

Player.Destroy(): calls avatar.DestroyAvatar(totalGameSeconds), lives--, plays scream

## Score Values (from legacy Enemies.cs)

ZOMBIE=10, RAT=15, WOLF=30, MINOTAUR=80
Extra life at 8000 points.

## Level 1 Enemy Parameters (from GamePlayScreen.cs StartNewLevel)

zombieLife=1.0, zombieSpeed=0.0, ratLife=1.0, ratSpeed=0.8, wolfLife=1.0, wolfSpeed=3.0

## Power-Up Constants (from legacy PowerUp.cs)

| Type | Effect |
|---|---|
| LIVE | Restore 30 HP |
| MACHINEGUN | +50 machinegun ammo |
| SHOTGUN | +25 shotgun ammo |
| FLAMETHROWER | +25 flamethrower ammo |
| GRENADE | +5 grenades |
| SPEED_BUFF | Speed boost 20s |
| IMMUNE_BUFF | Immunity 20s |
| EXTRA_LIFE | +1 life |

Power-up lifespan: active=20s, dying=1.5s
Spawn chance: 1/14 on enemy kill
