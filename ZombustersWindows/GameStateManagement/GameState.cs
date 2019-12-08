namespace GameStateManagement
{
    public enum GameState 
    { 
        SignIn, FindSession, CreateSession, Start, InGame, GameOver, InLobby, SelectPlayer, WaitingToBegin, Paused 
    }

    public enum GameplayState
    {
        Playing, StageCleared, Pause, StartLevel, GameOver, NotPlaying
    }
}