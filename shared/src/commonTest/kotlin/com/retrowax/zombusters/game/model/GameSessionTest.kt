package com.retrowax.zombusters.game.model

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class GameSessionTest {

    @Test
    fun newGame_defaultsToLevel1CharTracy() {
        val s = GameSession.newGame()
        assertEquals(1, s.currentLevel)
        assertEquals(0, s.characterIndex)
        assertEquals("Tracy", s.characterName)
    }

    @Test
    fun characterNameMatchesLegacyAvatarNameList() {
        assertEquals("Tracy",   GameSession.CHARACTER_NAMES[0])
        assertEquals("Charles", GameSession.CHARACTER_NAMES[1])
        assertEquals("Ryan",    GameSession.CHARACTER_NAMES[2])
        assertEquals("Peter",   GameSession.CHARACTER_NAMES[3])
    }

    @Test
    fun withNextLevel_advancesCurrentLevel() {
        val s = GameSession.newGame().withNextLevel()
        assertEquals(2, s.currentLevel)
    }

    @Test
    fun withNextLevel_exceedsMaxToSignalCampaignComplete() {
        val s = GameSession.newGame().copy(currentLevel = GameSession.MAX_CAMPAIGN_LEVELS)
        val next = s.withNextLevel()
        assertEquals(GameSession.MAX_CAMPAIGN_LEVELS + 1, next.currentLevel)
        assertTrue(next.isCampaignComplete())
    }

    @Test
    fun withNextLevel_updatesLevelsUnlocked() {
        val s = GameSession.newGame().copy(currentLevel = 3, levelsUnlocked = 3)
        val next = s.withNextLevel()
        assertEquals(4, next.levelsUnlocked)
    }

    @Test
    fun withNextLevel_doesNotDecreaseAlreadyHighLevelsUnlocked() {
        val s = GameSession.newGame().copy(currentLevel = 2, levelsUnlocked = 9)
        val next = s.withNextLevel()
        assertEquals(9, next.levelsUnlocked)
    }

    @Test
    fun isCampaignComplete_falseForLevels1to10() {
        for (level in 1..10) {
            val s = GameSession.newGame().copy(currentLevel = level)
            assertFalse(s.isCampaignComplete(), "level $level should not be complete")
        }
    }

    @Test
    fun isCampaignComplete_trueAfterLevel10() {
        val s = GameSession.newGame().copy(currentLevel = GameSession.MAX_CAMPAIGN_LEVELS)
            .withNextLevel()
        assertTrue(s.isCampaignComplete())
    }

    @Test
    fun withScore_accumulatesScore() {
        val s = GameSession.newGame().copy(score = 100).withScore(50)
        assertEquals(150, s.score)
    }

    @Test
    fun withLives_setsLives() {
        val s = GameSession.newGame().withLives(1)
        assertEquals(1, s.lives)
    }

    @Test
    fun characterSpritePath_alwaysReturnsJadeForAllIndices() {
        // Only Tracy (index 0) has sprites extracted; others fall back to jade
        for (i in GameSession.CHARACTER_NAMES.indices) {
            val s = GameSession.newGame(i)
            assertEquals("jade", s.characterSpritePath, "index $i should use jade path until sprites extracted")
        }
    }

    @Test
    fun availableCharacterIndices_onlyContainsZero() {
        assertEquals(setOf(0), GameSession.AVAILABLE_CHARACTER_INDICES)
    }

    @Test
    fun newGame_clampsCharacterIndexToValidRange() {
        val s = GameSession.newGame(99)
        assertEquals(3, s.characterIndex)  // clamped to max index
    }

    @Test
    fun maxCampaignLevels_is10() {
        assertEquals(10, GameSession.MAX_CAMPAIGN_LEVELS)
    }

    @Test
    fun debugLevelJump_defaultsFalse() {
        assertFalse(GameSession.newGame().debugLevelJump)
    }

    @Test
    fun levelRange_1to10AllLoadable() {
        // Verifies that GameSession correctly models levels 1-10 without overflow
        val sessions = (1..10).map { level ->
            GameSession.newGame().copy(currentLevel = level)
        }
        assertEquals(10, sessions.size)
        assertEquals(1, sessions.first().currentLevel)
        assertEquals(10, sessions.last().currentLevel)
    }
}
