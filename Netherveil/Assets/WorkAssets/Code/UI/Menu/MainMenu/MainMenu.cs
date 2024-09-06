using MeshUI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private MeshButton[] meshButtons;
    [SerializeField] private Selectable selectable;

    public void StartGame()
    {
        LevelLoader.current.LoadScene("InGame");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
#else
        Application.Quit();
#endif
    }

    public void SetEnableMainMenu(bool enable)
    {
        SetEnableAllMeshButton(enable);
    }

    private void SetEnableAllMeshButton(bool enable)
    {
        foreach (MeshButton meshButton in meshButtons)
        {
            meshButton.enabled = enable;
        }
    }
}
