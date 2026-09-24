package com.retrowax.zombusters.game.persistence

import kotlin.test.BeforeTest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class LocalHighScoresTest {
    @BeforeTest
    fun setUp() { LocalHighScores.clear() }

    @Test
    fun empty_scores_list() {
        assertEquals(emptyList(), LocalHighScores.load())
    }

    @Test
    fun submit_single_score() {
        val scores = LocalHighScores.submit("ACE", 1000)
        assertEquals(1, scores.size)
        assertEquals(HighScoreEntry("ACE", 1000), scores[0])
    }

    @Test
    fun scores_sorted_descending() {
        LocalHighScores.submit("LOW", 500)
        LocalHighScores.submit("HIGH", 2000)
        val scores = LocalHighScores.submit("MID", 1000)
        assertEquals(listOf(2000, 1000, 500), scores.map { it.score })
    }

    @Test
    fun capped_at_max_entries() {
        repeat(12) { i -> LocalHighScores.submit("P$i", i * 100) }
        assertEquals(10, LocalHighScores.load().size)
    }

    @Test
    fun is_high_score_when_empty() {
        assertTrue(LocalHighScores.isHighScore(1))
    }

    @Test
    fun not_high_score_when_table_full_and_lower() {
        repeat(10) { i -> LocalHighScores.submit("P$i", (i + 1) * 1000) }
        assertFalse(LocalHighScores.isHighScore(100))
    }

    @Test
    fun is_high_score_when_table_full_and_higher() {
        repeat(10) { i -> LocalHighScores.submit("P$i", (i + 1) * 100) }
        assertTrue(LocalHighScores.isHighScore(9999))
    }

    @Test
    fun name_truncated_to_10_chars() {
        val scores = LocalHighScores.submit("TOOLONGNAME123", 500)
        assertEquals(10, scores[0].name.length)
    }

    @Test
    fun scores_persist_across_load_calls() {
        LocalHighScores.submit("PER", 750)
        val loaded = LocalHighScores.load()
        assertEquals(1, loaded.size)
        assertEquals(HighScoreEntry("PER", 750), loaded[0])
    }

    @Test
    fun clear_removes_all_scores() {
        LocalHighScores.submit("CLR", 999)
        LocalHighScores.clear()
        assertEquals(emptyList(), LocalHighScores.load())
    }
}
