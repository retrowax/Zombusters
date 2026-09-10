package com.retrowax.zombusters.game.model

// From legacy GunType.cs
enum class GunType(val id: Int) {
    PISTOL(0),
    SHOTGUN(1),
    GRENADE(2),
    FLAMETHROWER(3),
    MACHINEGUN(4)
}

// From legacy ObjectStatus.cs
enum class ObjectStatus {
    INACTIVE, ACTIVE, DYING, IMMUNE
}

// From legacy EnemyType.cs
enum class EnemyType {
    ZOMBIE, TANK, RAT, WOLF, MINOTAUR
}

// From legacy LevelType.cs
enum class LevelType {
    ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN, END_GAME, END_DEMO
}

// From legacy SubLevelType.cs
enum class SubLevelType {
    ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN
}

// From legacy PowerUpType.cs
enum class PowerUpType(val id: Int) {
    LIVE(0),
    MACHINEGUN(1),
    FLAMETHROWER(2),
    SHOTGUN(3),
    GRENADE(4),
    SPEED_BUFF(5),
    IMMUNE_BUFF(6),
    EXTRA_LIFE(7)
}

// From legacy FurnitureType.cs — Spanish original names preserved for asset key mapping
enum class FurnitureType {
    BASURA,          // trash/barrel
    ARBOL,           // tree
    BANCO,           // bench
    FAROLA,          // streetlight
    COCHE,           // car
    COCHE_ARDIENDO,  // burning car
    PUENTE           // bridge
}

// From legacy FurnitureOrientation.cs
enum class FurnitureOrientation {
    NORTH_EAST, NORTH_WEST, SOUTH_EAST, SOUTH_WEST
}

// From legacy AvatarState.cs
enum class AvatarState {
    IDLE, RUNNING, SHOOTING, DEAD
}

// From legacy GameplayState — adapted for KorGE scene model
enum class GameplayState {
    PLAYING, STAGE_CLEARED, PAUSE, START_LEVEL, GAME_OVER, NOT_PLAYING
}

// Player character selection (from legacy SelectPlayerScreen — 4 characters)
enum class PlayerCharacter {
    JADE, EGON, RAY, PETER
}
