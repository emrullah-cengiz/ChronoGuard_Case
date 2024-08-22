using System;

public static class Events
{
    public static Action OnApplicationPause = delegate { };
    public static Action OnApplicationQuit = delegate { };
    
    public static class GameStates
    {
        public static Action OnGameStarted = delegate { };
        public static Action OnLevelStarted = delegate { };
        public static Action<bool> OnLevelEnd = delegate { };
    }

    public static class Player
    {
        public static Action<int, int> OnDamageTake = delegate { };
        public static Action OnPlayerDead = delegate { };
        public static Action<bool> OnLockedTarget = delegate { };
    }
    
    public static class Enemies
    {
        public static Action<Enemy> OnEnemyDead = delegate { };
        public static Action OnWaveSpawned = delegate { };
    }
    
    public static class Level
    {
        public static Action<int> OnLevelCountdownTick = delegate { };
        public static Action OnLevelCountdownEnd = delegate { };
    }
    
    public static class UI
    {
        public static Action<UIPanelType> OnPanelActionButtonClick = delegate { };
    }
}