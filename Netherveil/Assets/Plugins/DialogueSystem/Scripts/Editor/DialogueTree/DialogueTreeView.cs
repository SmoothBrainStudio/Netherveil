using DialogueSystem.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class DialogueTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogueTreeView, GraphView.UxmlTraits> { }
        private NodeSearchWindow searchWindow;
        public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
        public DialogueTree tree;

        public DialogueTreeView()
        {
            Insert(0, new GridBackground());

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/DialogueTreeEditor.uss");
            styleSheets.Add(styleSheet);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public void PopulateTree(DialogueTree tree)
        {
            this.tree = tree;

            DeleteElements(graphElements);

            if (tree.root == null)
            {
                CreateNode(typeof(RootNodeView), Vector2.zero);
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            var saveUtility = GraphSaveUtility.GetInstance(this);
            saveUtility.LoadGraph();
        }

        public void AddSearchWindow(EditorWindow editorWindow)
        {
            searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            searchWindow.Init(editorWindow, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        public NodeView CreateNode(System.Type type, Vector2 position)
        {
            NodeView newNode;

            if (type == typeof(RootNodeView))
            {
                newNode = new RootNodeView(this);
                newNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(newNode);
                return newNode;
            }
            if (type == typeof(SimpleDialogueNodeView))
            {
                newNode = new SimpleDialogueNodeView(this);
                newNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(newNode);
                return newNode;
            }
            if (type == typeof(ChoiceDialogueNodeView))
            {
                newNode = new ChoiceDialogueNodeView(this);
                newNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(newNode);
                return newNode;
            }
            if (type == typeof(EventDialogueNodeView))
            {
                newNode = new EventDialogueNodeView(this);
                newNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(newNode);
                return newNode;
            }
            if (type == typeof(QuestDialogueNodeView))
            {
                newNode = new QuestDialogueNodeView(this);
                newNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(newNode);
                return newNode;
            }

            return null;
        }

        public NodeView CreateNode(Runtime.Node node, Vector2 position)
        {
            RootNode root = node as RootNode;
            SimpleDialogueNode simple = node as SimpleDialogueNode;
            ChoiceDialogueNode choice = node as ChoiceDialogueNode;
            EventDialogueNode eventN = node as EventDialogueNode;
            QuestDialogueNode quest = node as QuestDialogueNode;

            if (root != null)
            {
                RootNodeView viewNode = new RootNodeView(this);
                viewNode.GUID = node.GUID;
                viewNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(viewNode);
                return viewNode;
            }
            if (simple != null)
            {
                SimpleDialogueNodeView viewNode = new SimpleDialogueNodeView(this);
                viewNode.GUID = node.GUID;
                viewNode.DialogueData = simple.dialogueData;
                viewNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(viewNode);
                return viewNode;
            }
            if (choice != null)
            {
                ChoiceDialogueNodeView viewNode = new ChoiceDialogueNodeView(this);
                viewNode.GUID = node.GUID;
                viewNode.DialogueData = choice.dialogueData;
                viewNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(viewNode);
                return viewNode;
            }
            if (eventN != null)
            {
                EventDialogueNodeView viewNode = new EventDialogueNodeView(this);
                viewNode.GUID = node.GUID;
                viewNode.DialogueData = eventN.dialogueData;
                viewNode.EventTag = eventN.eventTag;
                viewNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(viewNode);
                return viewNode;
            }
            if (quest != null)
            {
                QuestDialogueNodeView viewNode = new QuestDialogueNodeView(this);
                viewNode.GUID = node.GUID;
                viewNode.DialogueData = quest.dialogueData;
                viewNode.QuestTag = quest.questTag;
                viewNode.Difficulty = quest.difficulty;
                viewNode.SetPosition(new Rect(position, defaultNodeSize));
                AddElement(viewNode);
                return viewNode;
            }

            return null;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
    }
}
