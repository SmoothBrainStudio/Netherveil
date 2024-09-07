using DialogueSystem.Runtime;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.
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
