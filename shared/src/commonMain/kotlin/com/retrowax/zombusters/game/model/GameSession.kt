package com.retrowax.zombusters.game.model

/**
 * Domain model representing a campaign session (single or multi-player).
 * Immutable — produce new copies with withXxx() helpers.
 * Lives outside KorGE views; pass as constructor arg to scenes.
 *
 * Character names from legacy SelectPlayerScreen.cs avatarNameList:
 *   index 0 → Tracy (sprites in jade/ folder — migration naming artifact)
 *   index 1 → Charles
 *   index 2 → Ryan
 *   index 3 → Peter
 */
data class GameSession(
    val characterIndex: Int = 0,
    /** Per-player character indices (multi-player). P1 = characterIndices[0]. */
    val characterIndices: List<Int> = listOf(characterIndex),
    val currentLevel: Int = 1,
    val score: Int = 0,
    val lives: Int = AVATAR_LIVES,
    val levelsUnlocked: Int = 1,
    /** true when used only for level-jumping in debug mode */
    val debugLevelJump: Boolean = false
) {

    /** Number of human players in this session (1-4). */
    val numPlayers: Int get() = characterIndices.size.coerceIn(1, MAX_PLAYERS)

    val characterName: String
        get() = CHARACTER_NAMES.getOrElse(characterIndex) { "Tracy" }

    /** Asset sprite folder for P1. Only index 0 (Tracy/jade) has extracted sprites. */
    val characterSpritePath: String
        get() = characterSpritePathFor(0)

    /** Asset sprite folder for player at given index. Falls back to jade until sprites extracted. */
    fun characterSpritePathFor(idx: Int): String = when (characterIndices.getOrElse(idx) { 0 }) {
        0 -> "jade"
        else -> "jade"
    }

    /** Whether this character index has extracted gameplay sprites. */
    val characterSpritesAvailable: Boolean
        get() = characterIndex == 0

    fun withNextLevel(): GameSession {
        val next = currentLevel + 1  // may exceed MAX_CAMPAIGN_LEVELS to signal isCampaignComplete()
        return copy(
            currentLevel = next,
            levelsUnlocked = maxOf(levelsUnlocked, next.coerceAtMost(MAX_CAMPAIGN_LEVELS))
        )
    }

    fun withScore(added: Int): GameSession = copy(score = score + added)

    fun withLives(newLives: Int): GameSession = copy(lives = newLives)

    fun isCampaignComplete(): Boolean = currentLevel > MAX_CAMPAIGN_LEVELS

    companion object {
        const val MAX_CAMPAIGN_LEVELS = 10

        /** From legacy SelectPlayerScreen.cs avatarNameList */
        val CHARACTER_NAMES = listOf("Tracy", "Charles", "Ryan", "Peter")

        /** Character indices that have extracted gameplay sprites (Step 6). */
        val AVAILABLE_CHARACTER_INDICES = setOf(0)

        fun newGame(characterIndex: Int = 0): GameSession {
            val ci = characterIndex.coerceIn(0, CHARACTER_NAMES.size - 1)
            return GameSession(
                characterIndex = ci,
                characterIndices = listOf(ci),
                currentLevel = 1,
                score = 0,
                lives = AVATAR_LIVES,
                levelsUnlocked = 1
            )
        }

        /** Factory for multi-player sessions from SelectPlayerScene joining flow. */
        fun newMultiGame(
            characterIndices: List<Int>,
            currentLevel: Int = 1,
            levelsUnlocked: Int = 1
        ): GameSession {
            val clamped = characterIndices
                .map { it.coerceIn(0, CHARACTER_NAMES.size - 1) }
                .ifEmpty { listOf(0) }
            return GameSession(
                characterIndex = clamped[0],
                characterIndices = clamped,
                currentLevel = currentLevel,
                score = 0,
                lives = AVATAR_LIVES,
                levelsUnlocked = levelsUnlocked
            )
        }
    }
}
