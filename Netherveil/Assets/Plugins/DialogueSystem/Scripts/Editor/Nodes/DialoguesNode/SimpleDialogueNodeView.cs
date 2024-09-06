using DialogueSystem.Runtime;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class SimpleDialogueNodeView : DialogueNodeView
    {
        public override Type type => typeof(SimpleDialogueNode);

        public SimpleDialogueNodeView(GraphView graphView)
            : base("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/SimpleDialogueNodeView.uxml", graphView)
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/SimpleDialogueNodeView.uss"));
            AddPort(Direction.Input, Port.Capacity.Single, "previous dialogue");
            AddPort(Direction.Output, Port.Capacity.Single, "next dialogue");
            title = "Simple Dialogue";
        }
    }
}
