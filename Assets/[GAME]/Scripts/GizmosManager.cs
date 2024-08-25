using System;
using System.Collections.Generic;
using UnityEngine;

public class GizmosManager : MonoBehaviour
{
    static List<Action> gizmosDraws = new List<Action>();
    
    public static void AddDrawAction(Action action) => gizmosDraws.Add(action);
    public static void ClearGizmos() => gizmosDraws.Clear();
    
    private void OnDrawGizmos()
    {
        foreach (var action in gizmosDraws)
        {
            action.Invoke();
        }
    }

    private void OnDisable()
    {
        ClearGizmos();
    }

}
