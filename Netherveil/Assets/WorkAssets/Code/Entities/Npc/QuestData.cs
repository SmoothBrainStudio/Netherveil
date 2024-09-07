using System;
using UnityEngine;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.
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
