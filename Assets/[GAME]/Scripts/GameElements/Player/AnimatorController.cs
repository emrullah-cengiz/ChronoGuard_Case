using System.Collections;
using UnityEngine;
using UnityEngine.AI;
 

public class AnimatorController : MonoBehaviour
{
    private readonly int MOVE_SPEED_PARAM = Animator.StringToHash("MoveSpeed");

    private float _targetMovementSpeed;
    private float _currentMovementSpeed;

    [SerializeField] private float _speedSmoothingPower = 3f;

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;

    private void Update()
    {
        _targetMovementSpeed = _agent.velocity.magnitude / _agent.speed;

        _currentMovementSpeed = Mathf.Lerp(_currentMovementSpeed, _targetMovementSpeed, _speedSmoothingPower * Time.deltaTime);

        _animator.SetFloat(MOVE_SPEED_PARAM, _currentMovementSpeed);
    }

}
