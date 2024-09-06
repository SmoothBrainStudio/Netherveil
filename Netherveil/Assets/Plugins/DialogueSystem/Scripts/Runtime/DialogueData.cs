using UnityEngine;

namespace DialogueSystem.Runtime
{
    public class DialogueData : ScriptableObject
    {
        public new string name = null;
        [TextArea(3, 10)] public string dialogue = null;
        public Sprite illustration = null;
    }
}