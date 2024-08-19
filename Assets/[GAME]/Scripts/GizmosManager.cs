using System;
using System.Collections.Generic;
using UnityEngine;

public class GizmosManager : MonoBehaviour
{
    // private static GizmosManager _instance;
    //
    // public static GizmosManager Instance
    // {
    //     get
    //     {
    //         if (_instance == null)
    //         {
    //             var obj = new GameObject().AddComponent<GizmosManager>();
    //             _instance = obj;
    //         }
    //         return _instance;
    //     }
    // }
    
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
}
