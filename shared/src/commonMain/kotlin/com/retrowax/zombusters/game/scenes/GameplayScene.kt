package com.retrowax.zombusters.game.scenes

import com.retrowax.zombusters.game.combat.FacingDirection
import com.retrowax.zombusters.game.debug.GameDebugConfig
import com.retrowax.zombusters.game.enemy.BaseEnemy
import com.retrowax.zombusters.game.enemy.Rat
import com.retrowax.zombusters.game.enemy.SteeringEntity
import com.retrowax.zombusters.game.enemy.Vec2
import com.retrowax.zombusters.game.enemy.Wolf
import com.retrowax.zombusters.game.engine.CollisionSystem
import com.retrowax.zombusters.game.input.DualVirtualStickState
import com.retrowax.zombusters.game.input.GameInput
import com.retrowax.zombusters.game.input.InputMode
import com.retrowax.zombusters.game.input.VirtualThumbstickLogic
import com.retrowax.zombusters.game.model.AVATAR_PIXELS_PER_SECOND
import com.retrowax.zombusters.game.model.EnemyType
import com.retrowax.zombusters.game.model.FurnitureOrientation
import com.retrowax.zombusters.game.model.FurnitureType
import com.retrowax.zombusters.game.model.GAME_HEIGHT
import com.retrowax.zombusters.game.model.GAME_WIDTH
import com.retrowax.zombusters.game.model.GameSession
import com.retrowax.zombusters.game.model.GunType
import com.retrowax.zombusters.game.persistence.GameSettings
import com.retrowax.zombusters.game.model.ObjectStatus
import com.retrowax.zombusters.game.systems.CombatSystem
import com.retrowax.zombusters.game.systems.DamageSystem
import com.retrowax.zombusters.game.ui.ZombustersFonts
import com.retrowax.zombusters.game.world.Furniture
import com.retrowax.zombusters.game.world.GameplayWorld
import com.retrowax.zombusters.game.world.LevelParser
import korlibs.audio.sound.readMusic
import korlibs.event.Key
import korlibs.image.bitmap.Bitmap
import korlibs.image.color.Colors
import korlibs.image.color.RGBA
import korlibs.image.format.readBitmap
import korlibs.korge.input.mouse
import korlibs.korge.input.onClick
import korlibs.korge.input.touch
import korlibs.korge.scene.Scene
import korlibs.korge.view.SContainer
import korlibs.korge.view.SpriteAnimation
import korlibs.korge.view.addUpdater
import korlibs.korge.view.container
import korlibs.korge.view.cpuGraphics
import korlibs.korge.view.image
import korlibs.korge.view.solidRect
import korlibs.korge.view.sprite
import korlibs.korge.view.text
import korlibs.io.file.std.resourcesVfs
import korlibs.math.geom.Point
import korlibs.math.geom.Size
import kotlinx.coroutines.launch
import kotlin.math.sqrt
import kotlin.time.Duration
import kotlin.time.Duration.Companion.seconds

// Player sprite offsets from legacy GamePlayScreen.cs line 1458
private const val PLAYER_RENDER_OFFSET_X = -20.0
private const val PLAYER_RENDER_OFFSET_Y = -55.0

// Jade idle animation from AnimationDef.xml JadeIdleTrunkDef
private const val JADE_IDLE_FRAME_W = 42
private const val JADE_IDLE_FRAME_H = 55
private const val JADE_IDLE_COLS = 11
private const val JADE_IDLE_FPS = 15

// Jade directional shot animations (legacy AnimationDef.xml JadePistolShot*Def)
private const val JADE_SHOT_E_W  = 71;  private const val JADE_SHOT_E_H  = 51;  private const val JADE_SHOT_E_COLS  = 8
private const val JADE_SHOT_NE_W = 71;  private const val JADE_SHOT_NE_H = 73;  private const val JADE_SHOT_NE_COLS = 8
private const val JADE_SHOT_N_W  = 32;  private const val JADE_SHOT_N_H  = 85;  private const val JADE_SHOT_N_COLS  = 8
private const val JADE_SHOT_SE_W = 71;  private const val JADE_SHOT_SE_H = 51;  private const val JADE_SHOT_SE_COLS = 8
private const val JADE_SHOT_S_W  = 31;  private const val JADE_SHOT_S_H  = 70;  private const val JADE_SHOT_S_COLS  = 8
private const val JADE_SHOT_FPS  = 21

// Trunk local offsets inside player container (relative to container origin = position + (-20,-55))
// Legacy positions: all shot anims at screen (position.X + 7 + offsetX, position.Y + offsetY + dy)
private const val JADE_TRUNK_SHOT_LX = 7.0  // +7 relative to container origin
private const val JADE_TRUNK_N_LY  = -30.0
private const val JADE_TRUNK_NE_LY = -18.0
private const val JADE_TRUNK_E_LY  =   4.0
private const val JADE_TRUNK_SE_LY =   4.0
private const val JADE_TRUNK_S_LY  =   4.0

// Legs sprite offsets inside container
// Legacy: position.X+7+offsetX(-20), position.Y+offsetY(-55)+3 → local (7, 3)
private const val LEGS_LOCAL_X = 7.0
private const val LEGS_LOCAL_Y = 3.0

// Run (legs) animation — JadeRunEDef: 49×24, 8 cols, Speed=15
private const val JADE_RUN_FRAME_W = 49
private const val JADE_RUN_FRAME_H = 24
private const val JADE_RUN_COLS = 8
private const val JADE_RUN_FPS = 15
// Run local positions in player container (east: x-7+offsetX, y-26 → local (-7,29))
// West is flipped (scaleX=-1): x_west = x_east + frameWidth = -7+49 = 42 so sprite
// stays at same screen position after flip (KorGE scaleX=-1 pivots at x=0).
private const val RUN_LOCAL_X_E = -7.0
private const val RUN_LOCAL_X_W = 42.0
private const val RUN_LOCAL_Y   = 29.0

// Shadow offsets inside container
// Legacy: position.X+legsW/2-5+offsetX(-20), position.Y+legsH-6+offsetY(-55) → local (8, 46)
private const val SHADOW_LOCAL_X = 8.0
private const val SHADOW_LOCAL_Y = 46.0
private const val SHADOW_ALPHA   = 0.6

// Enemy shadow offsets relative to enemy container (at (ex, ey-50))
// Legacy zombie: (entity.X-10, entity.Y-3) → local (-10, 47)
private const val ENEMY_SHADOW_LX = -10.0
private const val ENEMY_SHADOW_LY =  47.0

// Zombie walk: ZombieDef — 48×55, 8 cols, Speed=10
private const val ZOMBIE_FRAME_W = 48
private const val ZOMBIE_FRAME_H = 55
private const val ZOMBIE_COLS = 8
private const val ZOMBIE_FPS = 10

// Zombie death: ZombieDeathDef — 50×57, 12 cols, Speed=15
private const val ZOMBIE_DEATH_W = 50
private const val ZOMBIE_DEATH_H = 57
private const val ZOMBIE_DEATH_COLS = 12
private const val ZOMBIE_DEATH_FPS = 15

// Rat: 48×48 per frame
private const val RAT_FRAME_W = 48
private const val RAT_FRAME_H = 48
private const val RAT_IDLE_COLS = 8
private const val RAT_RUN_COLS = 6
private const val RAT_FPS = 12

// Rat death: 48×48, 5 cols
private const val RAT_DEATH_COLS = 5

// Wolf: 80×48 per frame
private const val WOLF_FRAME_W = 80
private const val WOLF_FRAME_H = 48
private const val WOLF_IDLE_COLS = 8
private const val WOLF_RUN_COLS = 6
private const val WOLF_FPS = 12

// Wolf death: 80×48, 6 cols
private const val WOLF_DEATH_COLS = 6

// Score values from Enemies.cs
private const val SCORE_ZOMBIE    = 10
private const val SCORE_RAT       = 15
private const val SCORE_WOLF      = 30
private const val SCORE_MINOTAUR  = 80

// Pause icon hit region from legacy GamePlayScreen.cs (1280×720 logical coords)
private const val PAUSE_ICON_X1 = 1088f
private const val PAUSE_ICON_Y1 = 39f
private const val PAUSE_ICON_X2 = 1135f
private const val PAUSE_ICON_Y2 = 83f

/**
 * Main gameplay scene — driven by [GameSession].
 *
 * Step 7: Input refactored.
 *   - All production movement/aim/fire uses [GameInput] — no direct keyboard in gameplay logic.
 *   - Touch: dual virtual thumbsticks via [DualVirtualStickState] (legacy VirtualThumbsticks.cs behavior).
 *   - Keyboard: WASD/arrows + SPACE mapped to [GameInput] each frame.
 *   - Debug keys (F1/F2/K/G/H/digits) remain as direct keyboard checks (developer tooling only).
 *   - Virtual stick visuals rendered as circle placeholders when InputMode.TOUCH.
 *   - Pause icon hit region (legacy coords 1088-1135, 39-83) for touch pause.
 */
class GameplayScene(
    private val exit: () -> Unit = {},
    private val drawableResourcesPath: String = "",
    private val fontResourcesPath: String = "",
    private val filesResourcesPath: String = "",
    private val session: GameSession = GameSession()
) : Scene() {

    private val assetBase get() = "$filesResourcesPath/zombusters"
    private val levelNumber get() = session.currentLevel.coerceIn(1, 10)

    override suspend fun SContainer.sceneInit() {
        ZombustersFonts.loadFrom(fontResourcesPath)

        val levelDef = if (filesResourcesPath.isNotEmpty()) {
            try { LevelParser(filesResourcesPath).loadLevel(levelNumber) } catch (_: Exception) { null }
        } else null

        if (levelDef == null) {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x22, 0x22, 0x22, 0xFF))
            text("Failed to load Level $levelNumber") {
                textSize = 32.0; color = Colors.RED
                x = 400.0; y = 340.0
            }
            return
        }

        val world = GameplayWorld(levelDef, session.numPlayers)
        world.player1.score = session.score
        world.player1.lives = session.lives

        // Load bitmaps
        val levelPad = levelNumber.toString().padStart(2, '0')
        val mapBitmap        = tryLoadBitmap("$assetBase/levels/level$levelPad/map.png")
        val charPath         = session.characterSpritePath
        val playerIdleBmp    = tryLoadBitmap("$assetBase/characters/$charPath/idle.png")
        val playerLegsBmp    = tryLoadBitmap("$assetBase/characters/$charPath/legs_idle.png")
        val playerRunBmp     = tryLoadBitmap("$assetBase/characters/$charPath/run_e.png")
        val shadowBmp        = tryLoadBitmap("$assetBase/characters/shadow.png")
        val hudPanelBmp      = tryLoadBitmap("$assetBase/hud/gameplay_gui_stats.png")
        val shotEBmp         = tryLoadBitmap("$assetBase/characters/$charPath/shot_e.png")
        val shotNEBmp        = tryLoadBitmap("$assetBase/characters/$charPath/shot_ne.png")
        val shotNBmp         = tryLoadBitmap("$assetBase/characters/$charPath/shot_n.png")
        val shotSEBmp        = tryLoadBitmap("$assetBase/characters/$charPath/shot_se.png")
        val shotSBmp         = tryLoadBitmap("$assetBase/characters/$charPath/shot_s.png")
        val furnitureBitmaps = loadFurnitureBitmaps()
        val zombieBitmap     = tryLoadBitmap("$assetBase/enemies/zombie/walk1.png")
        val ratIdleBitmap    = tryLoadBitmap("$assetBase/enemies/rat/idle.png")
        val ratRunBitmap     = tryLoadBitmap("$assetBase/enemies/rat/run.png")
        val wolfIdleBitmap   = tryLoadBitmap("$assetBase/enemies/wolf/idle.png")
        val wolfRunBitmap    = tryLoadBitmap("$assetBase/enemies/wolf/run.png")
        val zombieDeathBmp   = tryLoadBitmap("$assetBase/enemies/zombie/death.png")
        val ratDeathBmp      = tryLoadBitmap("$assetBase/enemies/rat/death.png")
        val wolfDeathBmp     = tryLoadBitmap("$assetBase/enemies/wolf/death.png")
        val minotaurBitmap   = tryLoadBitmap("$assetBase/enemies/minotaur/walk.png")
        val playerDiedBmp    = tryLoadBitmap("$assetBase/characters/$charPath/died.png")
        val hudPanelRedBmp   = tryLoadBitmap("$assetBase/hud/gui_stats_bkg_red.png")
        val hudPortraitBmp   = tryLoadBitmap("$assetBase/hud/${charPath}_gui.png")
        val hudHeartBmp      = tryLoadBitmap("$assetBase/hud/heart.png")
        val hudAmmoBmp       = tryLoadBitmap("$assetBase/hud/pistol_ammo.png")

        val idleAnim     = playerIdleBmp?.let { SpriteAnimation(it, spriteWidth = JADE_IDLE_FRAME_W, spriteHeight = JADE_IDLE_FRAME_H, columns = JADE_IDLE_COLS, rows = 1) }
        val shotEAnim    = shotEBmp?.let  { SpriteAnimation(it, spriteWidth = JADE_SHOT_E_W,  spriteHeight = JADE_SHOT_E_H,  columns = JADE_SHOT_E_COLS,  rows = 1) }
        val shotNEAnim   = shotNEBmp?.let { SpriteAnimation(it, spriteWidth = JADE_SHOT_NE_W, spriteHeight = JADE_SHOT_NE_H, columns = JADE_SHOT_NE_COLS, rows = 1) }
        val shotNAnim    = shotNBmp?.let  { SpriteAnimation(it, spriteWidth = JADE_SHOT_N_W,  spriteHeight = JADE_SHOT_N_H,  columns = JADE_SHOT_N_COLS,  rows = 1) }
        val shotSEAnim   = shotSEBmp?.let { SpriteAnimation(it, spriteWidth = JADE_SHOT_SE_W, spriteHeight = JADE_SHOT_SE_H, columns = JADE_SHOT_SE_COLS, rows = 1) }
        val shotSAnim    = shotSBmp?.let  { SpriteAnimation(it, spriteWidth = JADE_SHOT_S_W,  spriteHeight = JADE_SHOT_S_H,  columns = JADE_SHOT_S_COLS,  rows = 1) }

        val runEAnim     = playerRunBmp?.let { SpriteAnimation(it, spriteWidth = JADE_RUN_FRAME_W, spriteHeight = JADE_RUN_FRAME_H, columns = JADE_RUN_COLS, rows = 1) }

        val zombieAnim      = zombieBitmap?.let { SpriteAnimation(it, spriteWidth = ZOMBIE_FRAME_W, spriteHeight = ZOMBIE_FRAME_H, columns = ZOMBIE_COLS, rows = 1) }
        val zombieDeathAnim = zombieDeathBmp?.let { SpriteAnimation(it, spriteWidth = ZOMBIE_DEATH_W, spriteHeight = ZOMBIE_DEATH_H, columns = ZOMBIE_DEATH_COLS, rows = 1) }
        val ratIdleAnim     = ratIdleBitmap?.let { SpriteAnimation(it, spriteWidth = RAT_FRAME_W, spriteHeight = RAT_FRAME_H, columns = RAT_IDLE_COLS, rows = 1) }
        val ratRunAnim      = ratRunBitmap?.let { SpriteAnimation(it, spriteWidth = RAT_FRAME_W, spriteHeight = RAT_FRAME_H, columns = RAT_RUN_COLS, rows = 1) }
        val ratDeathAnim    = ratDeathBmp?.let { SpriteAnimation(it, spriteWidth = RAT_FRAME_W, spriteHeight = RAT_FRAME_H, columns = RAT_DEATH_COLS, rows = 1) }
        val wolfIdleAnim    = wolfIdleBitmap?.let { SpriteAnimation(it, spriteWidth = WOLF_FRAME_W, spriteHeight = WOLF_FRAME_H, columns = WOLF_IDLE_COLS, rows = 1) }
        val wolfRunAnim     = wolfRunBitmap?.let { SpriteAnimation(it, spriteWidth = WOLF_FRAME_W, spriteHeight = WOLF_FRAME_H, columns = WOLF_RUN_COLS, rows = 1) }
        val wolfDeathAnim   = wolfDeathBmp?.let { SpriteAnimation(it, spriteWidth = WOLF_FRAME_W, spriteHeight = WOLF_FRAME_H, columns = WOLF_DEATH_COLS, rows = 1) }

        // Map background
        if (mapBitmap != null) {
            image(mapBitmap) { x = 0.0; y = 0.0; zIndex = 0.0; smoothing = false }
        } else {
            solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0x22, 0x33, 0x22, 0xFF))
            text("Level $levelNumber — map not extracted") {
                textSize = 20.0; color = Colors.ORANGE; x = 20.0; y = 20.0; zIndex = 0.1
                font = ZombustersFonts.menuInfo
            }
        }

        buildFurnitureViews(world.furnitures, furnitureBitmaps)

        // Per-player trunk sprite references (for animation swapping based on fire direction)
        val playerTrunkSprites = mutableListOf<korlibs.korge.view.Sprite?>()
        // Per-player legs views: idle image and run sprite
        val playerLegsIdleViews  = mutableListOf<korlibs.korge.view.Image?>()
        val playerLegsRunSprites = mutableListOf<korlibs.korge.view.Sprite?>()
        // Per-player last known facing direction (for animation dirty-check)
        val playerLastFacing = MutableList(session.numPlayers) { FacingDirection.E }
        val playerLastFiring = MutableList(session.numPlayers) { false }
        val playerLastMoving = MutableList(session.numPlayers) { false }

        // Player containers — one per player
        val playerContainers = List(session.numPlayers) { playerIdx ->
            val c = container { zIndex = world.players[playerIdx].position.y }
            // Shadow (lowest layer, semi-transparent ellipse under feet)
            if (shadowBmp != null) {
                c.image(shadowBmp) {
                    x = SHADOW_LOCAL_X; y = SHADOW_LOCAL_Y
                    alpha = SHADOW_ALPHA; smoothing = false; zIndex = -0.1
                }
            }
            // Legs idle (static image, shown when player is not moving)
            val legsIdleView: korlibs.korge.view.Image? = if (playerLegsBmp != null) {
                c.image(playerLegsBmp) {
                    x = LEGS_LOCAL_X; y = LEGS_LOCAL_Y
                    smoothing = false; zIndex = 0.0
                }
            } else null
            playerLegsIdleViews.add(legsIdleView)

            // Legs run sprite (shown when player is moving)
            val legsRunSprite: korlibs.korge.view.Sprite? = if (runEAnim != null) {
                c.sprite(runEAnim) {
                    x = RUN_LOCAL_X_E; y = RUN_LOCAL_Y
                    smoothing = false; zIndex = 0.0; visible = false
                }.also { it.playAnimationLooped(runEAnim, (1.0 / JADE_RUN_FPS).seconds) }
            } else null
            playerLegsRunSprites.add(legsRunSprite)

            // Trunk — start with idle animation
            val trunkSprite: korlibs.korge.view.Sprite? = if (idleAnim != null) {
                c.sprite(idleAnim) {
                    smoothing = false; zIndex = 0.1; x = 0.0; y = 0.0
                }.also { it.playAnimationLooped(idleAnim, (1.0 / JADE_IDLE_FPS).seconds) }
            } else {
                val col = listOf(Colors.CYAN, Colors.LIME, Colors.YELLOW, Colors.ORANGE).getOrElse(playerIdx) { Colors.WHITE }
                c.solidRect(20.0, 20.0, col).also { it.x = -10.0; it.y = -10.0 }
                null
            }
            playerTrunkSprites.add(trunkSprite)
            c
        }
        val playerContainer = playerContainers[0]  // backward-compat alias for single-player HUD/refs

        val bulletGraphics = cpuGraphics { }
        bulletGraphics.zIndex = 900.0

        // Touch virtual stick overlay — redrawn each frame
        val touchOverlayGraphics = cpuGraphics { }
        touchOverlayGraphics.zIndex = 1050.0

        // HUD — bitmap panel + character portrait + stats text
        // Panel background: neutral frame first, then colored overlay on top (legacy: UIStats then UIStatsRed for player 1)
        if (hudPanelBmp != null) {
            image(hudPanelBmp) { x = 10.0; y = 0.0; zIndex = 999.0; smoothing = false }
        } else {
            solidRect(214.0, 69.0, RGBA(0, 0, 0, 180)).apply { x = 10.0; y = 0.0; zIndex = 999.0 }
        }
        // Colored background overlay (player 1 = red by default; drawn at same pos per legacy DrawUI)
        if (hudPanelRedBmp != null) {
            image(hudPanelRedBmp) { x = 10.0; y = 0.0; zIndex = 999.1; smoothing = false }
        }
        // Character portrait (partially off-screen left per legacy art style)
        if (hudPortraitBmp != null) {
            image(hudPortraitBmp) {
                x = 10.0 - hudPortraitBmp.width / 2.0 + 5.0  // ≈ -62
                y = 20.0 - 34.0  // = -14
                zIndex = 1000.0; smoothing = false
            }
        }
        // Heart icon at (Pos.X+120, Pos.Y+3) = (130, 23)
        if (hudHeartBmp != null) {
            image(hudHeartBmp) { x = 130.0; y = 23.0; zIndex = 1000.0; smoothing = false }
        }
        // Ammo icon at (Pos.X+124, Pos.Y+23) = (134, 43)
        if (hudAmmoBmp != null) {
            image(hudAmmoBmp) { x = 134.0; y = 43.0; zIndex = 1000.0; smoothing = false }
        }
        val heartW = hudHeartBmp?.width?.toDouble() ?: 19.0
        // Text overlays on HUD panel — positions match legacy (Pos.X=10, Pos.Y=20)
        val hudLives   = text("x${session.lives}") { textSize = 18.0; color = Colors.WHITE;  x = 60.0;  y = 4.0;  zIndex = 1001.0; font = ZombustersFonts.menuInfo }
        val hudHealth  = text("100")              { textSize = 14.0; color = Colors.LIME;    x = 10.0 + heartW + 125.0; y = 4.0;  zIndex = 1001.0; font = ZombustersFonts.menuInfo }
        val hudAmmo    = text("- - -")            { textSize = 12.0; color = Colors.WHITE;   x = 10.0 + heartW + 130.0; y = 44.0; zIndex = 1001.0; font = ZombustersFonts.menuInfo }
        val hudScore   = text("SC${session.score.toString().padStart(7, '0')}") { textSize = 11.0; color = Colors.YELLOW; x = 14.0; y = 54.0; zIndex = 1001.0; font = ZombustersFonts.menuInfo }
        // Wave / level / enemies info (center-top)
        val hudWave    = text("Wave 1")             { textSize = 14.0; color = Colors.LIGHTGRAY; x = 430.0; y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudLevel   = text("LEVEL $levelNumber") { textSize = 14.0; color = Colors.WHITE;     x = 590.0; y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }
        val hudEnemies = text("Enemies: 0")         { textSize = 14.0; color = Colors.LIGHTGRAY; x = 720.0; y = 4.0;  zIndex = 1000.0; font = ZombustersFonts.menuInfo }

        // Pause overlay (legacy GamePlayMenu entries)
        val pauseOverlay = solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0, 0, 0, 160)).apply { zIndex = 1100.0; visible = false }
        val pauseLabel = text("PAUSED") {
            textSize = 48.0; color = Colors.WHITE
            x = GAME_WIDTH / 2.0 - 80.0; y = GAME_HEIGHT / 2.0 - 80.0; zIndex = 1101.0; visible = false
            font = ZombustersFonts.menuHeader
        }
        val pauseEntries = listOf("RESUME", "HOW TO PLAY", "OPTIONS", "RESTART LEVEL", "QUIT TO MAIN MENU")
        var pauseMenuIndex = 0
        val pauseEntryViews = pauseEntries.mapIndexed { i, label ->
            text(label) {
                textSize = 30.0
                x = GAME_WIDTH / 2.0 - 160.0; y = GAME_HEIGHT / 2.0 - 20.0 + i * 46.0
                zIndex = 1101.0; visible = false
                font = ZombustersFonts.menuList
            }
        }

        // Game-over overlay (legacy GameOverMenu)
        val gameOverBmp = tryLoadBitmap("$assetBase/hud/gameover.png")
        val gameOverOverlay = solidRect(GAME_WIDTH.toDouble(), GAME_HEIGHT.toDouble(), RGBA(0, 0, 0, 200)).apply { zIndex = 1200.0; visible = false }
        val gameOverImgView = if (gameOverBmp != null) {
            image(gameOverBmp) {
                x = GAME_WIDTH / 2.0 - gameOverBmp.width / 2.0
                y = GAME_HEIGHT * 0.1
                zIndex = 1201.0; visible = false; smoothing = true
            }
        } else null
        val gameOverEntries = listOf("RESTART THIS WAVE", "RESTART FROM BEGINNING", "RETURN TO MAIN MENU")
        var gameOverMenuIndex = 0
        val gameOverEntryViews = gameOverEntries.mapIndexed { i, label ->
            text(label) {
                textSize = 30.0
                x = GAME_WIDTH / 2.0 - 200.0
                y = GAME_HEIGHT * 0.45 + i * 52.0
                zIndex = 1201.0; visible = false
                font = ZombustersFonts.menuList
            }
        }

        // Wave/level transition overlay — matches legacy StageCleared + StartLevel states
        val transitionLine1  = solidRect((GAME_WIDTH * 0.6).toInt().toDouble(), 3.0, Colors.WHITE).apply {
            x = GAME_WIDTH * 0.2; y = GAME_HEIGHT / 2.0 - 10.0; zIndex = 1199.0; visible = false
        }
        val transitionLine2  = solidRect((GAME_WIDTH * 0.6).toInt().toDouble(), 3.0, Colors.WHITE).apply {
            x = GAME_WIDTH * 0.2; y = GAME_HEIGHT / 2.0 + 90.0; zIndex = 1199.0; visible = false
        }
        val transitionLabel1 = text("CLEARED") {
            textSize = 56.0; color = Colors.YELLOW
            x = GAME_WIDTH / 2.0 - 120.0; y = GAME_HEIGHT / 2.0 - 40.0
            zIndex = 1200.0; visible = false; font = ZombustersFonts.menuHeader
        }
        val transitionLabel2 = text("PREPARE NEXT WAVE") {
            textSize = 28.0; color = Colors.WHITE
            x = GAME_WIDTH / 2.0 - 160.0; y = GAME_HEIGHT / 2.0 + 50.0
            zIndex = 1200.0; visible = false; font = ZombustersFonts.menuList
        }
        // Stage-complete overlay (all waves in level done)
        val stageClearedLabel = text("STAGE CLEARED!") {
            textSize = 56.0; color = Colors.YELLOW
            x = GAME_WIDTH / 2.0 - 200.0; y = GAME_HEIGHT / 2.0 - 40.0
            zIndex = 1200.0; visible = false; font = ZombustersFonts.menuHeader
        }

        fun showTransitionPhase1() {
            transitionLabel1.text = "CLEARED"; transitionLabel2.text = "PREPARE NEXT WAVE"
            transitionLine1.visible = true; transitionLine2.visible = true
            transitionLabel1.visible = true; transitionLabel2.visible = true
        }
        fun showTransitionPhase2(lvl: Int, wave: Int) {
            transitionLabel1.text = "LEVEL $lvl"; transitionLabel2.text = "WAVE $wave"
            transitionLine1.visible = true; transitionLine2.visible = true
            transitionLabel1.visible = true; transitionLabel2.visible = true
        }
        fun hideTransition() {
            transitionLine1.visible = false; transitionLine2.visible = false
            transitionLabel1.visible = false; transitionLabel2.visible = false
        }

        fun refreshPauseColors() {
            pauseEntryViews.forEachIndexed { i, v -> v.color = if (i == pauseMenuIndex) Colors.YELLOW else Colors.WHITE }
        }
        fun refreshGameOverColors() {
            gameOverEntryViews.forEachIndexed { i, v -> v.color = if (i == gameOverMenuIndex) Colors.YELLOW else Colors.WHITE }
        }

        // Enemy view pool
        val enemyViews = mutableListOf<Pair<BaseEnemy, korlibs.korge.view.Container>>()
        // Per-enemy main sprite reference (for death animation swapping)
        val enemyMainSprites = mutableMapOf<BaseEnemy, korlibs.korge.view.Sprite?>()
        // Previous enemy status for transition detection
        val prevEnemyStatuses = mutableMapOf<BaseEnemy, ObjectStatus>()
        // Score popup list: (text view, creation time)
        val scorePopups = mutableListOf<Pair<korlibs.korge.view.Text, Float>>()

        // Player died static image (shown at death position, separate from animated container)
        val playerDiedViews = List(session.numPlayers) { playerIdx ->
            if (playerDiedBmp != null) {
                image(playerDiedBmp) {
                    zIndex = 500.0; visible = false; smoothing = false
                    x = world.players[playerIdx].position.x + PLAYER_RENDER_OFFSET_X
                    y = world.players[playerIdx].position.y + PLAYER_RENDER_OFFSET_Y
                }
            } else null
        }

        fun spawnEnemyViews(newEnemies: List<BaseEnemy>) {
            for (enemy in newEnemies) {
                val c = container { zIndex = enemy.entity.position.y.toDouble() }
                // Shadow under each enemy
                if (shadowBmp != null) {
                    c.image(shadowBmp) {
                        x = ENEMY_SHADOW_LX; y = ENEMY_SHADOW_LY
                        alpha = SHADOW_ALPHA; smoothing = false; zIndex = -0.1
                    }
                }
                var mainSprite: korlibs.korge.view.Sprite? = null
                when (enemy.type) {
                    EnemyType.ZOMBIE -> {
                        val anim = zombieAnim
                        if (anim != null) { val s = c.sprite(anim) { smoothing = false }; s.playAnimationLooped(anim, (1.0 / ZOMBIE_FPS).seconds); mainSprite = s }
                        else c.solidRect(20.0, 20.0, Colors.RED).also { it.x = -10.0; it.y = -10.0 }
                    }
                    EnemyType.RAT -> {
                        val anim = ratRunAnim ?: ratIdleAnim
                        if (anim != null) { val s = c.sprite(anim) { smoothing = false }; s.playAnimationLooped(anim, (1.0 / RAT_FPS).seconds); mainSprite = s }
                        else c.solidRect(16.0, 16.0, Colors.YELLOW).also { it.x = -8.0; it.y = -8.0 }
                    }
                    EnemyType.WOLF -> {
                        val anim = wolfRunAnim ?: wolfIdleAnim
                        if (anim != null) { val s = c.sprite(anim) { smoothing = false }; s.playAnimationLooped(anim, (1.0 / WOLF_FPS).seconds); mainSprite = s }
                        else c.solidRect(24.0, 24.0, Colors.MAGENTA).also { it.x = -12.0; it.y = -12.0 }
                    }
                    EnemyType.MINOTAUR -> {
                        if (minotaurBitmap != null) {
                            val anim = SpriteAnimation(minotaurBitmap, spriteWidth = 80, spriteHeight = 80, columns = 6, rows = 1)
                            val s = c.sprite(anim) { smoothing = false }; s.playAnimationLooped(anim, (1.0 / 10).seconds); mainSprite = s
                        } else c.solidRect(30.0, 30.0, Colors.BROWN).also { it.x = -15.0; it.y = -15.0 }
                    }
                    else -> c.solidRect(20.0, 20.0, Colors.ORANGE).also { it.x = -10.0; it.y = -10.0 }
                }
                enemyMainSprites[enemy] = mainSprite
                prevEnemyStatuses[enemy] = enemy.status
                enemyViews.add(Pair(enemy, c))
            }
        }

        // Debug overlay (F1)
        var debugMode = false
        val debugOverlay = cpuGraphics {
            for (wall in world.walls) {
                stroke(Colors.WHITE, lineWidth = 2.0) {
                    moveTo(Point(wall.fromX.toDouble(), wall.fromY.toDouble()))
                    lineTo(Point(wall.toX.toDouble(), wall.toY.toDouble()))
                }
            }
            for (f in world.furnitures) {
                stroke(Colors.ORANGE, lineWidth = 1.5) {
                    circle(Point(f.obstaclePositionX.toDouble(), f.obstaclePositionY.toDouble()), f.obstacleRadius.toDouble())
                }
            }
            for (z in world.spawnZones) {
                stroke(Colors.GREEN, lineWidth = 1.0) {
                    rect(z.xMin.toDouble(), z.yMin.toDouble(), (z.xMax - z.xMin).toDouble(), (z.yMax - z.yMin).toDouble())
                }
            }
        }
        debugOverlay.zIndex = 999.0
        debugOverlay.visible = false

        if (filesResourcesPath.isNotEmpty()) {
            try {
                val music = resourcesVfs["$filesResourcesPath/music/BradSucks_BadAttraction.ogg"].readMusic()
                music.playNoCancelForever()
            } catch (_: Exception) {}
        }

        // ── Input state ──────────────────────────────────────────────────────
        val dualStick = DualVirtualStickState()
        var pauseIconTouched = false  // set by touch handler, consumed by addUpdater

        // Register touch event handlers (event-based, fires on the KorGE main thread)
        touch {
            start { info ->
                val pos = Vec2(info.local.x.toFloat(), info.local.y.toFloat())
                // Pause icon hit (legacy: 1088-1135, 39-83 in 1280×720 logical)
                if (pos.x in PAUSE_ICON_X1..PAUSE_ICON_X2 && pos.y in PAUSE_ICON_Y1..PAUSE_ICON_Y2) {
                    pauseIconTouched = true
                    return@start
                }
                dualStick.onTouchDown(info.id, pos)
            }
            move { info ->
                dualStick.onTouchMove(info.id, Vec2(info.local.x.toFloat(), info.local.y.toFloat()))
            }
            end { info ->
                dualStick.onTouchUp(info.id)
            }
        }

        var totalSeconds = 0f
        var isPaused = false
        var pauseConfirmed = false
        var isGameOver = false
        var gameOverConfirmed = false
        var isStageClear = false
        var stageClearTimer = 0f
        // Wave transition phases: 0=none, 1=cleared (show CLEARED msg), 2=starting (show LEVEL/WAVE msg)
        var waveTransitionPhase = 0
        var waveTransitionTimer = 0f
        var debugF2 = false
        // Per-player direction tracking
        val lastFireAngles = MutableList(session.numPlayers) { FacingDirection.angleFrom(0f, -1f) }
        val lastMoveDxList = MutableList(session.numPlayers) { 0f }
        val lastMoveDyList = MutableList(session.numPlayers) { -1f }
        // Gamepad just-pressed prev state for P2-P4 (indices 0-2 = gamepads[0-2])
        val prevGpStart = BooleanArray(3)
        val prevGpR1    = BooleanArray(3)
        var currentInputMode = InputMode.KEYBOARD_MOUSE

        val capturedViews = views
        val sceneScope = this@GameplayScene

        addUpdater { dt: Duration ->
            val ks = capturedViews.input.keys

            // ── Determine input mode ─────────────────────────────────────────
            currentInputMode = when {
                capturedViews.input.isTouchDevice || dualStick.left.active || dualStick.right.active -> InputMode.TOUCH
                else -> InputMode.KEYBOARD_MOUSE
            }

            // ── Build semantic GameInput ─────────────────────────────────────
            val gameInput: GameInput = when (currentInputMode) {
                InputMode.TOUCH -> {
                    val consumed = pauseIconTouched
                    pauseIconTouched = false
                    GameInput(
                        movement = dualStick.movementVector,
                        aim = dualStick.aimVector,
                        firing = dualStick.isFiring,
                        pauseJustPressed = consumed,
                        confirmJustPressed = false,
                        cancelJustPressed = false,
                        weaponNextJustPressed = false,
                    )
                }
                else -> {
                    pauseIconTouched = false
                    var dx = 0f; var dy = 0f
                    if (ks[Key.W] || ks[Key.UP])    dy -= 1f
                    if (ks[Key.S] || ks[Key.DOWN])   dy += 1f
                    if (ks[Key.A] || ks[Key.LEFT])   dx -= 1f
                    if (ks[Key.D] || ks[Key.RIGHT])  dx += 1f
                    val len = sqrt(dx * dx + dy * dy)
                    if (len > 0f) { dx /= len; dy /= len }
                    val movVec = Vec2(dx, dy)
                    val firing = ks[Key.SPACE]
                    val aimVec = if (firing && len > 0f) movVec else Vec2(0f, 0f)
                    GameInput(
                        movement = movVec,
                        aim = aimVec,
                        firing = firing,
                        pauseJustPressed = ks.justPressed(Key.ESCAPE),
                        confirmJustPressed = ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE),
                        cancelJustPressed = ks.justPressed(Key.ESCAPE),
                        weaponNextJustPressed = ks.justPressed(Key.TAB),
                    )
                }
            }

            // ── Debug keys — direct keyboard only (developer tooling) ────────
            if (GameDebugConfig.ENABLED) {
                if (ks.justPressed(Key.F2)) debugF2 = !debugF2
                if (debugMode && debugF2) {
                    val jumpLevel = when {
                        ks.justPressed(Key.N1) || ks.justPressed(Key.NUMPAD1) -> 1
                        ks.justPressed(Key.N2) || ks.justPressed(Key.NUMPAD2) -> 2
                        ks.justPressed(Key.N3) || ks.justPressed(Key.NUMPAD3) -> 3
                        ks.justPressed(Key.N4) || ks.justPressed(Key.NUMPAD4) -> 4
                        ks.justPressed(Key.N5) || ks.justPressed(Key.NUMPAD5) -> 5
                        ks.justPressed(Key.N6) || ks.justPressed(Key.NUMPAD6) -> 6
                        ks.justPressed(Key.N7) || ks.justPressed(Key.NUMPAD7) -> 7
                        ks.justPressed(Key.N8) || ks.justPressed(Key.NUMPAD8) -> 8
                        ks.justPressed(Key.N9) || ks.justPressed(Key.NUMPAD9) -> 9
                        ks.justPressed(Key.N0) || ks.justPressed(Key.NUMPAD0) -> 10
                        else -> -1
                    }
                    if (jumpLevel in 1..10) {
                        val jumpSession = session.copy(currentLevel = jumpLevel, debugLevelJump = true)
                        sceneScope.launch {
                            sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, jumpSession)
                            }
                        }
                        return@addUpdater
                    }
                }
            }

            // ── Game-over menu ───────────────────────────────────────────────
            if (isGameOver) {
                if (gameInput.movement.y < -0.5f || ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                    gameOverMenuIndex = (gameOverMenuIndex - 1 + gameOverEntries.size) % gameOverEntries.size
                    refreshGameOverColors()
                }
                if (gameInput.movement.y > 0.5f || ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                    gameOverMenuIndex = (gameOverMenuIndex + 1) % gameOverEntries.size
                    refreshGameOverColors()
                }
                if (!gameOverConfirmed && (gameInput.confirmJustPressed || ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE))) {
                    gameOverConfirmed = true
                    sceneScope.launch {
                        when (gameOverMenuIndex) {
                            0 -> sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session)
                            }
                            1 -> sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath,
                                    GameSession.newGame(session.characterIndex))
                            }
                            2 -> sceneContainer.changeTo {
                                MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                        }
                    }
                }
                return@addUpdater
            }

            // ── Pause menu ───────────────────────────────────────────────────
            if (isPaused) {
                if (gameInput.movement.y < -0.5f || ks.justPressed(Key.UP) || ks.justPressed(Key.W)) {
                    pauseMenuIndex = (pauseMenuIndex - 1 + pauseEntries.size) % pauseEntries.size
                    refreshPauseColors()
                }
                if (gameInput.movement.y > 0.5f || ks.justPressed(Key.DOWN) || ks.justPressed(Key.S)) {
                    pauseMenuIndex = (pauseMenuIndex + 1) % pauseEntries.size
                    refreshPauseColors()
                }
                val resumeTouch = gameInput.pauseJustPressed || gameInput.cancelJustPressed || ks.justPressed(Key.ESCAPE)
                if (resumeTouch) {
                    isPaused = false
                    dualStick.resetAll()
                    pauseOverlay.visible = false; pauseLabel.visible = false
                    pauseEntryViews.forEach { it.visible = false }
                }
                if (!pauseConfirmed && (gameInput.confirmJustPressed || ks.justPressed(Key.RETURN) || ks.justPressed(Key.SPACE))) {
                    pauseConfirmed = true
                    sceneScope.launch {
                        when (pauseMenuIndex) {
                            0 -> {
                                isPaused = false
                                pauseConfirmed = false
                                dualStick.resetAll()
                                pauseOverlay.visible = false; pauseLabel.visible = false
                                pauseEntryViews.forEach { it.visible = false }
                            }
                            1 -> sceneContainer.changeTo {
                                HowToPlayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                            2 -> sceneContainer.changeTo {
                                OptionsScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                            3 -> sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session)
                            }
                            4 -> sceneContainer.changeTo {
                                MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                        }
                    }
                }
                return@addUpdater
            }

            // ── Enter pause (P1 or any connected gamepad START) ──────────────
            val anyGpPauseJP = (0 until minOf(session.numPlayers - 1, 3)).any { gpIdx ->
                val gp = capturedViews.input.gamepads[gpIdx]
                gp.connected && gp.start && !prevGpStart[gpIdx]
            }
            if (gameInput.pauseJustPressed || anyGpPauseJP) {
                isPaused = true
                pauseMenuIndex = 0
                dualStick.resetAll()   // clear sticks so no movement/fire resumes on unpause
                pauseOverlay.visible = true; pauseLabel.visible = true
                pauseEntryViews.forEach { it.visible = true }
                refreshPauseColors()
                return@addUpdater
            }

            if (isPaused || isGameOver) return@addUpdater

            // ── Stage-clear advance ──────────────────────────────────────────
            if (isStageClear) {
                stageClearTimer += dt.inWholeMilliseconds / 1000f
                if (stageClearTimer >= 2.5f || gameInput.confirmJustPressed) {
                    val updatedSession = session.copy(
                        currentLevel = levelNumber,
                        score = world.player1.score,
                        lives = world.player1.lives
                    ).withNextLevel()
                    if (updatedSession.currentLevel > GameSettings.levelsUnlocked) {
                        GameSettings.levelsUnlocked = updatedSession.currentLevel.coerceAtMost(GameSession.MAX_CAMPAIGN_LEVELS)
                    }
                    sceneScope.launch {
                        if (updatedSession.isCampaignComplete()) {
                            sceneContainer.changeTo {
                                MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath)
                            }
                        } else {
                            sceneContainer.changeTo {
                                GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, updatedSession)
                            }
                        }
                    }
                }
                return@addUpdater
            }

            val dtSec = dt.inWholeMilliseconds / 1000f
            totalSeconds += dtSec

            // ── Debug overlays ───────────────────────────────────────────────
            if (GameDebugConfig.ENABLED) {
                if (ks.justPressed(Key.F1)) { debugMode = !debugMode; debugOverlay.visible = debugMode }
                if (debugMode) {
                    if (ks.justPressed(Key.K)) world.enemySystem.enemies.forEach { if (it.isActive) it.crash(totalSeconds) }
                    if (ks.justPressed(Key.G)) {
                        world.player1.ammo[GunType.MACHINEGUN.id] = 50
                        world.player1.ammo[GunType.SHOTGUN.id] = 25
                        world.player1.ammo[GunType.FLAMETHROWER.id] = 25
                        world.player1.ammo[GunType.GRENADE.id] = 5
                    }
                    if (ks.justPressed(Key.H)) {
                        world.player1.lifecounter -= 20
                        if (world.player1.lifecounter <= 0) {
                            world.player1.lives--; world.player1.lifecounter = 100; world.player1.destroy(totalSeconds)
                        }
                    }
                }
            }

            // ── Per-player weapon / movement / fire ──────────────────────────
            for ((idx, player) in world.players.withIndex()) {
                // Build GameInput for this player
                val pInput: GameInput = if (idx == 0) {
                    gameInput  // P1: keyboard / touch (built above)
                } else {
                    val gpIdx = idx - 1  // P2 = gamepads[0], P3 = gamepads[1], P4 = gamepads[2]
                    val gp = capturedViews.input.gamepads[gpIdx]
                    if (!gp.connected) GameInput.EMPTY
                    else {
                        val lx = gp.lx
                        val ly = gp.ly   // ly: -1=down, +1=up (XNA convention); negate for screen Y+down
                        val rx = gp.rx
                        val ry = gp.ry
                        val movLen = sqrt(lx * lx + ly * ly)
                        val movNormX = if (movLen > 0.1f) lx / movLen else 0f
                        val movNormY = if (movLen > 0.1f) -ly / movLen else 0f
                        val aimLen = sqrt(rx * rx + ry * ry)
                        val firing = aimLen > 0.3f
                        val startJP = gp.start && !prevGpStart[gpIdx]
                        val r1JP    = gp.r1    && !prevGpR1[gpIdx]
                        GameInput(
                            movement = Vec2(movNormX, movNormY),
                            aim = Vec2(rx, if (firing) -ry else 0f),
                            firing = firing,
                            pauseJustPressed = startJP,
                            confirmJustPressed = gp.south,
                            cancelJustPressed = gp.east,
                            weaponNextJustPressed = r1JP,
                        )
                    }
                }

                // Weapon cycle
                if (pInput.weaponNextJustPressed) player.cycleWeapon()

                // Movement
                val playerCanAct = player.status == ObjectStatus.ACTIVE || player.status == ObjectStatus.IMMUNE
                val mvx = pInput.movement.x
                val mvy = pInput.movement.y
                if (playerCanAct && (mvx != 0f || mvy != 0f)) {
                    lastMoveDxList[idx] = mvx; lastMoveDyList[idx] = mvy
                    lastFireAngles[idx] = FacingDirection.angleFrom(mvx, mvy)
                    val speed = player.pixelsPerSecond * dtSec
                    val delta = Point(mvx.toDouble() * speed, mvy.toDouble() * speed)
                    player.position = CollisionSystem.resolveMovement(player.position, delta, world.walls, world.furnitures)
                } else if (playerCanAct) {
                    lastMoveDxList[idx] = 0f; lastMoveDyList[idx] = 0f
                }

                // Fire
                if (player.status == ObjectStatus.ACTIVE && pInput.firing) {
                    if (pInput.aim.length() > VirtualThumbstickLogic.DEAD_ZONE / VirtualThumbstickLogic.maxRadius) {
                        lastFireAngles[idx] = FacingDirection.angleFrom(pInput.aim.x, pInput.aim.y)
                    }
                    CombatSystem.tryFire(player, world.combatStates[idx], totalSeconds, lastFireAngles[idx], shooterIndex = idx)
                }
            }

            // Update gamepad prev state (after consuming above)
            for (gpIdx in 0 until minOf(session.numPlayers - 1, 3)) {
                val gp = capturedViews.input.gamepads[gpIdx]
                prevGpStart[gpIdx] = gp.connected && gp.start
                prevGpR1[gpIdx]    = gp.connected && gp.r1
            }

            // Prune out-of-bounds projectiles for all players
            for (cs in world.combatStates) { CombatSystem.pruneOutOfBounds(cs, totalSeconds) }

            // Multi-player bullet/contact damage
            val kills = DamageSystem.processBulletCollisionsMulti(world.combatStates, world.enemySystem.enemies, world.players, totalSeconds)
            repeat(kills) {
                world.powerUpSystem.trySpawnOnKill(world.player1.position.x.toFloat(), world.player1.position.y.toFloat(), totalSeconds)
            }

            DamageSystem.processEnemyContactMulti(world.enemySystem.enemies, world.players, totalSeconds)
            world.powerUpSystem.update(totalSeconds, world.players)

            val waveSystem = world.waveSystem
            if (!waveSystem.isLevelComplete) {
                // Spawn next wave only when not in a transition pause
                if (!waveSystem.spawned && waveTransitionPhase == 0) {
                    val wave = waveSystem.currentWave
                    if (wave != null) {
                        val newEnemies = world.spawnSystem.spawnWave(
                            enemiesCount = wave.enemiesCount,
                            spawnZones = world.spawnZones,
                            furnitures = world.furnitures,
                            subLevelIndex = waveSystem.waveNumber - 1
                        )
                        world.enemySystem.addEnemies(newEnemies)
                        spawnEnemyViews(newEnemies)
                        waveSystem.markSpawned()
                    }
                }

                // Update per-player steering from current position/velocity
                for ((idx, se) in world.playerSteeringEntities.withIndex()) {
                    val p = world.players[idx]
                    se.position = Vec2(p.position.x.toFloat(), p.position.y.toFloat())
                    se.velocity = Vec2(
                        lastMoveDxList[idx] * AVATAR_PIXELS_PER_SECOND / 60f,
                        lastMoveDyList[idx] * AVATAR_PIXELS_PER_SECOND / 60f
                    )
                }
                world.enemySystem.update(dtSec, world.players, world.playerSteeringEntities, totalSeconds)

                if (waveSystem.spawned && world.enemySystem.activeCount == 0 && waveTransitionPhase == 0) {
                    waveSystem.advanceWave()
                    if (!waveSystem.isLevelComplete) {
                        // More waves ahead: show CLEARED → LEVEL/WAVE transition
                        waveTransitionPhase = 1
                        waveTransitionTimer = 0f
                        showTransitionPhase1()
                    }
                    // Level complete case: isStageClear handled below
                }

                // Wave transition timers
                if (waveTransitionPhase == 1) {
                    waveTransitionTimer += dtSec
                    if (waveTransitionTimer >= 2f) {
                        waveTransitionPhase = 2
                        waveTransitionTimer = 0f
                        showTransitionPhase2(levelNumber, waveSystem.waveNumber)
                    }
                } else if (waveTransitionPhase == 2) {
                    waveTransitionTimer += dtSec
                    if (waveTransitionTimer >= 2f) {
                        waveTransitionPhase = 0
                        hideTransition()
                    }
                }
            }

            world.players.forEach { it.update(totalSeconds) }

            // ── Game over check (all players must be eliminated) ──────────────
            val allDead = world.players.all { it.status == ObjectStatus.INACTIVE && it.lives <= 0 }
            if (allDead && !isGameOver) {
                isGameOver = true
                gameOverMenuIndex = 0
                gameOverOverlay.visible = true
                gameOverImgView?.visible = true
                gameOverEntryViews.forEach { it.visible = true }
                refreshGameOverColors()
            }

            // ── Stage cleared ─────────────────────────────────────────────────
            if (waveSystem.isLevelComplete && !isStageClear) {
                isStageClear = true
                stageClearedLabel.visible = true
            }

            // ── Score popup update (move upward, fade, remove old) ────────────
            val popupIter = scorePopups.iterator()
            while (popupIter.hasNext()) {
                val (tv, birth) = popupIter.next()
                val age = totalSeconds - birth
                if (age > 1.5f) { tv.removeFromParent(); popupIter.remove() }
                else { tv.y -= dtSec * 40.0; tv.alpha = 1.0 - (age / 1.5f).toDouble() }
            }

            // ── Enemy views sync ──────────────────────────────────────────────
            for ((enemy, view) in enemyViews) {
                val prev = prevEnemyStatuses[enemy]

                // Detect transition to DYING: swap to death animation + show score popup
                if (prev != ObjectStatus.DYING && enemy.status == ObjectStatus.DYING) {
                    val deathAnim = when (enemy.type) {
                        EnemyType.ZOMBIE   -> zombieDeathAnim
                        EnemyType.RAT      -> ratDeathAnim
                        EnemyType.WOLF     -> wolfDeathAnim
                        else               -> null
                    }
                    val sprite = enemyMainSprites[enemy]
                    if (deathAnim != null && sprite != null) {
                        sprite.stopAnimation()
                        sprite.playAnimation(deathAnim, (1.0 / ZOMBIE_DEATH_FPS).seconds)
                    }
                    // Floating score popup
                    val scoreVal = when (enemy.type) {
                        EnemyType.ZOMBIE   -> SCORE_ZOMBIE
                        EnemyType.RAT      -> SCORE_RAT
                        EnemyType.WOLF     -> SCORE_WOLF
                        EnemyType.MINOTAUR -> SCORE_MINOTAUR
                        else               -> 0
                    }
                    if (scoreVal > 0) {
                        val ex0 = enemy.entity.position.x.toDouble()
                        val ey0 = enemy.entity.position.y.toDouble() - 60.0
                        val tv = text("+$scoreVal") {
                            textSize = 20.0; color = Colors.YELLOW
                            x = ex0; y = ey0; zIndex = 950.0
                            font = ZombustersFonts.menuInfo
                        }
                        scorePopups.add(Pair(tv, totalSeconds))
                    }
                }

                prevEnemyStatuses[enemy] = enemy.status

                if (enemy.status == ObjectStatus.INACTIVE) { view.visible = false; continue }
                view.colorMul = Colors.WHITE; view.visible = true

                val ex = enemy.entity.position.x.toDouble()
                val ey = enemy.entity.position.y.toDouble()
                val yOffset = when (enemy.type) {
                    EnemyType.ZOMBIE -> 50.0
                    EnemyType.RAT    -> Rat.Y_OFFSET.toDouble()
                    EnemyType.WOLF   -> Wolf.Y_OFFSET.toDouble()
                    else             -> 50.0
                }
                // Flip compensation: scaleX=-1 pivots at x=0, shifting sprite left by frameWidth.
                val enemyFrameW = when (enemy.type) {
                    EnemyType.RAT      -> RAT_FRAME_W.toDouble()
                    EnemyType.WOLF     -> WOLF_FRAME_W.toDouble()
                    EnemyType.MINOTAUR -> 80.0
                    else               -> ZOMBIE_FRAME_W.toDouble()
                }
                val facingLeft = enemy.entity.velocity.x < 0
                view.scaleX = if (facingLeft) -1.0 else 1.0
                view.x = if (facingLeft) ex + enemyFrameW else ex
                view.y = ey - yOffset; view.zIndex = ey
            }

            // ── Player view sync (all players) ────────────────────────────────
            val blinkVisible = (totalSeconds * 10).toInt() % 2 == 0
            for ((idx, pc) in playerContainers.withIndex()) {
                val p = world.players[idx]
                val isDead = p.status == ObjectStatus.DYING || p.status == ObjectStatus.INACTIVE
                val diedView = playerDiedViews.getOrNull(idx)
                if (isDead && diedView != null) {
                    // Freeze the died image at the death position (only move it once on first DYING frame)
                    if (!diedView.visible) {
                        diedView.x = p.position.x + PLAYER_RENDER_OFFSET_X
                        diedView.y = p.position.y + PLAYER_RENDER_OFFSET_Y
                        diedView.visible = true
                    }
                } else if (!isDead) {
                    diedView?.visible = false
                }
                pc.visible = when (p.status) {
                    ObjectStatus.IMMUNE   -> blinkVisible
                    ObjectStatus.DYING    -> false
                    ObjectStatus.INACTIVE -> false
                    else                  -> true
                }
                pc.x = p.position.x + PLAYER_RENDER_OFFSET_X
                pc.y = p.position.y + PLAYER_RENDER_OFFSET_Y
                pc.zIndex = p.position.y

                // ── Legs animation (idle vs run) ──────────────────────────────
                val isMoving = lastMoveDxList[idx] != 0f || lastMoveDyList[idx] != 0f
                if (isMoving != playerLastMoving[idx]) {
                    playerLastMoving[idx] = isMoving
                    playerLegsIdleViews.getOrNull(idx)?.visible  = !isMoving
                    val runS = playerLegsRunSprites.getOrNull(idx)
                    if (runS != null) {
                        runS.visible = isMoving
                        if (isMoving) {
                            val movingLeft = lastMoveDxList[idx] < 0f
                            runS.x = if (movingLeft) RUN_LOCAL_X_W else RUN_LOCAL_X_E
                            runS.scaleX = if (movingLeft) -1.0 else 1.0
                        }
                    }
                } else if (isMoving) {
                    // Update flip direction while moving
                    val runS = playerLegsRunSprites.getOrNull(idx)
                    if (runS != null) {
                        val movingLeft = lastMoveDxList[idx] < 0f
                        val expectedScaleX = if (movingLeft) -1.0 else 1.0
                        if (runS.scaleX != expectedScaleX) {
                            runS.x = if (movingLeft) RUN_LOCAL_X_W else RUN_LOCAL_X_E
                            runS.scaleX = expectedScaleX
                        }
                    }
                }

                // ── Directional trunk animation swap ─────────────────────────
                val pInput0 = if (idx == 0) gameInput else GameInput.EMPTY
                val isFiring = pInput0.firing && p.status == ObjectStatus.ACTIVE
                val facing   = FacingDirection.classify(lastFireAngles[idx])
                val trunkS   = playerTrunkSprites.getOrNull(idx) ?: continue
                if (isFiring != playerLastFiring[idx] || facing != playerLastFacing[idx]) {
                    playerLastFiring[idx] = isFiring
                    playerLastFacing[idx] = facing
                    if (!isFiring || idleAnim == null) {
                        // Idle trunk
                        trunkS.x = 0.0; trunkS.y = 0.0; trunkS.scaleX = 1.0
                        if (idleAnim != null) trunkS.playAnimationLooped(idleAnim, (1.0 / JADE_IDLE_FPS).seconds)
                    } else {
                        // Directional shot animation — pick sheet + local offset + flip for W-facing dirs
                        // For flipped (scaleX=-1) sprites: x_flipped = x_normal + spriteWidth so
                        // the sprite visually stays in the same position (KorGE pivots at x=0).
                        data class TrunkAnim(val anim: SpriteAnimation?, val lx: Double, val ly: Double, val flip: Boolean)
                        val ta: TrunkAnim = when (facing) {
                            FacingDirection.N  -> TrunkAnim(shotNAnim,  JADE_TRUNK_SHOT_LX,                    JADE_TRUNK_N_LY,  false)
                            FacingDirection.NE -> TrunkAnim(shotNEAnim, JADE_TRUNK_SHOT_LX,                    JADE_TRUNK_NE_LY, false)
                            FacingDirection.E  -> TrunkAnim(shotEAnim,  JADE_TRUNK_SHOT_LX,                    JADE_TRUNK_E_LY,  false)
                            FacingDirection.SE -> TrunkAnim(shotSEAnim, JADE_TRUNK_SHOT_LX,                    JADE_TRUNK_SE_LY, false)
                            FacingDirection.S  -> TrunkAnim(shotSAnim,  JADE_TRUNK_SHOT_LX,                    JADE_TRUNK_S_LY,  false)
                            FacingDirection.SW -> TrunkAnim(shotSEAnim, JADE_TRUNK_SHOT_LX + JADE_SHOT_SE_W,   JADE_TRUNK_SE_LY, true)
                            FacingDirection.W  -> TrunkAnim(shotEAnim,  JADE_TRUNK_SHOT_LX + JADE_SHOT_E_W,    JADE_TRUNK_E_LY,  true)
                            FacingDirection.NW -> TrunkAnim(shotNEAnim, JADE_TRUNK_SHOT_LX + JADE_SHOT_NE_W,   JADE_TRUNK_NE_LY, true)
                        }
                        if (ta.anim != null) {
                            trunkS.x = ta.lx; trunkS.y = ta.ly
                            trunkS.scaleX = if (ta.flip) -1.0 else 1.0
                            trunkS.playAnimationLooped(ta.anim, (1.0 / JADE_SHOT_FPS).seconds)
                        } else if (idleAnim != null) {
                            trunkS.x = 0.0; trunkS.y = 0.0; trunkS.scaleX = 1.0
                            trunkS.playAnimationLooped(idleAnim, (1.0 / JADE_IDLE_FPS).seconds)
                        }
                    }
                }
            }

            // ── Bullet rendering (all players' projectiles) ───────────────────
            bulletGraphics.updateShape {
                for (cs in world.combatStates) {
                    for (bullet in cs.bullets) {
                        val (bx, by) = bullet.positionAt(totalSeconds)
                        fill(Colors.YELLOW) { circle(Point(bx.toDouble(), by.toDouble()), 3.0) }
                    }
                    for (shell in cs.shotgunShells) {
                        for (pi in 0..2) {
                            val (px, py) = shell.pelletPositionAt(pi, totalSeconds)
                            fill(Colors.ORANGE) { circle(Point(px.toDouble(), py.toDouble()), 2.5) }
                        }
                    }
                }
            }

            // ── Touch overlay (virtual sticks + pause icon) ───────────────────
            touchOverlayGraphics.updateShape {
                if (currentInputMode == InputMode.TOUCH && !isPaused && !isGameOver) {
                    val maxR = VirtualThumbstickLogic.maxRadius.toDouble()
                    val knobR = 18.0

                    // Left stick
                    if (dualStick.left.active) {
                        val cx = dualStick.left.center.x.toDouble()
                        val cy = dualStick.left.center.y.toDouble()
                        stroke(RGBA(200, 200, 200, 120), lineWidth = 2.0) { circle(Point(cx, cy), maxR) }
                        val disp = dualStick.left.clampedDisplacement
                        fill(RGBA(255, 255, 255, 180)) { circle(Point(cx + disp.x, cy + disp.y), knobR) }
                    }

                    // Right stick
                    if (dualStick.right.active) {
                        val cx = dualStick.right.center.x.toDouble()
                        val cy = dualStick.right.center.y.toDouble()
                        stroke(RGBA(200, 200, 200, 120), lineWidth = 2.0) { circle(Point(cx, cy), maxR) }
                        val disp = dualStick.right.clampedDisplacement
                        val knobColor = if (dualStick.isFiring) RGBA(255, 80, 80, 200) else RGBA(255, 255, 255, 180)
                        fill(knobColor) { circle(Point(cx + disp.x, cy + disp.y), knobR) }
                    }

                    // Pause icon placeholder (legacy: UI/pause_iconWP, 1088-1135, 39-83)
                    fill(RGBA(80, 80, 80, 160)) {
                        rect(PAUSE_ICON_X1.toDouble(), PAUSE_ICON_Y1.toDouble(),
                            (PAUSE_ICON_X2 - PAUSE_ICON_X1).toDouble(),
                            (PAUSE_ICON_Y2 - PAUSE_ICON_Y1).toDouble())
                    }
                    // Two pause bars
                    fill(RGBA(240, 240, 240, 220)) {
                        rect(1096.0, 46.0, 10.0, 31.0)
                        rect(1114.0, 46.0, 10.0, 31.0)
                    }
                }
            }

            // ── HUD update ────────────────────────────────────────────────────
            val p1 = world.player1
            val ammoLabel = when (p1.currentGun) {
                GunType.PISTOL       -> "- - -"
                GunType.SHOTGUN      -> p1.ammo[GunType.SHOTGUN.id].toString().padStart(3, '0')
                GunType.MACHINEGUN   -> p1.ammo[GunType.MACHINEGUN.id].toString().padStart(3, '0')
                GunType.FLAMETHROWER -> p1.ammo[GunType.FLAMETHROWER.id].toString().padStart(3, '0')
                GunType.GRENADE      -> p1.ammo[GunType.GRENADE.id].toString().padStart(3, '0')
            }
            hudHealth.text  = p1.lifecounter.toString().padStart(3, '0')
            hudHealth.color = when {
                p1.lifecounter > 60 -> Colors.LIME
                p1.lifecounter > 30 -> Colors.YELLOW
                else -> Colors.RED
            }
            hudLives.text   = "x${p1.lives}"
            hudScore.text   = "SC${p1.score.toString().padStart(7, '0')}"
            hudAmmo.text    = ammoLabel
            hudWave.text    = if (waveSystem.isLevelComplete) "CLEARED" else "Wave ${waveSystem.waveNumber}/${waveSystem.totalWaves}"
            hudLevel.text   = "LEVEL $levelNumber"
            hudEnemies.text = "Enemies: ${world.enemySystem.activeCount}"
        }

        // ── Touch handlers for game-over menu entries ────────────────────────
        // Each entry gets a direct onClick AND the overlay gets a tap-anywhere fallback.
        gameOverEntryViews.forEachIndexed { i, view ->
            view.onClick {
                if (!gameOverConfirmed && gameOverOverlay.visible) {
                    gameOverMenuIndex = i
                    refreshGameOverColors()
                    gameOverConfirmed = true
                    sceneScope.launch {
                        when (i) {
                            0 -> sceneContainer.changeTo { GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session) }
                            1 -> sceneContainer.changeTo { GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, GameSession.newGame(session.characterIndex)) }
                            2 -> sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                        }
                    }
                }
            }
        }
        // Overlay tap-anywhere: confirms the currently highlighted entry
        gameOverOverlay.onClick {
            if (!gameOverConfirmed && gameOverOverlay.visible) {
                gameOverConfirmed = true
                sceneScope.launch {
                    when (gameOverMenuIndex) {
                        0 -> sceneContainer.changeTo { GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session) }
                        1 -> sceneContainer.changeTo { GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, GameSession.newGame(session.characterIndex)) }
                        2 -> sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                    }
                }
            }
        }

        // ── Touch handlers for pause menu entries ────────────────────────────
        pauseEntryViews.forEachIndexed { i, view ->
            view.onClick {
                if (!pauseConfirmed && pauseOverlay.visible) {
                    pauseMenuIndex = i
                    refreshPauseColors()
                    when (i) {
                        0 -> {
                            isPaused = false; pauseConfirmed = false
                            dualStick.resetAll()
                            pauseOverlay.visible = false; pauseLabel.visible = false
                            pauseEntryViews.forEach { it.visible = false }
                        }
                        else -> {
                            pauseConfirmed = true
                            sceneScope.launch {
                                when (i) {
                                    1 -> sceneContainer.changeTo { HowToPlayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                                    2 -> sceneContainer.changeTo { OptionsScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                                    3 -> sceneContainer.changeTo { GameplayScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath, session) }
                                    4 -> sceneContainer.changeTo { MenuScene(exit, drawableResourcesPath, fontResourcesPath, filesResourcesPath) }
                                }
                            }
                        }
                    }
                }
            }
        }

        for ((idx, pc) in playerContainers.withIndex()) {
            pc.x = world.players[idx].position.x + PLAYER_RENDER_OFFSET_X
            pc.y = world.players[idx].position.y + PLAYER_RENDER_OFFSET_Y
        }
    }

    private suspend fun tryLoadBitmap(path: String): Bitmap? = try { resourcesVfs[path].readBitmap() } catch (_: Throwable) { null }

    private data class FurnitureBitmaps(val main: Bitmap?, val shadow: Bitmap?, val shadowOffX: Double, val shadowOffY: Double)

    private suspend fun loadFurnitureBitmaps(): Map<Pair<FurnitureType, FurnitureOrientation?>, FurnitureBitmaps> {
        val base = "$assetBase/furniture"
        suspend fun load(name: String) = tryLoadBitmap("$base/$name")
        return mapOf(
            Pair(FurnitureType.ARBOL, null) to FurnitureBitmaps(
                load("arbol.png"), load("arbol_shadow.png"), 45.0, 138.0),
            Pair(FurnitureType.BASURA, null) to FurnitureBitmaps(
                load("basura.png"), load("basura_shadow.png"), 2.0, 26.0),
            Pair(FurnitureType.COCHE_ARDIENDO, null) to FurnitureBitmaps(
                load("coche_ardiendo.png"), null, 0.0, 0.0),
            Pair(FurnitureType.PUENTE, null) to FurnitureBitmaps(
                load("puente.png"), null, 0.0, 0.0),
            Pair(FurnitureType.BANCO, FurnitureOrientation.SOUTH_EAST) to FurnitureBitmaps(
                load("banco_se.png"), load("banco_se_shadow.png"), 6.0, 17.0),
            Pair(FurnitureType.BANCO, FurnitureOrientation.SOUTH_WEST) to FurnitureBitmaps(
                load("banco_sw.png"), load("banco_sw_shadow.png"), 3.0, 18.0),
            Pair(FurnitureType.BANCO, FurnitureOrientation.NORTH_EAST) to FurnitureBitmaps(
                load("banco_se.png"), load("banco_se_shadow.png"), 0.0, 0.0),
            Pair(FurnitureType.BANCO, FurnitureOrientation.NORTH_WEST) to FurnitureBitmaps(
                load("banco_sw.png"), load("banco_sw_shadow.png"), 0.0, 0.0),
            Pair(FurnitureType.FAROLA, FurnitureOrientation.NORTH_EAST) to FurnitureBitmaps(
                load("farola_ne.png"), load("farola_ne_shadow.png"), 2.0, 146.0),
            Pair(FurnitureType.FAROLA, FurnitureOrientation.NORTH_WEST) to FurnitureBitmaps(
                load("farola_nw.png"), load("farola_nw_shadow.png"), 44.0, 146.0),
            Pair(FurnitureType.FAROLA, FurnitureOrientation.SOUTH_EAST) to FurnitureBitmaps(
                load("farola_se.png"), load("farola_se_shadow.png"), 2.0, 122.0),
            Pair(FurnitureType.FAROLA, FurnitureOrientation.SOUTH_WEST) to FurnitureBitmaps(
                load("farola_sw.png"), load("farola_sw_shadow.png"), 44.0, 122.0),
        )
    }

    private fun SContainer.buildFurnitureViews(furnitures: List<Furniture>, bitmaps: Map<Pair<FurnitureType, FurnitureOrientation?>, FurnitureBitmaps>) {
        for (f in furnitures) {
            val entry = bitmaps[Pair(f.type, f.orientation)] ?: bitmaps[Pair(f.type, null)] ?: continue
            val bmp = entry.main ?: continue
            val fx = f.positionX.toDouble()
            val fy = f.positionY.toDouble()
            val zBase = fy + bmp.height

            // Shadow drawn first (behind main sprite)
            if (entry.shadow != null) {
                image(entry.shadow) {
                    x = fx + entry.shadowOffX; y = fy + entry.shadowOffY
                    alpha = 60.0 / 255.0; zIndex = zBase - 0.1; smoothing = false
                }
            }

            // Main sprite — animated for COCHE_ARDIENDO, static for all others
            if (f.type == FurnitureType.COCHE_ARDIENDO) {
                val anim = SpriteAnimation(bmp, spriteWidth = 99, spriteHeight = 60, columns = 3, rows = 1)
                sprite(anim) {
                    x = fx; y = fy; zIndex = zBase; smoothing = false
                }.playAnimationLooped(anim, (1.0 / 10).seconds)
            } else {
                image(bmp) {
                    x = fx; y = fy; zIndex = zBase; smoothing = false
                }
            }
        }
    }

    override fun onSizeChanged(size: Size) = super.onSizeChanged(size)
}
