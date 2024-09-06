using System.Collections;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager instance = null;
    public static CoroutineManager Instance
    {
        get
        {
            if (instance == null)
            {
                Instantiate(Resources.Load<GameObject>(nameof(CoroutineManager)));
            }

            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void StartCustom(IEnumerator _coroutine)
    {
        StartCoroutine(_coroutine);
    }

    public void StopCustom(IEnumerator _coroutine)
    {
        StopCoroutine(_coroutine);
    }

    public static void StopAllCoroutinesInstance()
    {
        if (instance != null)
        {
            Instance.StopAllCoroutines();
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
