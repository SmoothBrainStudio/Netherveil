using UnityEngine;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class BootOfSwiftness : ItemEffect, IPassiveItem
{
    private readonly float speedStat = 1.5f;

    public void OnRemove()
    {
        Utilities.Hero.Stats.DecreaseValue(Stat.SPEED, speedStat, false);
    }

    public void OnRetrieved()
    {
        Utilities.Hero.Stats.IncreaseValue(Stat.SPEED, speedStat, false);
    }
}
