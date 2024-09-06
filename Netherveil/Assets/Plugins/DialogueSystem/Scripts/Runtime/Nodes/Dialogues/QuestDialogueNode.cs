namespace DialogueSystem.Runtime
{
    public enum QuestDialogueDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public class QuestDialogueNode : DialogueNode
    {
        public Node child;
        public string questTag;
        public QuestDialogueDifficulty difficulty;
    }
}
