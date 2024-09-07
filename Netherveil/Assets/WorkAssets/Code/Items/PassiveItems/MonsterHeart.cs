//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class MonsterHeart : ItemEffect, IPassiveItem
{
    private readonly int maxLifeStat = 10;

    public void OnRemove()
    {
        Utilities.Hero.Stats.DecreaseMaxValue(Stat.HP, maxLifeStat);
        Utilities.Hero.Stats.DecreaseValue(Stat.HP, maxLifeStat, true);
    }

    public void OnRetrieved()  
    {
        Utilities.Hero.Stats.IncreaseMaxValue(Stat.HP, maxLifeStat);
        Utilities.Hero.Stats.IncreaseValue(Stat.HP, maxLifeStat, true);
    }
}
