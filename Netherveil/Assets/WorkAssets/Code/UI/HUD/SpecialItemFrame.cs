using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialItemFrame : ItemFrame
{
    [Space]
    [SerializeField] private Image backgroundFiller;
    [SerializeField] private TMP_Text cooldownText;

    [Space]
    [SerializeField] private Image backgroundKey;
    [SerializeField] private TMP_Text keyText;

    public void ToggleCooldown(bool toggle)
    {
        backgroundFiller.gameObject.SetActive(toggle);
        cooldownText.gameObject.SetActive(toggle);
    }

    public void SetCooldown(float current, float duration)
    {
        float factor = current / duration;

        backgroundFiller.fillAmount = 1 - factor;
        cooldownText.text = (Mathf.RoundToInt(duration) - Mathf.RoundToInt(current)).ToString();
    }

    public void SetKey(Sprite sprite, string key)
    {
        keyText.text = key;
        backgroundKey.sprite = sprite;
    }

    public void SetKey(Sprite sprite)
    {
        SetKey(sprite, string.Empty);
    }
}
