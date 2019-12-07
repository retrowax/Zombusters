//using Steamworks;

namespace ZombustersWindows
{
    // Represents different types of network messages
    public enum MessageType {
        StartGame = 0,
        EndGame = 1,
        RestartGame = 2,
        RejoinLobby = 3,
        UpdatePlayerPos = 4,
        SelectPlayer = 5,
        ChangeLevel = 6,
        ToggleReady = 7,
        ChangeCharacter = 8,
        SendPlayerSlot = 9,
        ReadyToBegin = 10,
        BeginToPlay = 11,
        GameplayStatePlaying = 12,
        GameplayStateStageCleared = 13,
        GameplayStatePause = 14,
        GameplayStateStartLevel = 15,
        GameplayStateGameOver = 16,
        GameplayPosition = 17,
        GameplayAddBullet = 18,
        GameplayPlayerCrashed = 19,
        GameplayPlayerLifecounter = 20,
        ZombieEnemyPosition = 21,
        TankEnemyPosition = 22,
        ZombieDestroyed = 23,
        TankDestroyed = 24,
        PowerUpAdded = 25,
        PowerUpPicked = 26
    }
}
