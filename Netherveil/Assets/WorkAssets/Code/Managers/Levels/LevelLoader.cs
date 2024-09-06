using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private static LevelLoader instance;
    public static LevelLoader current
    {
        get
        {
            if (instance == null)
                throw new System.Exception("No LevelLoader in the scene !");

            return instance;
        }
    }

    [SerializeField] private Transition transition = null;

    private void Awake()
    {
        instance = this;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }

    public void LoadScene(string sceneName)
    {
        int sceneIndex = GetIndexSceneByName(sceneName);
        LoadScene(sceneIndex, false);
    }

    public void LoadScene(string sceneName, bool transitionActive)
    {
        int sceneIndex = GetIndexSceneByName(sceneName);
        LoadScene(sceneIndex, transitionActive);
    }

    public void LoadScene(int sceneIndex, bool transitionActive = false)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"No scene with the index {sceneIndex} !");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneIndex, transitionActive));
    }

    private IEnumerator LoadSceneRoutine(int sceneIndex, bool transitionActive = false)
    {
        if (transitionActive)
        {
            transition.Toggle();
            yield return new WaitUntil(() => transition.TransitionEnd);
        }

        SceneManager.LoadScene(sceneIndex);
    }

    #region Utils
    private int GetIndexSceneByName(string name)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameInBuild == name)
            {
                return i;
            }
        }

        return -1;
    }
    #endregion
}
