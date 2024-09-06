using DialogueSystem.Runtime;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class DialogueTreeEditor : EditorWindow
    {
        [SerializeField]
        //private VisualTreeAsset m_VisualTreeAsset = default;
        private DialogueTreeView graphView;
        public string assetPath;

        [OnOpenAsset]
        public static bool ShowWindow(int instanceID, int line)
        {
            string assetPath = AssetDatabase.GetAssetPath(instanceID);
            DialogueTree tree = AssetDatabase.LoadAssetAtPath<DialogueTree>(assetPath);

            if (tree != null)
            {
                string assetName = Path.GetFileNameWithoutExtension(assetPath);

                Object[] windows = Resources.FindObjectsOfTypeAll(typeof(DialogueTreeEditor));

                foreach (var window in windows)
                {
                    DialogueTreeEditor dialogueTreeEditor = window as DialogueTreeEditor;
                    if (dialogueTreeEditor.assetPath == assetPath)
                    {
                        dialogueTreeEditor.Focus();
                        return true;
                    }
                }

                DialogueTreeEditor wnd = CreateWindow<DialogueTreeEditor>();
                wnd.titleContent = new GUIContent(assetName + " (Dialogue Tree Editor)");
                wnd.assetPath = assetPath;
                wnd.graphView.PopulateTree(tree);

                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/DialogueTreeEditor.uxml");
            visualTree.CloneTree(root);

            // Instantiate UXML
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/DialogueTreeEditor.uss");
            root.styleSheets.Add(styleSheet);

            // Querry
            graphView = root.Q<DialogueTreeView>();
            graphView.AddSearchWindow(this);

            ToolbarButton saveAssetButton = root.Q<ToolbarButton>("SaveAsset-button");
            saveAssetButton.clicked += (() => {
                var saveUtility = GraphSaveUtility.GetInstance(graphView);
                saveUtility.SaveGraph(assetPath);
            });

            ToolbarButton saveAsButton = root.Q<ToolbarButton>("SaveAs-button");
            saveAsButton.clicked += (() => {
                var saveUtility = GraphSaveUtility.GetInstance(graphView);
                saveUtility.SaveGraph();
            });
        }
    }
}
