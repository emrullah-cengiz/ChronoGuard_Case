using System;

public static class Events
{
    public static Action OnApplicationPause = delegate { };
    public static Action OnApplicationQuit = delegate { };
    
    public static class GameStates
    {
        public static Action OnGameStarted = delegate { };
        public static Action OnLevelStarted = delegate { };
        public static Action OnLevelEnd = delegate { };
    }   
}