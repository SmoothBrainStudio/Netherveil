//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class RuneOfWrath : ItemEffect, IPassiveItem
{
    private readonly float AttackCoeffStat = 0.5f;

    public void OnRemove()
    {
        Utilities.Hero.Stats.DecreaseCoeffValue(Stat.ATK, AttackCoeffStat);
    }

    public void OnRetrieved()
    {
        Utilities.Hero.Stats.IncreaseCoeffValue(Stat.ATK, AttackCoeffStat);
    }
}
