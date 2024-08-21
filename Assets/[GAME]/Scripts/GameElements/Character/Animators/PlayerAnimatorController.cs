using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimatorController : CharacterAnimatorController
{
    private readonly int MOVE_DIRECTION_PARAM = Animator.StringToHash("Direction");
    
    [SerializeField] protected float _walkDirectionSmoothingPower = 7f;
    private float _targetDirection;
    private float _currentDirection;

    protected override void Update()
    {
        base.Update();

        _targetDirection = Vector3.Dot(transform.forward, _agent.velocity.normalized);
        _currentDirection = Mathf.Lerp(_currentDirection, _targetDirection, _walkDirectionSmoothingPower * Time.deltaTime);
        
        _animator.SetFloat(MOVE_DIRECTION_PARAM, _currentDirection);
    }
}