using UnityEngine;

namespace DialogueSystem.Runtime
{
    public abstract class Node : ScriptableObject
    {
#if UNITY_EDITOR
        [HideInInspector] public string GUID;
        [HideInInspector] public Vector2 position;
#endif
    }
}
