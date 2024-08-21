using Cysharp.Threading.Tasks;
using UnityEngine;

public static class Extensions
{
    public static string GetTimeFormatInSeconds(this int seconds)
    {
        return $"{seconds / 60:00}:{seconds % 60:0}";
    }
    
    public static async UniTask<T> LoadResource<T>(string resourcePath) where T : UnityEngine.Object
    {
        var operation = Resources.LoadAsync<T>(resourcePath);

        await operation;

        return operation.asset as T;
    }

}