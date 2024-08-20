using UnityEngine;

public abstract class TransformObject : MonoBehaviour
{
    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    public Quaternion Rotation
    {
        get => transform.rotation;
        set => transform.rotation = value;
    }

    public Vector3 Forward
    {
        get => transform.forward;
        set => transform.forward = value;
    }
}
