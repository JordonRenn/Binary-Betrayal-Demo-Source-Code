using UnityEngine;

public static class SingletonExtensions
{
    public static T InitializeSingleton<T>(this T instance, ref T staticInstance, bool persistAcrossScenes = false) 
        where T : MonoBehaviour
    {
        if (staticInstance != null && staticInstance != instance)
        {
            Debug.LogWarning($"Duplicate singleton instance of type {typeof(T)} detected. Destroying duplicate.");
            Object.Destroy(instance.gameObject);
            return staticInstance;
        }

        staticInstance = instance;
        
        if (persistAcrossScenes)
        {
            Object.DontDestroyOnLoad(instance.gameObject);
        }

        return instance;
    }
}
