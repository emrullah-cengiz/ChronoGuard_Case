using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DataSaveService<T> where T : class, new()
{
    private bool _autoSave;
    private readonly string _saveFilePath;
    private readonly int _saveTimerInterval;

    private CancellationTokenSource _cts;

    // ReSharper disable once MemberCanBePrivate.Global
    public T Data { get; private set; }

    private bool _isSaveTimerActive;

    public DataSaveService(bool autoSave, int saveInterval, string saveFilePath)
    {
        _autoSave = autoSave;
        _saveTimerInterval = saveInterval;
        _saveFilePath = saveFilePath;

        _cts = new CancellationTokenSource();
    }

    public void Initialize(out bool isDataFound)
    {
        Data = new T();

        LoadData(out isDataFound);

        if (_autoSave)
            StartAutoSaveLoop().Forget();
    }

    public void SetData(T saveData) => Data = saveData;

    private void LoadData(out bool isDataFound)
    {
        try
        {
            var json = File.ReadAllText(_saveFilePath);
            Data = JsonUtility.FromJson<T>(json);
            isDataFound = true;
            
            if (Data == null)
            {
                Data ??= new T();
                isDataFound = false;
            }
        }
        catch (FileNotFoundException)
        {
            Data = new T();
            isDataFound = false;
        }
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(_saveFilePath, json);
    }

    public void StartAutoSave()
    {
        _autoSave = true;
        StartAutoSaveLoop().Forget();
    }

    public void StopAutoSave()
    {
        _autoSave = false;
        _cts.Cancel();
    }

    private async UniTaskVoid StartAutoSaveLoop()
    {
        while (_autoSave)
        {
            await UniTask.WaitForSeconds(_saveTimerInterval, cancellationToken: _cts.Token);
            Save();
        }
    }
}