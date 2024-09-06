using DialogueSystem.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueDataEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private TextField nameField;
    private TextField dialogueField;
    private ObjectField illustrationField;

    [MenuItem("Tools/Dialogue System/Dialogue Data Editor")]
    public static void ShowExample()
    {
        DialogueDataEditor wnd = GetWindow<DialogueDataEditor>();
        wnd.titleContent = new GUIContent("Dialogue Data Editor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        // Querry
        var saveButton = root.Q<Button>("SaveAsset-button");
        saveButton.clicked += () => Save();

        var loadButton = root.Q<Button>("LoadAsset-button");
        loadButton.clicked += () => Load();

        nameField = root.Q<TextField>("Name-field");
        dialogueField = root.Q<TextField>("Dialogue-field");
        illustrationField = root.Q<ObjectField>("Illustration-field");
    }

    public void Save()
    {
        var dialogueContainer = CreateInstance<DialogueData>();
        dialogueContainer.name = nameField.text;
        dialogueContainer.dialogue = dialogueField.text;
        dialogueContainer.illustration = illustrationField.value as Sprite;

        string path = EditorUtility.SaveFilePanelInProject("Save dialogue data", "new Dialogue Data", "asset",
            "Please enter a file name to save your Dialogue Data");

        AssetDatabase.CreateAsset(dialogueContainer, path);
        AssetDatabase.SaveAssets();
    }

    public void Load()
    {
        string path = EditorUtility.OpenFilePanel("Load Dialogue Data", Application.dataPath, "asset");

        string assetPath = "Assets" + path.Replace(Application.dataPath, "");
        DialogueData data = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);

        if (data == null)
        {
            EditorUtility.DisplayDialog("File not Found", "Target dialogue graph file does not exists!", "OK");
            return;
        }

        nameField.value = data.name;
        dialogueField.value = data.dialogue;
        illustrationField.value = data.illustration;
    }
}
