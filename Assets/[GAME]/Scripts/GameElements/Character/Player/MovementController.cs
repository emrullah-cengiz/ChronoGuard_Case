using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [SerializeField] private InputActionReference _movementInputActionRef;
    [SerializeField] private NavMeshAgent _agent;

    private PlayerSystem _playerSystem;

    private bool _isActive;
    private Vector2 _input;
    private float _angularSpeed;
    private InputActionPhase _inputPhase;

    private void Awake()
    {
        _angularSpeed = _agent.angularSpeed;
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
    }

    private void OnEnable()
    {
        Events.Player.OnLockedTarget += ActivateAgentRotation;
        Events.GameStates.OnLevelStarted += SetNewSpeed;
        _movementInputActionRef.action.performed += SetInput;
        _movementInputActionRef.action.canceled += ClearInput;
    }

    private void OnDisable()
    {
        Events.Player.OnLockedTarget -= ActivateAgentRotation;
        Events.GameStates.OnLevelStarted -= SetNewSpeed;
        _movementInputActionRef.action.performed -= SetInput;
        _movementInputActionRef.action.canceled -= ClearInput;
    }

    public void Activate(bool s) => _isActive = s;

    private void Update()
    {
        if(!_isActive) return;
        
        if (_inputPhase is InputActionPhase.Performed or InputActionPhase.Canceled)
            Move();
    }

    private void Move()
    {
        Vector3 movementVector = new(_input.x, 0, _input.y);
        _agent.velocity = _agent.speed * movementVector;
    }
    
    private void ActivateAgentRotation(bool s) => _agent.angularSpeed = s ? 0 : _angularSpeed;

    private void SetInput(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _inputPhase = context.phase;
    }
    private void ClearInput(InputAction.CallbackContext context)
    {
        _input = Vector3.zero;
        _inputPhase = context.phase;
    }
    
    private void SetNewSpeed() => _agent.speed = _playerSystem.Properties.Speed;
}