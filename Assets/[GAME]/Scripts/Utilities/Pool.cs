using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public interface IPoolableInitializationData
{
}

public interface IInitializablePoolable
{
    public void OnCreated();
}

public interface IInitializablePoolable<in TInitializationData> : IInitializablePoolable where TInitializationData : IPoolableInitializationData
{
    public void OnSpawned(TInitializationData data);
}

public interface IPool
{
    void Initialize();
}

public interface IPool<TObject, in TEnum> : IPool where TObject : Component where TEnum : Enum
{
    public TObject Spawn(TEnum type);
    void Despawn(TObject obj, TEnum type);
}

public abstract class Pool<TObject, TEnum> : IPool<TObject, TEnum> where TObject : Component
    where TEnum : Enum
{
    private readonly PoolSettings _poolSettings;

    private readonly Dictionary<TEnum, Stack<TObject>> _pools;

    protected Pool(PoolSettings poolSettings)
    {
        _poolSettings = poolSettings;
        _pools = new();
    }

    public void Initialize()
    {
        foreach (var kv in _poolSettings.Properties)
            CreatePool(kv.Key, kv.Value);
    }

    private void CreatePool(TEnum type, PoolProperties properties)
    {
        Stack<TObject> pool = new();

        if (properties.FillOnInit)
            ExpandPool(pool, properties);

        _pools.Add(type, pool);
    }

    public TObject Spawn(TEnum type)
    {
        var pool = _pools[type];

        if (!pool.TryPop(out var obj))
        {
            ExpandPool(type);
            obj = pool.Pop();
        }

        obj.gameObject.SetActive(true);

        return obj;
    }

    public TObject Spawn<TInitializationData>(TEnum type, TInitializationData data) where TInitializationData : IPoolableInitializationData
    {
        var obj = Spawn(type);

        if (obj is IInitializablePoolable<TInitializationData> initializable)
            initializable.OnSpawned(data);
        else
            Debug.LogError($"{typeof(TObject).Name} is not {typeof(IInitializablePoolable<TInitializationData>).Name}!");

        return obj;
    }

    public void Despawn(TObject obj, TEnum type)
    {
        obj.gameObject.SetActive(false);

        _pools[type].Push(obj);
    }

    private void ExpandPool(TEnum type)
    {
        var pool = _pools[type];
        var properties = _poolSettings.Properties[type];

        ExpandPool(pool, properties);
    }

    private void ExpandPool(Stack<TObject> pool, PoolProperties properties)
    {
        for (var i = 0; i < properties.ExpansionSize; i++)
            pool.Push(Create(properties.Prefab));
    }

    // private TObject Create(TEnum type)
    // {
    //     Assert.IsTrue(_poolSettings.Properties.TryGetValue(type, out var properties),
    //         $"PoolProperties for {type} not found!");
    //
    //     return Create(properties!.Prefab);
    // }

    private TObject Create(TObject prefab)
    {
        var obj = Object.Instantiate(prefab);
        obj.gameObject.SetActive(false);

        if (obj is IInitializablePoolable initializable)
            initializable.OnCreated();

        return obj;
    }

    [Serializable]
    public class PoolSettings
    {
        public Dictionary<TEnum, PoolProperties> Properties = new();
    }

    [Serializable]
    public class PoolProperties
    {
        public TObject Prefab;

        public bool FillOnInit = true;

        [MinValue(1)] public int ExpansionSize = 1;
        //..
    }
}

public abstract class Pool<TEnum> : Pool<Transform, TEnum> where TEnum : Enum
{
    protected Pool(PoolSettings poolSettings) : base(poolSettings)
    {
    }
}
