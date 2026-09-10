package com.steelvectors.app.feature.game.base

import com.steelvectors.app.feature.game.entities.Bullet
import com.steelvectors.app.feature.game.entities.Enemy
import com.steelvectors.app.feature.game.entities.EnemyType
import com.steelvectors.app.feature.game.entities.enemies.Assault
import com.steelvectors.app.feature.game.entities.enemies.Bomber
import com.steelvectors.app.feature.game.entities.enemies.Boss
import com.steelvectors.app.feature.game.entities.enemies.BossType
import com.steelvectors.app.feature.game.entities.enemies.Fighter
import com.steelvectors.app.feature.game.entities.enemies.Gunship
import com.steelvectors.app.feature.game.entities.enemies.Interdictor
import com.steelvectors.app.feature.game.entities.enemies.Recon
import korlibs.image.bitmap.Bitmap
import korlibs.korge.view.Container
import korlibs.math.geom.Point
import kotlin.random.Random

private const val BOSS_LEVEL_FREQUENCY = 10
private const val RIGHT_OFFSET = 270
private const val LEFT_OFFSET = 0
private const val UP_OFFSET = 2
private const val DOWN_OFFSET = 150
private const val ENEMY_Y_OFFSET = 50

class LevelManager(
    private val bossBitmapList: List<Bitmap>,
    private val enemiesBitmapList: List<Bitmap>,
    private val bulletBitmap: Bitmap,
    private val bullets: MutableList<Bullet>,
    private val container: Container,
    //private val soundPool: SoundPool?,
    private val phaserSound: Int?,
    private val screenWidth: Float,
    private val screenHeight: Float
) {
    var currentLevel = FIRST_LEVEL

    fun levelFinished() {
        currentLevel++
    }

    fun restart() {
        currentLevel = FIRST_LEVEL
    }

    fun getLevelEnemies(): MutableList<Enemy> {
        val enemies = mutableListOf<Enemy>()
        enemies.addBoss()
        if (currentLevel.rem(BOSS_LEVEL_FREQUENCY) != 0) {
            val strength = getStrengthForTheLevel()
            val enemiesForTheRound = getNumberOfEnemiesDepnendingTheCurrentLevel()
            var verticalGap = -5
            for (enemyGroup in enemiesForTheRound) {
                for (i in 0 until enemyGroup.first) {
                    when (enemyGroup.second) {
                        EnemyType.GUNSHIP -> verticalGap -= enemies.addGunships(
                            strength,
                            verticalGap
                        )

                        EnemyType.ASSAULT -> verticalGap -= enemies.addAssaults(
                            strength,
                            verticalGap
                        )

                        EnemyType.BOMBER -> verticalGap -= enemies.addBombers(strength, verticalGap)
                        EnemyType.FIGHTER -> verticalGap -= enemies.addFighters(
                            strength,
                            verticalGap
                        )

                        EnemyType.RECON -> enemies.addRecons(strength)
                        EnemyType.INTERDICTOR -> enemies.addInterdictors(strength)
                        else -> {
                        }
                    }
                }
            }
        }
        return enemies
    }

    private fun getNumberOfEnemiesDepnendingTheCurrentLevel(): List<Pair<Int, EnemyType>> {
        val enemiesForTheRound = mutableListOf<Pair<Int, EnemyType>>()
        return when (currentLevel) {
            in 1..9 -> {
                enemiesForTheRound.add(Pair(1, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(1, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(2, EnemyType.BOMBER))
                enemiesForTheRound
            }

            in 11..19 -> {
                enemiesForTheRound.add(Pair(1, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(1, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(1, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(2, EnemyType.BOMBER))
                enemiesForTheRound
            }

            in 21..29 -> {
                enemiesForTheRound.add(Pair(1, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(1, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(3, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(1, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(1, EnemyType.RECON))
                enemiesForTheRound
            }

            in 31..39 -> {
                enemiesForTheRound.add(Pair(2, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(2, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(3, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(1, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(1, EnemyType.RECON))
                enemiesForTheRound.add(Pair(1, EnemyType.INTERDICTOR))
                enemiesForTheRound
            }

            in 41..49 -> {
                enemiesForTheRound.add(Pair(3, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(3, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(4, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(3, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(3, EnemyType.RECON))
                enemiesForTheRound.add(Pair(3, EnemyType.INTERDICTOR))
                enemiesForTheRound
            }

            in 51..59 -> {
                enemiesForTheRound.add(Pair(4, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(4, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(5, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(4, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(4, EnemyType.RECON))
                enemiesForTheRound.add(Pair(4, EnemyType.INTERDICTOR))
                enemiesForTheRound
            }

            in 61..69 -> {
                enemiesForTheRound.add(Pair(5, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(6, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(6, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(5, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(5, EnemyType.RECON))
                enemiesForTheRound.add(Pair(5, EnemyType.INTERDICTOR))
                enemiesForTheRound
            }

            in 71..79 -> {
                enemiesForTheRound.add(Pair(6, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(7, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(7, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(7, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(6, EnemyType.RECON))
                enemiesForTheRound.add(Pair(8, EnemyType.INTERDICTOR))
                enemiesForTheRound
            }

            in 81..89 -> {
                enemiesForTheRound.add(Pair(7, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(7, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(7, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(7, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(7, EnemyType.RECON))
                enemiesForTheRound.add(Pair(7, EnemyType.INTERDICTOR))
                enemiesForTheRound
            }

            in 91..99 -> {
                enemiesForTheRound.add(Pair(8, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(8, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(8, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(8, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(8, EnemyType.RECON))
                enemiesForTheRound.add(Pair(8, EnemyType.INTERDICTOR))
                enemiesForTheRound
            }

            in 101..109 -> {
                enemiesForTheRound.add(Pair(9, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(9, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(9, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(9, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(9, EnemyType.RECON))
                enemiesForTheRound.add(Pair(9, EnemyType.INTERDICTOR))
                enemiesForTheRound
            }

            else -> {
                enemiesForTheRound.add(Pair(10, EnemyType.GUNSHIP))
                enemiesForTheRound.add(Pair(10, EnemyType.ASSAULT))
                enemiesForTheRound.add(Pair(10, EnemyType.BOMBER))
                enemiesForTheRound.add(Pair(10, EnemyType.FIGHTER))
                enemiesForTheRound.add(Pair(10, EnemyType.RECON))
                enemiesForTheRound.add(Pair(10, EnemyType.INTERDICTOR))
                enemiesForTheRound
            }
        }
    }

    private fun getStrengthForTheLevel(): Int {
        return when {
            currentLevel > 20 -> 2
            currentLevel > 40 -> 3
            currentLevel > 50 -> 4
            currentLevel > 100 -> 5
            currentLevel > 150 -> 8
            else -> {
                1
            }
        }
    }

    private fun getBoss(): Boss? {
        return when {
            currentLevel.rem(BOSS_LEVEL_FREQUENCY) == 0 &&
                    currentLevel.toString().startsWith("1") -> getBossWithType(BossType.A)

            currentLevel.rem(BOSS_LEVEL_FREQUENCY) == 0 &&
                    currentLevel.toString().startsWith("2") -> getBossWithType(BossType.B)

            currentLevel.rem(BOSS_LEVEL_FREQUENCY) == 0 &&
                    currentLevel.toString().startsWith("3") -> getBossWithType(BossType.C)

            currentLevel.rem(BOSS_LEVEL_FREQUENCY) == 0 &&
                    currentLevel.toString().startsWith("4") -> getBossWithType(BossType.D)

            currentLevel.rem(BOSS_LEVEL_FREQUENCY) == 0 &&
                    currentLevel.toString().startsWith("5") -> getBossWithType(BossType.E)

            currentLevel.rem(BOSS_LEVEL_FREQUENCY) == 0 &&
                    currentLevel.toString().startsWith("6") -> getBossWithType(BossType.F)

            currentLevel.rem(BOSS_LEVEL_FREQUENCY) == 0 &&
                    currentLevel.toString().startsWith("7") -> getBossWithType(BossType.G)

            currentLevel.rem(BOSS_LEVEL_FREQUENCY) == 0 &&
                    currentLevel.toString().startsWith("8") -> getBossWithType(BossType.F)

            currentLevel.rem(BOSS_LEVEL_FREQUENCY) == 0 &&
                    currentLevel.toString().startsWith("9") -> getBossWithType(BossType.E)

            else -> {
                null
            }
        }
    }

    private fun getBossWithType(bossType: BossType): Boss {
        return Boss(
            bossType,
            Point(100, 40),
            bossBitmapList[bossType.ordinal],
            bulletBitmap,
            bullets,
            container,
            //soundPool,
            phaserSound,
            screenWidth,
            screenHeight
        )
    }

    private fun MutableList<Enemy>.addBoss() {
        val boss = getBoss()
        boss?.let {
            this.add(it)
            container.addChild(it)
        }
    }

    private fun MutableList<Enemy>.addGunships(strength: Int, verticalGap: Int): Int {
        // Squads of 2 of quick ships
        val xPosition =
            Random.nextInt(LEFT_OFFSET, screenWidth.toInt() - Gunship.SPRITE_WIDTH.toInt())
        var gap = 0
        for (i in 0..1) {
            gap += ENEMY_Y_OFFSET * i
            val position = Point(xPosition, verticalGap - ENEMY_Y_OFFSET * i)
            val enemy = Gunship(position, enemiesBitmapList[0], screenWidth, screenHeight)
            enemy.strength = strength
            this.add(enemy)
            container.addChild(enemy)
        }
        return gap
    }

    private fun MutableList<Enemy>.addAssaults(strength: Int, verticalGap: Int): Int {
        // Squads of 4 that cross the screen
        val xPosition =
            Random.nextInt(LEFT_OFFSET, screenWidth.toInt() - Assault.SPRITE_WIDTH.toInt())
        var gap = 0
        for (i in 0..3) {
            gap += ENEMY_Y_OFFSET * i
            val position = Point(xPosition - (ENEMY_Y_OFFSET * i), verticalGap - ENEMY_Y_OFFSET * i)
            val enemy = Assault(position, enemiesBitmapList[1], screenWidth, screenHeight)
            enemy.strength = strength
            this.add(enemy)
            container.addChild(enemy)
        }
        return gap
    }

    private fun MutableList<Enemy>.addBombers(strength: Int, verticalGap: Int): Int {
        // Squads of 3 flying in formation
        val xPosition =
            Random.nextInt(LEFT_OFFSET, screenWidth.toInt() - Bomber.SPRITE_WIDTH.toInt())
        var gap = 0
        for (i in 0..2) {
            gap += ENEMY_Y_OFFSET * i
            val position = if (xPosition > 150) {
                Point(xPosition - (ENEMY_Y_OFFSET * i), verticalGap - ENEMY_Y_OFFSET * i)
            } else {
                Point(xPosition + (ENEMY_Y_OFFSET * i), verticalGap - ENEMY_Y_OFFSET * i)
            }
            val enemy = Bomber(position, enemiesBitmapList[2], screenWidth, screenHeight)
            enemy.strength = strength
            this.add(enemy)
            container.addChild(enemy)
        }
        return gap
    }

    private fun MutableList<Enemy>.addFighters(strength: Int, verticalGap: Int): Int {
        // Squads of 3 flying in formation and shooting
        val xPosition =
            Random.nextInt(LEFT_OFFSET, screenWidth.toInt() - Fighter.SPRITE_WIDTH.toInt())
        var gap = 0
        for (i in 0..2) {
            gap += ENEMY_Y_OFFSET * i
            val position = if (xPosition > 150) {
                Point(xPosition - (ENEMY_Y_OFFSET * i), verticalGap - ENEMY_Y_OFFSET * i)
            } else {
                Point(xPosition + (ENEMY_Y_OFFSET * i), verticalGap - ENEMY_Y_OFFSET * i)
            }
            val enemy = Fighter(
                position,
                enemiesBitmapList[3],
                bulletBitmap,
                bullets,
                container,
                phaserSound,
                screenWidth,
                screenHeight
            )
            enemy.strength = strength
            this.add(enemy)
            container.addChild(enemy)
        }
        return gap
    }

    private fun MutableList<Enemy>.addRecons(strength: Int) {
        // Squads of 3 crossing screen horizontally
        val yPosition = Random.nextInt(UP_OFFSET, DOWN_OFFSET)
        for (i in 0..2) {
            val position = Point(-30 - (ENEMY_Y_OFFSET * i), yPosition)
            val enemy = Recon(position, enemiesBitmapList[4], screenWidth, screenHeight)
            enemy.strength = strength
            this.add(enemy)
            container.addChild(enemy)
        }
    }

    private fun MutableList<Enemy>.addInterdictors(strength: Int) {
        // Squads of 2 crossing screen horizontally and shooting
        val yPosition = Random.nextInt(UP_OFFSET, DOWN_OFFSET)
        for (i in 0..1) {
            val position = Point(-30 - (ENEMY_Y_OFFSET * i), yPosition)
            val enemy = Interdictor(
                position,
                enemiesBitmapList[5],
                bulletBitmap,
                bullets,
                container,
                phaserSound,
                screenWidth,
                screenHeight
            )
            enemy.strength = strength
            this.add(enemy)
            container.addChild(enemy)
        }
    }

    companion object {
        const val FIRST_LEVEL = 1
    }
}
