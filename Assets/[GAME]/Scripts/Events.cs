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
        public static Action OnDamageTake = delegate { };
        public static Action OnWeaponFire = delegate { };
    }
    
    public static class Weapon
    {
        public static Action<Bullet> OnBulletHit = delegate { };
    }
    
    public static class Enemies
    {
        public static Action<Enemy> OnEnemyDead = delegate { };
    }
    
    public static class Level
    {
        public static Action<int> OnLevelTimerTick = delegate { };
    }
}