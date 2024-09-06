using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSettingsManager()
    {
        _ = Instance;
    }

    private static SettingsManager instance = null;
    public static SettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                Instantiate(Resources.Load<GameObject>(nameof(SettingsManager)));
            }

            return instance;
        }
    }

    private void Awake()
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
}
