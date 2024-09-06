using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Selectable firstSelect;

    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject settings;

    [SerializeField] private GameOver gameOverPage;

    public static event Action OnPause;
    public static event Action OnUnpause;

    public void Toggle()
    {
        if (gameObject.activeSelf || settings.gameObject.activeSelf)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (settings.gameObject.activeSelf || gameOverPage.GameOverActive || HudHandler.current.DescriptionTab.isOpen)
            return;

        Time.timeScale = 0.0f;

        Utilities.PlayerInput.DisableGameplayInputs();
        HudHandler.current.SetActive(false, 0.0f);
        gameObject.SetActive(true);

        OnPause?.Invoke();

        AudioManager.Instance.PauseAllSounds();
        EventSystem.current.SetSelectedGameObject(firstSelect.gameObject);
    }

    public void Resume()
    {
        if (gameOverPage.GameOverActive)
            return;

        Time.timeScale = 1.0f;
        Utilities.PlayerInput.EnableGameplayInputs();

        HudHandler.current.SetActive(true, 0.0f);
        gameObject.SetActive(false);
        settings.SetActive(false);
        AudioManager.Instance.ResumeAllSounds();

        OnUnpause?.Invoke();
    }

    public void ReloadGame()
    {
        EventSystem.current.gameObject.SetActive(false);
        LevelLoader.current.LoadScene(SceneManager.GetActiveScene().buildIndex, true);
        AudioManager.Instance.ResumeAllMusics();

    }

    public void Setting()
    {
        settings.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Menu()
    {
        EventSystem.current.gameObject.SetActive(false);
        Time.timeScale = 1f;
        LevelLoader.current.LoadScene("MainMenu", true);
        AudioManager.Instance.ResumeAllSounds();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
