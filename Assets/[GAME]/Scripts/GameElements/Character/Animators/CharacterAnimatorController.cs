using System;
using UnityEngine;
using UnityEngine.AI;
 
public class CharacterAnimatorController : MonoBehaviour
{
    private readonly int MOVE_DIRECTION_PARAM = Animator.StringToHash("Direction");
    protected readonly int MOVE_SPEED_PARAM = Animator.StringToHash("MoveSpeed");
    protected readonly int DEAD_PARAM = Animator.StringToHash("IsDead");
    protected readonly int IS_AIMING_PARAM = Animator.StringToHash("IsAiming");
    
    [SerializeField] protected float _speedSmoothingPower = 3f;
    protected float _targetMovementSpeed;
    protected float _currentMovementSpeed;
    protected float _maxMoveSpeed;
    
    [SerializeField] protected float _walkDirectionSmoothingPower = 7f;
    private float _targetDirection;
    private float _currentDirection;

    [SerializeField] protected NavMeshAgent _agent;
    [SerializeField] protected Animator _animator;

    private void Awake() => _Awake();

    protected virtual void _Awake()
    {
    }

    public void Initialize(float maxMoveSpeed)
    {
        _maxMoveSpeed = maxMoveSpeed;   
    }
    
    protected virtual void Update()
    {
        SetMovementSpeed();

        SetDirection();
    }

    private void SetDirection()
    {
        _targetDirection = Vector3.Dot(transform.forward, _agent.velocity.normalized);
        _currentDirection = Mathf.Lerp(_currentDirection, _targetDirection, _walkDirectionSmoothingPower * Time.deltaTime);
        
        _animator.SetFloat(MOVE_DIRECTION_PARAM, _currentDirection);
    }

    private void SetMovementSpeed()
    {
        _targetMovementSpeed = _agent.velocity.magnitude / _maxMoveSpeed;

        _currentMovementSpeed = Mathf.Lerp(_currentMovementSpeed, _targetMovementSpeed, _speedSmoothingPower * Time.deltaTime);

        _animator.SetFloat(MOVE_SPEED_PARAM, _currentMovementSpeed);
    }


    public void SetDead(bool s) => _animator.SetBool(DEAD_PARAM, s);
    public void SetAim(bool s) => _animator.SetFloat(IS_AIMING_PARAM, s ? 1 : 0);
}
