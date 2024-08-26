using Cysharp.Threading.Tasks;
using UnityEngine;

public static class Extensions
{
    public static string GetTimeFormatInSeconds(this int seconds)
    {
        // ReSharper disable once UseStringInterpolation
        return string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
    }

    public static async UniTask<T> LoadResource<T>(string resourcePath) where T : UnityEngine.Object
    {
        var operation = Resources.LoadAsync<T>(resourcePath);

        await operation;

        return operation.asset as T;
    }
}