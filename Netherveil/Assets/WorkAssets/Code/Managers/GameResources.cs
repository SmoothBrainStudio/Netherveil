using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    static bool load = false;
    static private readonly Dictionary<string, Object> objDictionary = new Dictionary<string, Object>();
    [SerializeField] private List<Object> objectsToLoad;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadGameResources()
    {
        GameObject.Instantiate(Resources.Load<GameObject>(nameof(GameResources)));
    }

    private void Awake()
    {
        if (!load)
        {
            foreach (Object obj in objectsToLoad)
            {
                if (objDictionary.ContainsKey(obj.name))
                {
                    Debug.LogError("Two object have the same keys >:( !!! " + obj.name, obj);
                    continue;
                }

                objDictionary.Add(obj.name, obj);
            }
            objectsToLoad.Clear();
            objectsToLoad = null;

            load = true;
        }

        Destroy(gameObject);
    }

    static public T Get<T>(string key) where T : Object
    {
        if (!objDictionary.ContainsKey(key))
        {
            T obj = Resources.Load<T>(key);
            if (obj == null)
            {
                Debug.LogError("GameResources doesn't contain " + key + " and can't load this from resources file");
            }

            Debug.LogWarning("Load asset from Resources, can cause lag on the middle of the gameplay", obj);
            objDictionary.Add(key, obj);
        }

        return (T)objDictionary[key];
    }

    static public bool Remove(string key)
    {
        return objDictionary.Remove(key);
    }
}