using System;
using UnityEngine;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
[Serializable]
public class Bramble : ItemEffect, IPassiveItem
{
    private readonly int attackStat = 5;

    public void OnRemove()
    {
        Utilities.Hero.Stats.DecreaseValue(Stat.ATK, attackStat, false);
    }

    public void OnRetrieved()
    {
        Utilities.Hero.Stats.IncreaseValue(Stat.ATK, attackStat, false);
    }
}
