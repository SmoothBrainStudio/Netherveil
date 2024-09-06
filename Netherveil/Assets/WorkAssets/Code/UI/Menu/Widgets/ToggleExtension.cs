using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleExtension : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text toggleText;

    private void Start()
    {
        toggle.onValueChanged.AddListener(OnToggle);
        OnToggle(toggle.isOn);
    }

    public void OnToggle(bool toggle)
    {
        if (toggle)
            toggleText.text = "ENABLE";
        else
            toggleText.text = "DISABLE";
    }
}
