using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueTreeView graphView;
        private EditorWindow window;
        private Texture2D indentationIcon;

        public void Init(EditorWindow window, DialogueTreeView graphView)
        {
            this.window = window;
            this.graphView = graphView;

            // Intendation hack for search window as a transparent icon
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
                new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),

                new SearchTreeEntry(new GUIContent("Simple Dialogue Node", indentationIcon))
                {
                    userData = new SimpleDialogueNodeView(graphView),
                    level = 2
                },

                new SearchTreeEntry(new GUIContent("Choice Dialogue Node", indentationIcon))
                {
                    userData = new ChoiceDialogueNodeView(graphView),
                    level = 2
                },

                new SearchTreeEntry(new GUIContent("Event Dialogue Node", indentationIcon))
                {
                    userData = new EventDialogueNodeView(graphView),
                    level = 2
                },

                new SearchTreeEntry(new GUIContent("Quest Dialogue Node", indentationIcon))
                {
                    userData = new QuestDialogueNodeView(graphView),
                    level = 2
                },
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent,
                context.screenMousePosition - window.position.position);
            var localMousePosition = graphView.contentViewContainer.WorldToLocal(worldMousePosition);

            switch (SearchTreeEntry.userData)
            {
                case SimpleDialogueNodeView:
                    graphView.CreateNode(typeof(SimpleDialogueNodeView), localMousePosition);
                    return true;
                case ChoiceDialogueNodeView:
                    graphView.CreateNode(typeof(ChoiceDialogueNodeView), localMousePosition);
                    return true;
                case EventDialogueNodeView:
                    graphView.CreateNode(typeof(EventDialogueNodeView), localMousePosition);
                    return true;
                case QuestDialogueNodeView:
                    graphView.CreateNode(typeof(QuestDialogueNodeView), localMousePosition);
                    return true;
                default:
                    return false;
            }
        }
    }
}
