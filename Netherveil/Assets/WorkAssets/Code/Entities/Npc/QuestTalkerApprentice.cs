using DialogueSystem.Runtime;

public class QuestTalkerApprentice : QuestTalker
{
    protected override void StartDialogue()
    {
        DialogueTree dialogue = questDT;

        if (player.CurrentQuest != null)
        {
            dialogue = alreadyHaveQuestDT;
        }
        else if (Utilities.Hero.DoneQuestQTApprenticeThisStage)
        {
            dialogue = alreadyDoneQuestDT;
        }

        dialogueTreeRunner.StartDialogue(dialogue, this);
    }
}
