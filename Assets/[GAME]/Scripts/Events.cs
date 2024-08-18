using System;

public static class Events
{
    public static class GameStates
    {
        public static Action OnGameStarted = delegate { };
        public static Action OnLevelSpawned = delegate { };
    }   
}