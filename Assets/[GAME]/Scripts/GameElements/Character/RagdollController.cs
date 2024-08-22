using Sirenix.OdinInspector;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody[] _ragdollRigidbodies;
    private Collider[] _ragdollColliders;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = GetComponentsInChildren<Collider>();

        SetRagdollState(false);
    }

    
    [Button]
    public void SetRagdollState(bool isRagdoll)
    {
        _animator.enabled = !isRagdoll;

        foreach (var rb in _ragdollRigidbodies)
            rb.isKinematic = !isRagdoll;
        foreach (var c in _ragdollColliders)
            c.enabled = isRagdoll;
    }
}