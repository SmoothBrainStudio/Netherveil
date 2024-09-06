using UnityEngine;

public class RuneOfWrath : ItemEffect, IPassiveItem
{
    private readonly float AttackCoeffStat = 0.5f;

    public void OnRemove()
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.DecreaseCoeffValue(Stat.ATK, AttackCoeffStat);
    }

    public void OnRetrieved()
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.IncreaseCoeffValue(Stat.ATK, AttackCoeffStat);
    }
}
