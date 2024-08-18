using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [SerializeField] private InputActionReference _movementInputActionRef;
    [SerializeField] private NavMeshAgent _agent;
    
    [SerializeField] private PlayerSettings _playerSettings;

    private Vector2 _input;

    private void Awake()
    {
        _playerSettings = ServiceLocator.Resolve<PlayerSettings>();
    }

    private void Update()
    {
        if (_movementInputActionRef.action.phase == InputActionPhase.Started) 
            Move();
    }

    private void Move()
    {
        _input = _movementInputActionRef.action.ReadValue<Vector2>();

        Vector3 movementVector = new(_input.x, 0, _input.y);

        _agent.velocity = _playerSettings.BaseSpeed * movementVector;
    }
}
