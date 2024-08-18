

using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 SetYZero(this Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }
}