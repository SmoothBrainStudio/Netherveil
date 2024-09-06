using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class DescriptionTabHUD : MonoBehaviour
{
    [SerializeField] private RectTransform tabRectTransform;
    [SerializeField] private TMP_Text titleTextMesh;
    [SerializeField] private TMP_Text descriptionTextMesh;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Image background;
    [SerializeField] private Button CloseButton;
    float duration = 0.1f;
    private Coroutine displayRoutine;
    public bool isOpen = false;

    private void OnDisable()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            tabRectTransform.localScale = Vector3.zero;
        }
        isOpen = false;
    }

    public void SetTab(string title, string description, VideoClip clip, Sprite background)
    {
        titleTextMesh.text = title;
        descriptionTextMesh.text = description;
        videoPlayer.clip = clip;
        this.background.sprite = background;
    }

    public void OpenTab()
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        displayRoutine = StartCoroutine(tabRectTransform.UpScaleCoroutine(duration));

        StartCoroutine(PauseGame());
        EventSystem.current.SetSelectedGameObject(CloseButton.gameObject);
        isOpen = true;
    }

    public void CloseTab()
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        displayRoutine = StartCoroutine(tabRectTransform.DownScaleCoroutine(duration));

        Time.timeScale = 1;
        Utilities.PlayerInput.EnableGameplayInputs();
        isOpen = false;
    }

    private IEnumerator PauseGame()
    {
        yield return new WaitForSeconds(duration);
        Time.timeScale = 0;
        Utilities.PlayerInput.DisableGameplayInputs();
    }
}
