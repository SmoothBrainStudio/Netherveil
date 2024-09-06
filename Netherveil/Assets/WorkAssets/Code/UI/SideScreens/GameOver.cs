using Cinemachine;
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject firstSelect;
    readonly List<Graphic> drawables = new List<Graphic>();

    public CinemachineVirtualCamera mainCam;
    public CinemachineVirtualCamera deathCam;
    public Sound deathClockSound;

    public bool GameOverActive => gameObject.activeSelf;

    void FindDrawablesRecursively(Transform current)
    {
        if (current.TryGetComponent(out Graphic graphic))
        {
            drawables.Add(graphic);
        }

        foreach (Transform child in current)
        {
            FindDrawablesRecursively(child);
        }
    }

    public void LaunchDeathCam()
    {
        Camera.main.cullingMask = LayerMask.GetMask("Entity");
        FindDrawablesRecursively(transform);
        mainCam.m_Priority = -1;
        deathCam.m_Priority = 1;
        Color clearColor = new Color(1, 1, 1, 0);
        Camera.main.backgroundColor = new Color(0.31f, 0.31f, 0.31f) * clearColor;

        foreach (Graphic drawable in drawables)
        {
            drawable.color *= clearColor;
        }

        AudioManager.Instance.StopAllMusics();
        deathClockSound.Play();
        DisableAllMob();
        StartCoroutine(IncreaseAlpha());
    }

    private void DisableAllMob()
    {
        if (MapUtilities.currentRoomData.Enemies != null && MapUtilities.currentRoomData.Enemies.Count > 0)
        {
            foreach (GameObject enemy in MapUtilities.currentRoomData.Enemies)
            {
                if (enemy != null)
                {
                    enemy.SetActive(false);
                }
            }
        }
    }

    IEnumerator IncreaseAlpha()
    {
        float targetAlpha = 1.0f;
        float duration = 2.0f;
        float elapsedTime = 0f;
        Color initialColor = Camera.main.backgroundColor;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, targetAlpha);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Camera.main.backgroundColor = Color.Lerp(initialColor, targetColor, t);
            yield return null;
        }

        foreach (Graphic drawable in drawables)
        {
            StartCoroutine(IncreaseElementAlpha(drawable));
        }
    }

    IEnumerator IncreaseElementAlpha(Graphic element)
    {
        float targetAlpha = 1.0f;
        float duration = 2.0f;
        float elapsedTime = 0f;
        Color initialColor = element.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, targetAlpha);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            element.color = Color.Lerp(initialColor, targetColor, t);
            yield return null;
        }

        if (element.gameObject.TryGetComponent(out Button button))
        {
            button.interactable = true;
            EventSystem.current.SetSelectedGameObject(firstSelect);
        }

    }
}
