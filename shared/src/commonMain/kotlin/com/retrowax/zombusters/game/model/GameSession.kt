package com.retrowax.zombusters.game.model

/**
 * Domain model representing a single-player campaign session.
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
    val currentLevel: Int = 1,
    val score: Int = 0,
    val lives: Int = AVATAR_LIVES,
    val levelsUnlocked: Int = 1,
    /** true when used only for level-jumping in debug mode */
    val debugLevelJump: Boolean = false
) {

    val characterName: String
        get() = CHARACTER_NAMES.getOrElse(characterIndex) { "Tracy" }

    /** Asset sprite folder for this character. Only index 0 (Tracy/jade) has extracted sprites. */
    val characterSpritePath: String
        get() = when (characterIndex) {
            0 -> "jade"
            else -> "jade"   // fallback — sprites for chars 1-3 not yet extracted
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

        fun newGame(characterIndex: Int = 0): GameSession = GameSession(
            characterIndex = characterIndex.coerceIn(0, CHARACTER_NAMES.size - 1),
            currentLevel = 1,
            score = 0,
            lives = AVATAR_LIVES,
            levelsUnlocked = 1
        )
    }
}
