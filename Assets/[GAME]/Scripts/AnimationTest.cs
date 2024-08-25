using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class AnimationTest : MonoBehaviour
{
    public GameObject _ref;
    NavMeshAgent _agent;
    EnemyAnimatorController _animator;
    
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<EnemyAnimatorController>();
    }

    [Button]
    public void Send()
    {
        _animator.Initialize(_agent.speed);

        _agent.SetDestination(_ref.transform.position);
    }
}