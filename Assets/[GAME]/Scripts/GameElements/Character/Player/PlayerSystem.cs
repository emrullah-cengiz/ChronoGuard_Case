using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSystem : Character
{
   [SerializeField] private PlayerStateController _playerStateController;
   [SerializeField] private Weapon _weapon;
   
   
   private void OnEnable()
   {
      Events.GameStates.OnGameStarted += Initialize;
   }

   private void OnDisable()
   {
      Events.GameStates.OnGameStarted -= Initialize;
   }
   
   private void Initialize()
   {
      _playerStateController.Initialize();
   }
   
}
