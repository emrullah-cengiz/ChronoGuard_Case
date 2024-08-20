using Sirenix.OdinInspector;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody[] _ragdollRigidbodies;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();

        SetRagdollState(false);
    }

    
    [Button]
    public void SetRagdollState(bool isRagdoll)
    {
        _animator.enabled = !isRagdoll;

        foreach (var rb in _ragdollRigidbodies)
            rb.isKinematic = !isRagdoll;
    }
}