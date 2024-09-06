using UnityEngine;

public class SilverAmethystRing : ItemEffect, IPassiveItem
{
    private readonly int maxLifeStat = 25;
    private readonly int speed = 1;

    public void OnRemove()
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.DecreaseMaxValue(Stat.HP, maxLifeStat);
        player.Stats.DecreaseValue(Stat.HP, maxLifeStat, true);
        player.Stats.DecreaseValue(Stat.SPEED, speed, false);
    }

    public void OnRetrieved()
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.IncreaseMaxValue(Stat.HP, maxLifeStat);
        player.Stats.IncreaseValue(Stat.HP, maxLifeStat, true);
        player.Stats.IncreaseValue(Stat.SPEED, speed, false);

    }
}
