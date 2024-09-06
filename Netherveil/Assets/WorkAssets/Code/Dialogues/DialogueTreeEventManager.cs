using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTreeEventManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueEvent
    {
        public DialogueEvent(string tag, UnityAction action)
        {
            this.tag = tag;
            onCall.AddListener(action);
        }

        public string tag;
        public UnityEvent onCall = new();
    }

    [SerializeField] private List<DialogueEvent> dialogueEvent = new List<DialogueEvent>();

    public void Invoke(string tag)
    {
        dialogueEvent.Where(x => x.tag == tag).ToList().ForEach(x =>
        {
            x.onCall.Invoke();
        });
    }

    public void AddListener(string tag, UnityAction action)
    {
        dialogueEvent.Add(new DialogueEvent(tag, action));
    }

    public void RemoveListener(string tag)
    {
        if (dialogueEvent.FirstOrDefault(x => x.tag == tag) == null)
            return;

        dialogueEvent.Where(x => x.tag == tag).ToList().ForEach(x =>
        {
            dialogueEvent.Remove(x);
        });
    }
}
