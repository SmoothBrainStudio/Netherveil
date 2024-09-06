using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputBinding;

public class Keybinding : MonoBehaviour
{
    [SerializeField] InputActionAsset playerInput;

    public void ResetCurrentBindings()
    {
        if(DeviceManager.Instance.IsPlayingKB())
        {
            playerInput.FindActionMap("Keyboard", throwIfNotFound: true).RemoveAllBindingOverrides();
        }
        else
        {
            playerInput.FindActionMap("Gamepad", throwIfNotFound: true).RemoveAllBindingOverrides();
        }
    }

    public void ResetKeyboardBindings()
    {
        playerInput.FindActionMap("Keyboard", throwIfNotFound: true).RemoveAllBindingOverrides();
    }

    public void ResetGamepadBindings()
    {
        playerInput.FindActionMap("Gamepad", throwIfNotFound: true).RemoveAllBindingOverrides();
    }

    public static string GetAppropriateKeyString(InputActionReference actionRef, int bindingIndex = 0, DisplayStringOptions displayStringOptions = 0)
    {
        var action = actionRef != null ? actionRef.action : null;
        string bindingDisplayString = action.GetBindingDisplayString(bindingIndex, out _, out string controlPath, displayStringOptions);
        return GetAppropriateKeyString(controlPath, bindingDisplayString);
    }

    public static string GetAppropriateKeyString(string controlPath, string bindingDisplayString)
    {
        //OEM represents keys specific to specific keyboard layout or even manufacturers so there are too many different things it can be so
        //we just ignore it display a placeholder icon key
        if (controlPath.Contains("OEM"))
            return controlPath;

        switch (controlPath)
        {
            //keyboard bindings
            case "escape": return controlPath;
            case "space": return controlPath;
            case "enter": return controlPath;
            case "tab": return controlPath;
            case "backquote": return controlPath;
            //case "quote": return controlPath;
            //case "semicolon": return controlPath;
            //case "comma": return controlPath;
            //case "period": return controlPath;
            //case "slash": return controlPath;
            //case "backslash": return controlPath;
            //case "leftBracket": return controlPath;
            //case "rightBracket": return controlPath;
            //case "minus": return controlPath;
            case "equals": return controlPath;
            case "upArrow": return controlPath;
            case "downArrow": return controlPath;
            case "leftArrow": return controlPath;
            case "rightArrow": return controlPath;
            case "1": return controlPath;
            case "2": return controlPath;
            case "3": return controlPath;
            case "4": return controlPath;
            case "5": return controlPath;
            case "6": return controlPath;
            case "7": return controlPath;
            case "8": return controlPath;
            case "9": return controlPath;
            case "0": return controlPath;
            case "leftShift": return controlPath;
            case "rightShift": return controlPath;
            case "shift": return controlPath;
            case "leftAlt": return controlPath;
            case "rightAlt": return controlPath;
            case "alt": return controlPath;
            case "leftCtrl": return controlPath;
            case "rightCtrl": return controlPath;
            case "ctrl": return controlPath;
            case "leftMeta": return controlPath;
            case "rightMeta": return controlPath;
            case "contextMenu": return controlPath;
            case "backspace": return controlPath;
            case "pageDown": return controlPath;
            case "pageUp": return controlPath;
            case "home": return controlPath;
            case "end": return controlPath;
            case "insert": return controlPath;
            case "delete": return controlPath;
            case "capsLock": return controlPath;
            case "numLock": return controlPath;
            case "printScreen": return controlPath;
            case "scrollLock": return controlPath;
            case "pause": return controlPath;
            case "numpadEnter": return controlPath;
            case "numpadDivide": return controlPath;
            case "numpadMultiply": return controlPath;
            case "numpadPlus": return controlPath;
            case "numpadMinus": return controlPath;
            case "numpadPeriod": return controlPath;
            case "numpadEquals": return controlPath;
            case "numpad1": return controlPath;
            case "numpad2": return controlPath;
            case "numpad3": return controlPath;
            case "numpad4": return controlPath;
            case "numpad5": return controlPath;
            case "numpad6": return controlPath;
            case "numpad7": return controlPath;
            case "numpad8": return controlPath;
            case "numpad9": return controlPath;
            case "numpad0": return controlPath;
            case "f1": return controlPath;
            case "f2": return controlPath;
            case "f3": return controlPath;
            case "f4": return controlPath;
            case "f5": return controlPath;
            case "f6": return controlPath;
            case "f7": return controlPath;
            case "f8": return controlPath;
            case "f9": return controlPath;
            case "f10": return controlPath;
            case "f11": return controlPath;
            case "f12": return controlPath;
            //mouse bindings
            case "leftButton": return controlPath;
            case "rightButton": return controlPath;
            case "middleButton": return controlPath;
            case "forwardButton": return controlPath;
            //gamepad bindings
            case "backButton": return controlPath;
            case "buttonSouth": return controlPath;
            case "buttonNorth": return controlPath;
            case "buttonEast": return controlPath;
            case "buttonWest": return controlPath;
            case "start": return controlPath;
            case "select": return controlPath;
            case "leftTrigger": return controlPath;
            case "rightTrigger": return controlPath;
            case "leftShoulder": return controlPath;
            case "rightShoulder": return controlPath;
            case "dpad": return controlPath;
            case "dpad/up": return controlPath;
            case "dpad/down": return controlPath;
            case "dpad/left": return controlPath;
            case "dpad/right": return controlPath;
            case "leftStick": return controlPath;
            case "rightStick": return controlPath;
            case "leftStickPress": return controlPath;
            case "rightStickPress": return controlPath;
            //if key is localization dependent (letter keys and some other specific ones) display the binding display string
            default:
                return bindingDisplayString;
        }

    }
}
