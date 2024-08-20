using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSystem : Character, IDamagable
{
   [SerializeField] private PlayerProperties _properties;
   
   [SerializeField] private PlayerStateController _playerStateController;
   [SerializeField] private Weapon _weapon;
   [SerializeField] private Health _health;
   
   private void OnEnable()
   {
      Events.GameStates.OnLevelStarted += Initialize;
   }
   private void OnDisable()
   {
      Events.GameStates.OnLevelStarted -= Initialize;
   }
   
   private void Initialize()
   {
      Debug.Log("Initializing Player..");
      _playerStateController.Initialize();
      _health.Initialize(_properties.MaxHealth);
   }

   public void TakeDamage(int damage)
   {
      _health.TakeDamage(damage);
      
      Events.Player.OnDamageTake?.Invoke();
   }
}
