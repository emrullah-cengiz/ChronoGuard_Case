using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [SerializeField] private InputActionReference _movementInputActionRef;
    [SerializeField] private NavMeshAgent _agent;

    private PlayerSystem _playerSystem;

    private InputAction _inputAction;
    private bool _isActive;
    private Vector2 _input;
    private float _angularSpeed;
    private InputActionPhase _inputPhase;

    private void Awake()
    {
        _inputAction = _movementInputActionRef.action;
        _angularSpeed = _agent.angularSpeed;
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
    }

    private void OnEnable()
    {
        Events.Player.OnLockedTarget += ActivateAgentRotation;
        Events.GameStates.OnLevelStarted += SetNewSpeed;
    }

    private void OnDisable()
    {
        Events.Player.OnLockedTarget -= ActivateAgentRotation;
        Events.GameStates.OnLevelStarted -= SetNewSpeed;
    }

    public void Activate(bool s) => _isActive = s;

    private void Update()
    {
        if(!_isActive) return;

        switch (_inputAction.phase)
        {
            case InputActionPhase.Performed or InputActionPhase.Started:
                Move(_inputAction.ReadValue<Vector2>());
                break;
            case InputActionPhase.Canceled:
                Move(Vector2.zero);
                break;
        }
    }

    private void Move(Vector2 input)
    {
        Vector3 movementVector = new(input.x, 0, input.y);
        _agent.velocity = _agent.speed * movementVector;
    }
    
    private void ActivateAgentRotation(bool s) => _agent.angularSpeed = s ? 0 : _angularSpeed;
    
    private void SetNewSpeed() => _agent.speed = _playerSystem.Properties.Speed;
}