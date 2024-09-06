using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolNamer : EditorWindow
{
    private GroupBox replaceBox;
    private GroupBox deleteBox;
    private GroupBox randomBox;
    private GroupBox currentOpen;

    private TextField replaceFromField;
    private TextField replaceToField;
    private TextField deleteField;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Tools/Graphs Utilities/Namer", priority = 10)]
    public static void OpenWindow()
    {
        ToolNamer wnd = GetWindow<ToolNamer>();
        wnd.titleContent = new GUIContent("Tool Graphs Editor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        replaceBox = root.Q<GroupBox>("ReplaceBox");
        deleteBox = root.Q<GroupBox>("DeleteBox");
        randomBox = root.Q<GroupBox>("RandomRotBox");

        root.Q<Button>("Replace-Toolbar-Button").clicked += () => DisplayGroup(replaceBox);
        root.Q<Button>("Delete-Toolbar-Button").clicked += () => DisplayGroup(deleteBox);
        root.Q<Button>("RandomRotate-Toolbar-Button").clicked += () => DisplayGroup(randomBox);

        root.Q<Button>("Replace-Button").clicked += () => Replace();
        root.Q<Button>("Delete-Button").clicked += () => Delete();
        root.Q<Button>("Random-Button").clicked += () => RandomRot();

        replaceFromField = root.Q<TextField>("Replace-From");
        replaceToField = root.Q<TextField>("Replace-To");
        deleteField = root.Q<TextField>("Delete-Word");

        DisplayGroup(replaceBox);
    }

    public void DisplayGroup(GroupBox toDisplay)
    {
        if (currentOpen != null)
            currentOpen.style.display = DisplayStyle.None;

        toDisplay.style.display = DisplayStyle.Flex;
        currentOpen = toDisplay;
    }

    public void Replace()
    {
        GameObject[] objects = Selection.gameObjects;
        foreach (var item in objects)
        {
            item.name = item.name.Replace(replaceFromField.value, replaceToField.value);
        }
    }

    public void Delete()
    {
        GameObject[] objects = Selection.gameObjects;
        foreach (var item in objects)
        {
            item.name = item.name.Replace(deleteField.value, "");
        }
    }

    public void RandomRot()
    {
        float MinRotY = 0f;
        float MaxRotY = 360f;

        GameObject[] objects = Selection.gameObjects;

        foreach (var item in objects)
        {
            float actualRotX = item.transform.localRotation.eulerAngles.x;
            float actualRotY = item.transform.localRotation.eulerAngles.y;
            float actualRotz = item.transform.localRotation.eulerAngles.z;

            float newRotY = Random.Range(MinRotY, MaxRotY);
            item.transform.rotation = Quaternion.Euler(actualRotX, newRotY, actualRotz);
        }
    }
}
