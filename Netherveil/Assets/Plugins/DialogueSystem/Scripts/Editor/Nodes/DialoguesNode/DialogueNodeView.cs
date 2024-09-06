using DialogueSystem.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public abstract class DialogueNodeView : NodeView
    {
        protected DialogueData dialogueData;
        public DialogueData DialogueData
        {
            get => dialogueData;
            set
            {
                dialogueData = value;
                dialogueDataField.value = value;
            }
        }

        private ObjectField dialogueDataField;

        public DialogueNodeView(string uiFile, GraphView graphView)
            : base(uiFile, graphView)
        {
            // Querry
            dialogueDataField = this.Q<ObjectField>("DialogueData-field");
            dialogueDataField.RegisterValueChangedCallback(evt => DialogueData = evt.newValue as DialogueData);
        }
    }
}
