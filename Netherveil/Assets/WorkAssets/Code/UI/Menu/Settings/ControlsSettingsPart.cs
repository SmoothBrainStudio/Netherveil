using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsSettingsPart : MenuPart
{
    [System.Serializable]
    private class KeybindingPart
    {
        public GameObject part;
        public GameObject firstSelection;
    }

    [Header("Control Settings")]
    [SerializeField] private Slider deadzoneMin;
    [SerializeField] private Slider deadzoneMax;
    [SerializeField] private Toggle vibrationsToggle;
    [SerializeField] private Toggle mouseDashToggle;
    [SerializeField] private Button buttonKeybinding;
    [SerializeField] private TMP_Text buttonKeybindingTextMesh;
    [SerializeField] private KeybindingPart GlobalPart;
    [SerializeField] private KeybindingPart KeyboardKeysPart;
    [SerializeField] private KeybindingPart GamepadKeysPart;
    private KeybindingPart currentKeysPart;

    private void Start()
    {
        deadzoneMin.value = InputSystem.settings.defaultDeadzoneMin;
        deadzoneMax.value = InputSystem.settings.defaultDeadzoneMax;
        vibrationsToggle.isOn = DeviceManager.Instance.toggleVibrations;

        if (DeviceManager.Instance.IsPlayingKB())
            OnKeyboardEnable();
        else
            OnGamepadEnable();
    }
    private void OnEnable()
    {
        DeviceManager.OnChangedToKB += OnKeyboardEnable;
        DeviceManager.OnChangedToGamepad += OnGamepadEnable;
    }

    private void OnDisable()
    {
        DeviceManager.OnChangedToKB -= OnKeyboardEnable;
        DeviceManager.OnChangedToGamepad -= OnGamepadEnable;
    }

    private void OnKeyboardEnable()
    {
        deadzoneMin.transform.parent.gameObject.SetActive(false);
        deadzoneMax.transform.parent.gameObject.SetActive(false);
        vibrationsToggle.transform.parent.gameObject.SetActive(false);
        mouseDashToggle.transform.parent.gameObject.SetActive(true);

        currentKeysPart = KeyboardKeysPart;

        Navigation nav = buttonKeybinding.navigation;
        nav.selectOnDown = mouseDashToggle;
        buttonKeybinding.navigation = nav;

        buttonKeybindingTextMesh.text = "Keyboard";

        BackKeybinding();
    }

    private void OnGamepadEnable()
    {
        deadzoneMin.transform.parent.gameObject.SetActive(true);
        deadzoneMax.transform.parent.gameObject.SetActive(true);
        vibrationsToggle.transform.parent.gameObject.SetActive(true);
        mouseDashToggle.transform.parent.gameObject.SetActive(false);

        currentKeysPart = GamepadKeysPart;

        Navigation nav = buttonKeybinding.navigation;
        nav.selectOnDown = deadzoneMin;
        buttonKeybinding.navigation = nav;

        buttonKeybindingTextMesh.text = "Gamepad";

        BackKeybinding();
    }

    public void ToggleVibrations(bool toggle)
    {
        DeviceManager.Instance.toggleVibrations = toggle;
    }

    public void ChangeStickDeadzoneMin(float value)
    {
        InputSystem.settings.defaultDeadzoneMin = value;
    }

    public void ChangeStickDeadzoneMax(float value)
    {
        InputSystem.settings.defaultDeadzoneMax = value;
    }

    public void ToggleDashMouse(bool toggle)
    {
        GameManager.Instance.dashWithMouse = toggle;
    }

    public void OpenKeybinding()
    {
        GlobalPart.part.SetActive(false);
        currentKeysPart.part.SetActive(true);
        EventSystem.current.SetSelectedGameObject(currentKeysPart.firstSelection);
    }

    public void BackKeybinding()
    {
        GlobalPart.part.SetActive(true);

        KeyboardKeysPart.part.SetActive(false);
        GamepadKeysPart.part.SetActive(false);

        EventSystem.current.SetSelectedGameObject(GlobalPart.firstSelection);
    }
}
