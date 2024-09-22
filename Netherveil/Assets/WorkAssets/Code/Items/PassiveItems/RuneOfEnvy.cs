using System.Collections.Generic;
using UnityEngine;
using Map;

public class RuneOfEnvy : ItemEffect, IPassiveItem
{
    readonly List<List<float>> statsStolen = new();
    readonly float stealPourcentage = 0.1f;
    bool hasStolenStats = false;

    enum StolenStats
    {
        HP,
        ATK,
        SPD,
        NB
    }

    public void OnRetrieved()
    {
        MapUtilities.onFirstEnter += StealStats;
        MapUtilities.onExitRoom += ResetStats;

        for (int i = 0; i < (int)StolenStats.NB; i++)
        {
            statsStolen.Add(new List<float>());
        }
    }

    public void OnRemove()
    {
        MapUtilities.onFirstEnter -= StealStats;
        MapUtilities.onExitRoom -= ResetStats;
    }

    private void StealStats()
    {
        Hero hero = GameObject.FindWithTag("Player").GetComponent<Hero>();

        if (MapUtilities.currentRoomData.Enemies.Count > 0)
        {
            hasStolenStats = true;
            AudioManager.Instance.PlaySound(AudioManager.Instance.RuneOfEnvySFX);
        }

        foreach (GameObject enemy in MapUtilities.currentRoomData.Enemies)
        {
            float atkStolen = 0;
            int hpStolen = 0;
            float speedStolen = 0;

            Mobs mob = enemy.GetComponent<Mobs>();
            mob.StatSuckerVFX.GetComponent<VFXStopper>().Duration = 1f;
            mob.StatSuckerVFX.GetComponent<VFXStopper>().PlayVFX();

            if (mob.Stats.HasStat(Stat.HP))
            {
                hpStolen = (int)(mob.Stats.GetMaxValue(Stat.HP) * stealPourcentage);
            }

            if (mob.Stats.HasStat(Stat.ATK))
            {
                atkStolen = mob.Stats.GetValue(Stat.ATK) * stealPourcentage;
            }

            if (mob.Stats.HasStat(Stat.SPEED))
            {
                speedStolen = mob.Stats.GetValue(Stat.SPEED) * stealPourcentage;
            }

            statsStolen[(int)StolenStats.HP].Add(hpStolen);
            statsStolen[(int)StolenStats.ATK].Add(atkStolen);
            statsStolen[(int)StolenStats.SPD].Add(speedStolen);

            hero.Stats.IncreaseMaxValue(Stat.HP, hpStolen);
            hero.Stats.IncreaseValue(Stat.HP, hpStolen);

            mob.Stats.DecreaseMaxValue(Stat.HP, hpStolen);
            mob.Stats.DecreaseValue(Stat.HP, hpStolen, false);

            hero.Stats.IncreaseValue(Stat.ATK, atkStolen, clampToMaxValue: false);
            mob.Stats.DecreaseValue(Stat.ATK, atkStolen, false);

            hero.Stats.IncreaseValue(Stat.SPEED, speedStolen, clampToMaxValue: false);
            mob.Stats.DecreaseValue(Stat.SPEED, speedStolen, false);
        }
    }

    private void ResetStats()
    {
        if (!hasStolenStats)
        {
            foreach (List<float> statsStolenList in statsStolen)
            {
                statsStolenList.Clear();
            }
            return;
        }


        Hero hero = Utilities.Hero;

        foreach (float statStolen in statsStolen[(int)StolenStats.HP])
        {
            hero.Stats.DecreaseMaxValue(Stat.HP, statStolen);
            hero.Stats.DecreaseValue(Stat.HP, statStolen, false);
        }

        //protect player from dying when decreasing bonus HP
        if (hero.Stats.GetValue(Stat.HP) <= 0)
        {
            hero.Stats.SetValue(Stat.HP, 1);
        }

        foreach (float statStolen in statsStolen[(int)StolenStats.ATK])
        {
            hero.Stats.DecreaseValue(Stat.ATK, statStolen, false, takeOverloadIntoAccount: true);
        }

        foreach (float statStolen in statsStolen[(int)StolenStats.SPD])
        {
            hero.Stats.DecreaseValue(Stat.SPEED, statStolen, false, takeOverloadIntoAccount: true);
        }

        foreach (List<float> statsStolenList in statsStolen)
        {
            statsStolenList.Clear();
        }
        hasStolenStats = false;
    }
}
