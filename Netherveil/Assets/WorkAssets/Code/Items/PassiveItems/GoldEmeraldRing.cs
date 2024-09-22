using UnityEngine;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class GoldEmeraldRing : ItemEffect, IPassiveItem
{
    private readonly int attackStat = 2;
    private readonly int healthIncrease = 25;
    public void OnRemove()
    {
        Hero player = Utilities.Hero;
        player.Stats.DecreaseValue(Stat.ATK, attackStat, false);
        player.Stats.DecreaseMaxValue(Stat.HP, healthIncrease);
        player.Stats.DecreaseValue(Stat.HP, healthIncrease);
        if(player.Stats.GetValue(Stat.HP) <= 0)
        {
            player.Stats.SetValue(Stat.HP, 1);
        }
    }

    public void OnRetrieved()
    {
        Hero player = Utilities.Hero;
        player.Stats.IncreaseValue(Stat.ATK, attackStat, false);
        player.Stats.IncreaseMaxValue(Stat.HP, healthIncrease);
        player.Stats.IncreaseValue(Stat.HP, healthIncrease);
    }
}
