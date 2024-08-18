using System;
using System.Collections.Generic;

public class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T instance) where T : class
    {
        if (_services.ContainsKey(typeof(T)))
            throw new Exception($"Service of type {typeof(T).Name} is already registered.");

        _services[typeof(T)] = instance;
    }

    public static T Resolve<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var instance))
            return (T)instance;

        throw new Exception($"No singleton service of type {typeof(T).Name} is registered.");
    }
}