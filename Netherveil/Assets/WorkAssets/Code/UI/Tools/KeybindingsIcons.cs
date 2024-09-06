using System;
using TMPro;
using UnityEngine.UI;

////TODO: have updateBindingUIEvent receive a control path string, too (in addition to the device layout name)

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    /// <summary>
    /// This is an example for how to override the default display behavior of bindings. The component
    /// hooks into <see cref="RebindActionUI.updateBindingUIEvent"/> which is triggered when UI display
    /// of a binding should be refreshed. It then checks whether we have an icon for the current binding
    /// and if so, replaces the default text display with an icon.
    /// </summary>
    public class KeybindingsIcons : MonoBehaviour
    {
        public GamepadIconsSprites xbox;
        public GamepadIconsSprites ps4;
        public KeyboardIconsSprites kb;
        public MouseIconsSprites mouse;

        protected void Start()
        {
            // Hook into all updateBindingUIEvents on all RebindActionUI components in our hierarchy.
            var rebindUIComponents = FindObjectsOfType<RebindActionUI>(true);
            foreach (var component in rebindUIComponents)
            {
                component.updateBindingUIEvent.AddListener(OnUpdateBindingDisplay);
                component.UpdateBindingDisplay();
            }
        }

        protected void OnUpdateBindingDisplay(RebindActionUI component, string bindingDisplayString, string deviceLayoutName, string controlPath)
        {
            if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath))
                return;

            var icon = default(Sprite);
            
            if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
                icon = ps4.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
                icon = xbox.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Keyboard"))
                icon = kb.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Mouse"))
                icon = mouse.GetSprite(controlPath);


            var textComponent = component.bindingText;

            // Grab Image component.
            var imageGO = textComponent.transform.parent.Find("ActionBindingIcon");

            if (imageGO != null)
            {
                var imageComponent = imageGO.GetComponent<Image>();
                var textMesh = imageComponent.GetComponentInChildren<TMP_Text>();

                if (icon != null)
                {
                    textComponent.gameObject.SetActive(false);
                    imageComponent.sprite = icon;
                    imageComponent.gameObject.SetActive(true);

                    if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Keyboard"))
                        textMesh.text = Keybinding.GetAppropriateKeyString(controlPath, bindingDisplayString);
                    else if (textMesh != null)
                        textMesh.text = string.Empty;
                }
                else
                {
                    imageComponent.gameObject.SetActive(false);
                    textComponent.gameObject.SetActive(true);
                }
            }
        }

        [Serializable]
        public struct GamepadIconsSprites

        {
            public Sprite buttonSouth;
            public Sprite buttonNorth;
            public Sprite buttonEast;
            public Sprite buttonWest;
            public Sprite startButton;
            public Sprite selectButton;
            public Sprite leftTrigger;
            public Sprite rightTrigger;
            public Sprite leftShoulder;
            public Sprite rightShoulder;
            public Sprite dpad;
            public Sprite dpadUp;
            public Sprite dpadDown;
            public Sprite dpadLeft;
            public Sprite dpadRight;
            public Sprite leftStick;
            public Sprite rightStick;
            public Sprite leftStickPress;
            public Sprite rightStickPress;

            public Sprite GetSprite(string controlPath)
            {
                // From the input system, we get the path of the control on device. So we can just
                // map from that to the sprites we have for gamepads.
                switch (controlPath)
                {
                    case "buttonSouth": return buttonSouth;
                    case "buttonNorth": return buttonNorth;
                    case "buttonEast": return buttonEast;
                    case "buttonWest": return buttonWest;
                    case "start": return startButton;
                    case "select": return selectButton;
                    case "leftTrigger": return leftTrigger;
                    case "rightTrigger": return rightTrigger;
                    case "leftShoulder": return leftShoulder;
                    case "rightShoulder": return rightShoulder;
                    case "dpad": return dpad;
                    case "dpad/up": return dpadUp;
                    case "dpad/down": return dpadDown;
                    case "dpad/left": return dpadLeft;
                    case "dpad/right": return dpadRight;
                    case "leftStick": return leftStick;
                    case "rightStick": return rightStick;
                    case "leftStickPress": return leftStickPress;
                    case "rightStickPress": return rightStickPress;
                }
                return null;
            }
        }

        [Serializable]
        public struct KeyboardIconsSprites

        {
            public Sprite buttonLong;
            public Sprite buttonMedium;
            public Sprite buttonSimple;
            public Sprite buttonCorner;

            public Sprite GetSprite(string controlPath)
            {
                // From the input system, we get the path of the control on device. So we can just
                // map from that to the sprites we have for gamepads.

                switch (controlPath)
                {
                    case "enter": return buttonCorner;
                    case "escape": return buttonMedium;
                    case "backspace": return buttonMedium;
                    case "rightShift": return buttonMedium;
                    case "leftShift": return buttonMedium;
                    case "rightCtrl": return buttonMedium;
                    case "leftCtrl": return buttonMedium;
                    case "capsLock": return buttonMedium;
                    case "tab": return buttonMedium;
                    case "space": return buttonLong;
                    default: return buttonSimple;
                }
            }
        }

        [Serializable]
        public struct MouseIconsSprites

        {
            public Sprite mouse;
            public Sprite mouseLeft;
            public Sprite mouseRight;
            public Sprite mouseScroll;

            public Sprite GetSprite(string controlPath)
            {
                // From the input system, we get the path of the control on device. So we can just
                // map from that to the sprites we have for gamepads.

                switch (controlPath)
                {
                    case "leftButton": return mouseLeft;
                    case "rightButton": return mouseRight;
                    case "middleButton": return mouseScroll;
                    default: return mouse;
                }
            }
        }
    }
}
