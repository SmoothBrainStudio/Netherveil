using UnityEngine;

namespace DialogueSystem.Runtime
{
    public abstract class DialogueNode : Node
    {
        [Header("Dialogue parameters")]
        public DialogueData dialogueData;
    }
}
