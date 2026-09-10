package com.retrowax.zombusters.game.model

// Virtual resolution — all game coordinates are in this space (from legacy MyGame.cs)
const val GAME_WIDTH = 1280f
const val GAME_HEIGHT = 720f
const val MAX_PLAYERS = 4

// Avatar / Player (from legacy Avatar.cs)
const val AVATAR_PIXELS_PER_SECOND = 200f
const val AVATAR_CRASH_RADIUS = 20f
const val AVATAR_RESPAWN_TIME = 2.0f
const val AVATAR_IMMUNE_TIME = 8.0f
const val AVATAR_MAX_VELOCITY = 1.0f
const val AVATAR_MAX_FORCE = 0.15f
const val AVATAR_RATE_OF_FIRE = 2f       // shots per second (pistol default)
const val AVATAR_BULLET_MAX = 20
const val AVATAR_PELLET_MAX = 50
const val AVATAR_HP = 100
const val AVATAR_LIVES = 3
const val AVATAR_WIDTH = 28
const val AVATAR_HEIGHT = 50
const val AVATAR_BOUNDING_RADIUS = 10f
const val AVATAR_FLAMETHROWER_RECT_W = 88
const val AVATAR_FLAMETHROWER_RECT_H = 43

// Weapon speeds (from legacy GameplayHelper.cs)
const val BULLET_SPEED = 400f
const val PELLET_SPEED = 700f
const val COLLISION_DISTANCE = 30f

// Rates of fire (from legacy GamePlayScreen.cs)
const val MACHINEGUN_RATE_OF_FIRE = 10f   // shots per second
const val FLAMETHROWER_RATE_OF_FIRE = 15f
const val SHOTGUN_RATE_OF_FIRE = 0.8f

// Enemy velocities (from individual enemy classes)
const val ZOMBIE_MAX_VELOCITY = 1.5f
const val RAT_MAX_VELOCITY = 1.5f
const val WOLF_MAX_VELOCITY = 1.5f
const val WOLF_SCALE = 1.1f
const val MINOTAUR_MAX_VELOCITY = 1.5f
const val MINOTAUR_SCALE = 1.3f
const val TANK_MAX_VELOCITY = 1.0f

// Score values (from legacy Enemies.cs)
const val ZOMBIE_SCORE = 10
const val RAT_SCORE = 15
const val WOLF_SCORE = 30
const val MINOTAUR_SCORE = 80
const val EXTRA_LIFE_SCORE_THRESHOLD = 8000

// Power-up timing (from legacy PowerUp.cs)
const val POWERUP_ACTIVE_TIME = 20f
const val POWERUP_DYING_TIME = 1.5f
const val POWERUP_SHOTGUN_AMMO = 25
const val POWERUP_MACHINEGUN_AMMO = 50
const val POWERUP_FLAMETHROWER_AMMO = 25
const val POWERUP_GRENADES = 5
const val POWERUP_HEALTH_RESTORE = 30
const val POWERUP_SPEED_BUFF_DURATION = 20f
const val POWERUP_IMMUNE_BUFF_DURATION = 20f

// Steering weights (from legacy SteeringBehaviors.cs)
const val STEERING_WEIGHT_OBSTACLE_AVOIDANCE = 15.0f
const val STEERING_WEIGHT_PURSUIT = 0.22f
const val STEERING_OBSTACLE_BRAKING_WEIGHT = 0.2f
const val STEERING_MIN_DETECTION_BOX = 15.0f

// Audio defaults (from legacy Player.cs / AudioManager.cs)
const val DEFAULT_FX_VOLUME = 0.7f
const val DEFAULT_MUSIC_VOLUME = 0.6f
