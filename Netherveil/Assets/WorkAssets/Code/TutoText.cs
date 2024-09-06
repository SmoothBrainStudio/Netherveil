using Map;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class TutoText : MonoBehaviour
{
    [SerializeField] List<InputActionReference> keyboardActions;
    [SerializeField] List<InputActionReference> gamepadActions;
    [SerializeField] InputBinding.DisplayStringOptions displayStringOptions;

    string initText = string.Empty;
    TMP_Text text;

    void Awake()
    {
        // TODO : temporary
        if (MapUtilities.Stage > 1)
        {
            Destroy(gameObject);
            return;
        }
        //

        text = GetComponent<TMP_Text>();
        initText = text.text;

        PauseMenu.OnUnpause += UpdateBindingDisplayString;
        DeviceManager.OnChangedToKB += UpdateBindingDisplayString;
        DeviceManager.OnChangedToGamepad += UpdateBindingDisplayString;

        UpdateBindingDisplayString();
    }

    private void OnDestroy()
    {
        DeviceManager.OnChangedToKB -= UpdateBindingDisplayString;
        DeviceManager.OnChangedToGamepad -= UpdateBindingDisplayString;
        PauseMenu.OnUnpause -= UpdateBindingDisplayString;
    }

    private void UpdateBindingDisplayString()
    {
        List<InputActionReference> actionRefs = GetCurrentAction();

        string textString = initText;

        if (actionRefs[0].action.name == "Move" && DeviceManager.Instance.IsPlayingKB())
        {
            if (textString.Contains("^") && !textString.Contains("\"^^\""))
            {
                textString = textString.Replace("^", "<sprite name=\"" + GetDisplayString(actionRefs[0], 1).GetCamelCase() + "\">");
            }
            if (textString.Contains("$") && !textString.Contains("\"$\""))
            {
                textString = textString.Replace("$", "<sprite name=\"" + GetDisplayString(actionRefs[0], 3).GetCamelCase() + "\">");
            }
            if (textString.Contains("%") && !textString.Contains("\"%\""))
            {
                textString = textString.Replace("%", "<sprite name=\"" + GetDisplayString(actionRefs[0], 2).GetCamelCase() + "\">");
            }
            if (textString.Contains("*") && !textString.Contains("\"*\""))
            {
                textString = textString.Replace("*", "<sprite name=\"" + GetDisplayString(actionRefs[0], 4).GetCamelCase() + "\">");
            }
        }
        else if (actionRefs[0].action.name == "Move" && !DeviceManager.Instance.IsPlayingKB())
        {
            textString = textString.Replace("^", "<sprite name=\"" + "leftStick" + (DeviceManager.Instance.CurrentDevice is DualShockGamepad ? "_ps" : "_xbox") + "\">");
            textString = textString.Replace("$", string.Empty);
            textString = textString.Replace("%", string.Empty);
            textString = textString.Replace("*", string.Empty);
        }
        else
        {
            if (textString.Contains("^") && !textString.Contains("\"^^\""))
            {
                textString = textString.Replace("^", "<sprite name=\"" + GetDisplayString(actionRefs[0]).GetCamelCase() + "\">");
            }
            if (textString.Contains("$") && !textString.Contains("\"$\""))
            {
                textString = textString.Replace("$", "<sprite name=\"" + GetDisplayString(actionRefs[1]).GetCamelCase() + "\">");
            }
        }
        text.text = textString;

    }

    private List<InputActionReference> GetCurrentAction()
    {
        if (DeviceManager.Instance.IsPlayingKB())
        {
            return keyboardActions;
        }
        else
        {
            return gamepadActions;
        }
    }

    private string GetDisplayString(InputActionReference actionRef, int bindingIndex = 0)
    {
        // Get display string from action.
        var action = actionRef != null ? actionRef.action : null;
        string displayString = string.Empty;

        if (action != null)
        {

            displayString = Keybinding.GetAppropriateKeyString(actionRef, bindingIndex, displayStringOptions);
            if (!DeviceManager.Instance.IsPlayingKB() && DeviceManager.Instance.CurrentDevice is DualShockGamepad)
            {
                displayString += "_ps";
            }
            else if (!DeviceManager.Instance.IsPlayingKB())
            {
                displayString += "_xbox";
            }
        }

        return displayString;
    }
}
