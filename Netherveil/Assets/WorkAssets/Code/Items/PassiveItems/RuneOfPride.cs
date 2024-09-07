using Map;
using UnityEngine;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class RuneOfPride : ItemEffect, IPassiveItem
{
    [SerializeField] private int maxBoost = 4;
    [SerializeField] private int boostValue = 2;
    private readonly int MAX_BOOST;
    private int nbBoost = 0;

    public RuneOfPride()
    {
        MAX_BOOST = maxBoost * boostValue;
    }

    public void OnRemove()
    {
        Utilities.Hero.OnKill -= Berserk;
        MapUtilities.onExitRoom -= Reset;
    }

    public void OnRetrieved()
    {
        Utilities.Hero.OnKill += Berserk;
        MapUtilities.onExitRoom += Reset;
    }

    private void Berserk(IDamageable damageable)
    {
        if (nbBoost * boostValue > MAX_BOOST) return;
        Utilities.Hero.Stats.IncreaseValue(Stat.ATK, boostValue, false);
        AudioManager.Instance.PlaySound(AudioManager.Instance.RuneOfPrideSFX);
        nbBoost++;
        if (nbBoost == 1)
        {
            Utilities.PlayerController.RuneOfPrideVFX.Play();
        }
    }

    private void Reset()
    {
        Utilities.Hero.Stats.DecreaseValue(Stat.ATK, boostValue * nbBoost, false);
        nbBoost = 0;
        Utilities.PlayerController.RuneOfPrideVFX.Stop();
    }
}
