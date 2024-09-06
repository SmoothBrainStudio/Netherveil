using UnityEngine;
public class MonsterHeart : ItemEffect, IPassiveItem
{
    private readonly int maxLifeStat = 10;

    public void OnRemove()
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.DecreaseMaxValue(Stat.HP, maxLifeStat);
        player.Stats.DecreaseValue(Stat.HP, maxLifeStat, true);
    }

    public void OnRetrieved()  
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.IncreaseMaxValue(Stat.HP, maxLifeStat);
        player.Stats.IncreaseValue(Stat.HP, maxLifeStat, true);
    }
}
