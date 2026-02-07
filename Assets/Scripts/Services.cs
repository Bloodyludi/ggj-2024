using System;
using System.Collections.Generic;
using UnityEngine;

public static class Services
{
    private static readonly Dictionary<Type, MonoBehaviour> registry = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        registry.Clear();
    }

    public static void Register<T>(T instance) where T : MonoBehaviour
    {
        registry[typeof(T)] = instance;
    }

    public static T Get<T>() where T : MonoBehaviour
    {
        if (registry.TryGetValue(typeof(T), out var instance) && instance != null)
            return (T)instance;

        var found = UnityEngine.Object.FindFirstObjectByType<T>();
        if (found != null)
        {
            registry[typeof(T)] = found;
            return found;
        }

        throw new InvalidOperationException($"Service {typeof(T).Name} not registered and not found in scene.");
    }
}
