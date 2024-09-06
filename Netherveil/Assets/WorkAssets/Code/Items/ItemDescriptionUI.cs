using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ItemDescriptionUI : MonoBehaviour
{
    [SerializeField] private new TMP_Text name;
    [SerializeField] private TMP_Text state;
    [SerializeField] private TMP_Text description;
    [SerializeField] private float durationScale = 0.1f;

    public RectTransform rectTransform => transform as RectTransform;
    private Coroutine routine;
    private bool isToggle = false;

    public void SetDescription(string name, string state, string description)
    {
        this.name.text = name;
        this.state.text = state;
        this.description.text = description;
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            rectTransform.localScale = Vector3.zero;
        }
    }

    private void Update()
    {
        if (!HudHandler.current.ItemBar.ToggleOn && isToggle)
        {
            ScaleDownRoutine();
            isToggle = false;
        }
    }

    public void Toggle(bool toggle)
    {
        if (HudHandler.current.ItemBar.ToggleOn && toggle)
        {
            ScaleUpRoutine();
            isToggle = true;
        }
        else
        {
            ScaleDownRoutine();
            isToggle = false;
        }
    }

    private void ScaleUpRoutine()
    {
        // Scale routine
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(rectTransform.UpScaleCoroutine(durationScale));
    }

    private void ScaleDownRoutine()
    {
        // Scale routine
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(rectTransform.DownScaleCoroutine(durationScale));
    }

    public void SetAnchorPosition(Vector3 position)
    {
        if (position.y > Screen.height / 2.0f)
            rectTransform.pivot = Vector2.up;
        else
            rectTransform.pivot = Vector2.zero;

        rectTransform.anchoredPosition = position;
    }

    public void SetPosition(Vector3 position)
    {
        if (position.y > Screen.height / 2.0f)
            rectTransform.pivot = Vector2.up;
        else
            rectTransform.pivot = Vector2.zero;

        rectTransform.position = position;
    }
}
