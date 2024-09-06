using DialogueSystem.Runtime;
using UnityEngine;

public class Merchant : Npc, IInterractable
{
    [Header("Merchand parameters")]
    [SerializeField] private DialogueTree dialogue;

    public override void Interract()
    {
        TriggerDialogue();
    }

    private void TriggerDialogue()
    {
        FindObjectOfType<DialogueTreeRunner>().StartDialogue(dialogue, this);
    }
}
