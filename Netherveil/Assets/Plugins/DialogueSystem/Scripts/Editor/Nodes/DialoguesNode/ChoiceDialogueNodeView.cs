using DialogueSystem.Runtime;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class ChoiceDialogueNodeView : DialogueNodeView
    {
        public override Type type => typeof(ChoiceDialogueNode);

        public ChoiceDialogueNodeView(GraphView graphView)
            : base("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/ChoiceDialogueNodeView.uxml", graphView)
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/DialogueSystem/Scripts/Editor/UIBuilder/ChoiceDialogueNodeView.uss"));
            AddPort(Direction.Input, Port.Capacity.Single, "previous dialogue");
            title = "Choices Dialogue";

            // Querry
            var button = this.Q<Button>("button-new-choice");
            button.clickable.clicked += () => AddChoicePort();
        }

        public void AddChoicePort(string overridenPortName = "")
        {
            var generatedPort = AddPort(Direction.Output);

            var oldLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(oldLabel);

            var outputPortCount = outputContainer.Query("connector").ToList().Count;
            generatedPort.portName = $"Choice {outputPortCount}";

            var choicePortName = string.IsNullOrEmpty(overridenPortName)
                ? $"Choice {outputPortCount}"
                : overridenPortName;

            var textField = new TextField
            {
                name = string.Empty,
                value = choicePortName,
            };
            textField.style.minWidth = 60;
            textField.style.maxWidth = 100;
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(textField);
            var deleteButton = new Button(() => RemovePort(graphView, generatedPort))
            {
                text = "X",
            };
            generatedPort.contentContainer.Add(deleteButton);

            generatedPort.portName = choicePortName;
            outputContainer.Add(generatedPort);
            RefreshExpandedState();
            RefreshPorts();
        }

        private void RemovePort(GraphView graphView, Port generatedPort)
        {
            var targetEdge = graphView.edges.ToList()
                .Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                graphView.RemoveElement(targetEdge.First());
            }

            outputContainer.Remove(generatedPort);
            RefreshPorts();
            RefreshExpandedState();
        }
    }
}
