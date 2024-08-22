﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [SerializeField] private InputActionReference _movementInputActionRef;
    [SerializeField] private NavMeshAgent _agent;

    [SerializeField] private PlayerProperties _playerProperties;

    private Vector2 _input;
    private float _angularSpeed;

    private void Awake()
    {
        _angularSpeed = _agent.angularSpeed;
        _agent.speed = _playerProperties.Speed;
    }

    private void OnEnable()
    {
        Debug.Log("MovementController");
        Events.GameStates.OnGameStarted += Initialize;
        Events.Player.OnLockedTarget += ActivateAgentRotation;
    }

    private void OnDisable()
    {
        Events.GameStates.OnGameStarted -= Initialize;
        Events.Player.OnLockedTarget -= ActivateAgentRotation;
    }

    private void ActivateAgentRotation(bool s)
    {
        _agent.angularSpeed = s ? 0 : _angularSpeed;
    }

    private void Initialize()
    {
        _playerProperties = ServiceLocator.Resolve<PlayerProperties>();
    }

    private void Update()
    {
        if (_movementInputActionRef.action.phase == InputActionPhase.Started)
            // _input = Vector3.left;
            Move();
    }

    private void Move()
    {
        _input = _movementInputActionRef.action.ReadValue<Vector2>();

        Vector3 movementVector = new(_input.x, 0, _input.y);

        //_playerProperties.BaseSpeed *
        _agent.velocity = _agent.speed * movementVector;
    }
}