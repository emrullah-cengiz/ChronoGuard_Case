using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public void OnDeSpawned();
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
    // private readonly Transform _parent;

    private readonly Dictionary<TEnum, (Stack<TObject>, Transform)> _pools;

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
        var poolTuple = (new Stack<TObject>(), CreateParent($"Pool_{typeof(TObject).Name}_{type}"));

        if (properties.FillOnInit)
            ExpandPool(poolTuple, properties);

        _pools.Add(type, poolTuple);
    }

    public TObject Spawn(TEnum type)
    {
        var pool = _pools[type];

        if (!pool.Item1.TryPop(out var obj))
        {
            ExpandPool(type);
            obj = pool.Item1.Pop();
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

        _pools[type].Item1.Push(obj);

        if (obj is IInitializablePoolable initializable)
            initializable.OnDeSpawned();
    }

    public void DespawnAll()
    {
        foreach (var poolKV in _pools)
        {
            var components = poolKV.Value.Item2.GetComponentsInChildren<TObject>().ToList();

            for (var i = 0; i < components.Count; i++)
            {
                var obj = components[i];
                if (obj.gameObject.activeSelf)
                    Despawn(obj, poolKV.Key);
            }
        }
    }

    private void ExpandPool(TEnum type)
    {
        var poolTuple = _pools[type];
        var properties = _poolSettings.Properties[type];

        ExpandPool(poolTuple, properties);
    }

    private void ExpandPool((Stack<TObject> pool, Transform parent) poolTuple, PoolProperties properties)
    {
        for (var i = 0; i < properties.ExpansionSize; i++)
            poolTuple.pool.Push(Create(properties.Prefab, poolTuple.parent));
    }

    // private TObject Create(TEnum type)
    // {
    //     Assert.IsTrue(_poolSettings.Properties.TryGetValue(type, out var properties),
    //         $"PoolProperties for {type} not found!");
    //
    //     return Create(properties!.Prefab);
    // }

    private TObject Create(TObject prefab, Transform parent)
    {
        var obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);

        if (obj is IInitializablePoolable initializable)
            initializable.OnCreated();

        return obj;
    }

    private Transform CreateParent(string name)
    {
        var go = new GameObject(name);
        return go.transform;
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
    protected Pool(PoolSettings poolSettings, bool createParent = true) : base(poolSettings)
    {
    }
}