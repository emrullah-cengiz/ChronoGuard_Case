using UnityEngine;
using UnityEngine.AI;
 
public class CharacterAnimatorController : MonoBehaviour
{
    protected readonly int MOVE_SPEED_PARAM = Animator.StringToHash("MoveSpeed");
    protected readonly int DEAD_PARAM = Animator.StringToHash("IsDead");
    protected readonly int IS_AIMING_PARAM = Animator.StringToHash("IsAiming");
    
    protected float _targetMovementSpeed;
    protected float _currentMovementSpeed;

    [SerializeField] protected float _speedSmoothingPower = 3f;

    [SerializeField] protected NavMeshAgent _agent;
    [SerializeField] protected Animator _animator;

    protected virtual void Update()
    {
        _targetMovementSpeed = _agent.velocity.magnitude / _agent.speed;

        _currentMovementSpeed = Mathf.Lerp(_currentMovementSpeed, _targetMovementSpeed, _speedSmoothingPower * Time.deltaTime);

       _animator.SetFloat(MOVE_SPEED_PARAM, _currentMovementSpeed);
    }


    public void SetDead(bool s) => _animator.SetBool(DEAD_PARAM, s);
    public void SetAim(bool s) => _animator.SetFloat(IS_AIMING_PARAM, s ? 1 : 0);
}
