using DialogueSystem.Runtime;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class RootNodeView : NodeView
    {
        public override Type type => typeof(RootNode);

        public RootNodeView(GraphView graphView) 
            : base("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/RootNodeView.uxml", graphView)
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/RootNodeView.uss"));

            title = "Root";
            AddPort(Direction.Output, Port.Capacity.Single, "next dialogue");

            capabilities &= ~Capabilities.Deletable;
            
            RefreshExpandedState();
            RefreshPorts();

            SetPosition(new Rect(0, 0, 100, 150));
        }
    }
}