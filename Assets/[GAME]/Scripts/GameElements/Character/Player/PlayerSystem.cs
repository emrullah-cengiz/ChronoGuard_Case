using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSystem : MonoBehaviour
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
   
   public Vector3 GetPlayerPosition() => transform.position;
   public Vector3 GetPlayerLookingDirection() => transform.forward;
}
