using System.Collections.Generic;

namespace DialogueSystem.Runtime
{
    public class ChoiceDialogueNode : DialogueNode
    {
        [System.Serializable]
        public class Option
        {
            public Option(string option, Node child)
            {
                this.option = option;
                this.child = child;
            }

            public string option;
            public Node child;
        }

        public List<Option> options = new List<Option>();

        public void AddOption(string optionText, Node childNode)
        {
            Option newOption = new Option(optionText, childNode);
            options.Add(newOption);
        }
    }
}
