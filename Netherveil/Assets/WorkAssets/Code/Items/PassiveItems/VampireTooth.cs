//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class VampireTooth : ItemEffect, IPassiveItem
{
    const float lifeStealStat = 0.1f;
    public void OnRetrieved()
    {
        Utilities.Hero.Stats.IncreaseValue(Stat.LIFE_STEAL, lifeStealStat);
    }
    public void OnRemove()
    {
        Utilities.Hero.Stats.DecreaseValue(Stat.LIFE_STEAL, lifeStealStat);
    }
}
