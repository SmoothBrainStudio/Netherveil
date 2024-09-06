using DialogueSystem.Runtime;
using System.Linq;
using UnityEngine;

public class Talker : Npc
{
    private enum TalkerMethod
    {
        Random,
        Ascendant,
        Descendant
    }

    [Header("Talker parameters")]
    [SerializeField] private TalkerMethod method = TalkerMethod.Ascendant;
    [SerializeField] protected DialogueTree[] dialogues;
    protected DialogueTreeRunner dialogueTreeRunner;
    private int currentDialogue = 0;

    protected override void Start()
    {
        base.Start();
        dialogueTreeRunner = FindObjectOfType<DialogueTreeRunner>();
    }

    public override void Interract()
    {
        TriggerDialogue();
    }

    private void TriggerDialogue()
    {
        if (!dialogueTreeRunner.IsStarted)
        {
            StartDialogue();
        }
    }

    protected virtual void StartDialogue()
    {
        if (!dialogues.Any())
            return;
        
        DialogueTree dialogueToRun = null;

        switch (method)
        {
            case TalkerMethod.Random:
                dialogueToRun = dialogues[Random.Range(0, dialogues.Length)];
                break;
            case TalkerMethod.Ascendant:
                dialogueToRun = dialogues[currentDialogue++];
                currentDialogue %= dialogues.Length;
                break;
            case TalkerMethod.Descendant:
                dialogueToRun = dialogues[currentDialogue--];
                currentDialogue = currentDialogue < 0 ? dialogues.Length - 1 : currentDialogue;
                break;
        }

        dialogueTreeRunner.StartDialogue(dialogueToRun, this);
    }
}
