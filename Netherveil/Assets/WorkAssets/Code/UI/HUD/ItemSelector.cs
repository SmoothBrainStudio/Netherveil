using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ItemSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private new string name;
    private string state;
    private string description;

    private RectTransform rectTransform => transform as RectTransform;
    private ItemDescriptionUI panel;

    private void Awake()
    {
        panel = FindAnyObjectByType<ItemDescriptionUI>();
    }

    public void SetPanelDescriton(string _name, string _state, string _description)
    {
        name = _name;
        state = _state;
        description = _description;
    }

    private void SetPanelPosAtCursor()
    {
       panel.rectTransform.anchoredPosition = Vector2.zero;
       panel.rectTransform.pivot = Vector2.zero;
        panel.rectTransform.localScale = Vector3.one;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(panel.rectTransform, Input.mousePosition, null, out Vector2 mousePosition);

        panel.SetAnchorPosition(mousePosition);
        panel.rectTransform.localScale = Vector3.zero;
    }

    private void SetPanelPosAtCorner()
    {
        Vector3 cornerPos = rectTransform.position;
        cornerPos.x = MathF.Max(cornerPos.x, 46f);

        panel.SetPosition(cornerPos);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (string.IsNullOrEmpty(description) || DeviceManager.Instance.IsPlayingKB())
            return;

        panel.SetDescription(name, state, description);
        SetPanelPosAtCorner();
        panel.Toggle(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (string.IsNullOrEmpty(description) || DeviceManager.Instance.IsPlayingKB())
            return;

        panel.Toggle(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(description))
            return;

        panel.SetDescription(name, state, description);
        SetPanelPosAtCursor();
        panel.Toggle(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(description))
            return;

        panel.Toggle(false);
    }
}
