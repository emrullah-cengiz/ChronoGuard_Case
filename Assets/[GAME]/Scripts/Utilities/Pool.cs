using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;


public interface IPoolableInitializationData { }

public interface IInitializablePoolable<in TInitializationData> where TInitializationData : IPoolableInitializationData
{
    public void OnSpawned(TInitializationData data);
}

public interface IPool<TObject, in TEnum> where TObject : Component
                                       where TEnum : Enum
{
    public TObject Spawn(TEnum type);
    void Despawn(TObject obj, TEnum type);
}

public abstract class Pool<TEnum> : Pool<Transform, TEnum> where TEnum : Enum
{
    protected Pool(PoolSettings poolSettings) : base(poolSettings)
    {
    }
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

        Initialize();
    }

    private void Initialize()
    {
        foreach (var kv in _poolSettings.Properties)
            FillThePool(kv.Key, kv.Value);
    }

    public TObject Spawn(TEnum type)
    {
        if (!_pools[type].TryPop(out var obj))
            obj = Create(type);

        obj.gameObject.SetActive(true);

        return obj;
    }

    public TObject Spawn<TInitializationData>(TEnum type, TInitializationData data) where TInitializationData : IPoolableInitializationData
    {
        try
        {
            var obj = Spawn(type);

            ((IInitializablePoolable<TInitializationData>)obj).OnSpawned(data);

            return obj;
        }
        catch (InvalidCastException)
        {
            Debug.LogError($"{typeof(TObject).Name} is not {typeof(IInitializablePoolable<TInitializationData>).Name}!");

            throw;
        }
    }

    public void Despawn(TObject obj, TEnum type)
    {
        obj.gameObject.SetActive(false);

        _pools[type].Push(obj);
    }

    private void FillThePool(TEnum type, PoolProperties properties)
    {
        Stack<TObject> pool = new();

        for (var i = 0; i < properties.InitialSize; i++)
            pool.Push(Create(properties.Prefab));
        
        _pools.Add(type, pool);
    }
    
    
    private TObject Create(TEnum type)
    {
        Assert.IsTrue(_poolSettings.Properties.TryGetValue(type, out var properties), 
                $"PoolProperties for {type} not found!");

        return Create(properties!.Prefab);
    }

    private TObject Create(TObject prefab)
    {
        var obj = Object.Instantiate(prefab);
        obj.gameObject.SetActive(false);

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
        public int InitialSize;
        //..
    }
}

//public interface ISimplePool<TObject> where TObject : MonoBehaviour
//{
//    TObject Spawn();
//    void Despawn(TObject obj);
//}

//public abstract class SimplePool<TObject> : ISimplePool<TObject> where TObject : MonoBehaviour
//{
//    private PoolSettings _poolSettings;
//    private Stack<TObject> _pool;
//    private TObject _prefab;

//    protected SimplePool(TObject prefab, int initialSize)
//    {
//        _prefab = prefab;
//        _poolSettings = new PoolSettings { InitialSize = initialSize };
//        _pool = new Stack<TObject>();

//        Initialize();
//    }

//    private void Initialize()
//    {
//        for (int i = 0; i < _poolSettings.InitialSize; i++)
//        {
//            _pool.Push(Create());
//        }
//    }

//    public TObject Spawn()
//    {
//        if (_pool.Count > 0)
//        {
//            var obj = _pool.Pop();
//            obj.gameObject.SetActive(true);
//            return obj;
//        }
//        else
//        {
//            return Create();
//        }
//    }

//    public void Despawn(TObject obj)
//    {
//        obj.gameObject.SetActive(false);
//        _pool.Push(obj);
//    }

//    private TObject Create()
//    {
//        var obj = GameObject.Instantiate(_prefab);
//        obj.gameObject.SetActive(false);
//        return obj;
//    }

//    class PoolSettings
//    {
//        public int InitialSize { get; set; } = 0;
//    }
//}
