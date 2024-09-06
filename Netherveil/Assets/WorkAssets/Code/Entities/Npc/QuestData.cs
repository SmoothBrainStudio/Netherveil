using System;
using UnityEngine;

[Serializable]
public class QuestData
{
    public enum QuestType
    {
        RESTRICTIVE,
        EXPLORATION,
        FIGHTING,
        SURVIVABILITY
    }

    public QuestType Type;
    public string idName;
    [Multiline] public string Description;
    public int CorruptionModifierValue;
    public bool HasDifferentGrades;
    public bool LimitedTime;
}
