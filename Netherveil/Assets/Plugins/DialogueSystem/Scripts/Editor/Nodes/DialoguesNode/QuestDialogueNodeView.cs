using DialogueSystem.Runtime;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class QuestDialogueNodeView : DialogueNodeView
    {
        public override Type type => typeof(QuestDialogueNode);

        protected string questTag;
        public string QuestTag
        {
            get => questTag;
            set
            {
                questTag = value;
                questTagField.value = value;
            }
        }

        private QuestDialogueDifficulty difficulty;
        public QuestDialogueDifficulty Difficulty
        {
            get => difficulty;
            set
            {
                difficulty = value;
                difficultyField.value = value;
            }
        }

        private TextField questTagField;
        private EnumField difficultyField;

        public QuestDialogueNodeView(GraphView graphView)
            : base("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/QuestDialogueNodeView.uxml", graphView)
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/QuestDialogueNodeView.uss"));
            AddPort(Direction.Input, Port.Capacity.Single, "previous dialogue");
            AddPort(Direction.Output, Port.Capacity.Single, "next dialogue");
            title = "Quest Dialogue";

            // Querry
            questTagField = this.Q<TextField>("QuestTag-field");
            questTagField.RegisterValueChangedCallback(evt => QuestTag = evt.newValue);

            difficultyField = this.Q<EnumField>("Difficulty-field");
            difficultyField.RegisterValueChangedCallback(evt => Difficulty = (QuestDialogueDifficulty)evt.newValue);
        }
    }
}
