using DialogueSystem.Runtime;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class EventDialogueNodeView : DialogueNodeView
    {
        public override Type type => typeof(EventDialogueNode);

        protected string eventTag;
        public string EventTag
        {
            get => eventTag;
            set
            {
                eventTag = value;
                eventTagField.value = value;
            }
        }

        private TextField eventTagField;

        public EventDialogueNodeView(GraphView graphView)
            : base("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/EventDialogueNodeView.uxml", graphView)
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/EventDialogueNodeView.uss"));
            AddPort(Direction.Input, Port.Capacity.Single, "previous dialogue");
            AddPort(Direction.Output, Port.Capacity.Single, "next dialogue");
            title = "Event Dialogue";

            // Querry
            eventTagField = this.Q<TextField>("EventTag-field");
            eventTagField.RegisterValueChangedCallback(evt => EventTag = evt.newValue);
        }
    }
}
