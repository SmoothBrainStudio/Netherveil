using Map;
using System.Linq;
using System.Security.Principal;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TextEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private string descriptionToSet;

    private string description = string.Empty;
    private ItemData dataToChange;
    ItemDatabase database;

    private int firstSelect;
    private int lastSelect;

    private bool mousePressed;
    public static void OpenWindow(ItemData data)
    {
        TextEditor wnd = GetWindow<TextEditor>();
        wnd.titleContent = new GUIContent("Text Editor");
        wnd.description = data.Description;
        wnd.dataToChange = data;
        VisualElement root = wnd.rootVisualElement;
        root.Q<TextField>("Description").value = data.Description;
        root.Q<Label>("Preview").text = data.Description;
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
        database = GameResources.Get<ItemDatabase>("ItemDatabase");

        var descriptionField = root.Q<TextField>("Description");

        Button saveButton = root.Q<Button>("Save");
        saveButton.clicked += Save;

        Button addColorButton = root.Q<Button>("AddColor");
        addColorButton.focusable = false;
        addColorButton.clicked += AddColor;
    }
    private void OnGUI()
    {
        VisualElement root = rootVisualElement;
        var descriptionField = root.Q<TextField>("Description");
        description = descriptionField.value;

        SetSelection();
        root.Q<Label>("Preview").text = descriptionField.value;
    }

    private void Save()
    {
        dataToChange.Description = description;
        Close();
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssetIfDirty(database);
    }

    private void AddColor()
    {
        VisualElement root = rootVisualElement;
        // Get uxml field
        var descriptionField = root.Q<TextField>("Description");
        var colorPicker = root.Q<ColorField>("ColorPicker");


        Color color = colorPicker.value;
        int delta = descriptionField.value.Length;
        if (firstSelect != lastSelect)
        {
            var word = descriptionField.value.Substring(firstSelect, lastSelect - firstSelect);
            Debug.Log(word);
            int index = -1;
            if (!word.Contains("<color=#") && !HasColor(out index))
            {
                descriptionField.value = descriptionField.value.Insert(firstSelect, "<color=#" + color.ToHexString() + ">");
                delta = descriptionField.value.Length - delta;
                descriptionField.value = descriptionField.value.Insert(lastSelect + delta, "</color>");
            }
            else
            {
                var splitString = descriptionField.value.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                descriptionField.value = string.Empty;
                bool hasChanged = false;
                for(int i = 0; i < splitString.Length; i++)
                {
                    int indexColor = splitString[i].IndexOf('#');
                    if (!hasChanged && indexColor != -1 && i >= index)
                    {
                        splitString[i] = splitString[i].Replace(splitString[i].Substring(indexColor + 1, 8), color.ToHexString());
                        hasChanged = true;
                    }
                    descriptionField.value += splitString[i] + " ";
                }
            }

        }

    }

    private bool HasColor(out int colorIndex)
    {
        VisualElement root = rootVisualElement;
        // Get uxml field
        var descriptionField = root.Q<TextField>("Description");
        var substring = descriptionField.value.Substring(0, firstSelect);
        string[] substringSplit = substring.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = substringSplit.Length - 1; i >= 0; i--)
        {
            
            if (substringSplit[i].Contains("</color>"))
            {
                colorIndex = -1;
                return false;
            }
            if (substringSplit[i].Contains("<color=#"))
            {
                colorIndex = i;
                return true;
            }
        }
        colorIndex = 0;
        return false;
    }
    private void SetSelection()
    {
        if (rootVisualElement.Q<TextField>().focusController.focusedElement is not TextField) return;
        VisualElement root = rootVisualElement;
        var descriptionField = root.Q<TextField>("Description");
        firstSelect = descriptionField.cursorIndex;
        lastSelect = descriptionField.selectIndex;
        if (lastSelect < firstSelect)
        {
            (firstSelect, lastSelect) = (lastSelect, firstSelect);
        }
    }
}