using Map;
using System.Collections.Generic;
using UnityEngine;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class HornOfBarbatos : ItemEffect, IActiveItem
{
    private IActiveItem activeItem { get => this; }
    public float Cooldown { get; set; } = 1f;
    public bool TimeBased { get; set; } = false;
#pragma warning disable IDE0052 // Supprimer les membres privés non lus
    private readonly float increaseValue = 0.2f;
    private readonly float displayValue;
#pragma warning restore IDE0052 // Supprimer les membres privés non lus
    readonly List<float> changesList = new List<float>();
    private readonly List<Stat> statToChange = new List<Stat>()
    {
        Stat.ATK,
        Stat.SPEED,
        Stat.ATK_RANGE
    };

    bool itemActivatedThisRoom = false;

    public HornOfBarbatos()
    {
        displayValue = Cooldown;
    }

    public void OnRetrieved()
    {
        MapUtilities.onExitRoom += ResetStat;
        MapUtilities.onExitRoom += activeItem.WaitToUseRoom;
    }

    public void OnRemove()
    {
        MapUtilities.onExitRoom -= ResetStat;
        MapUtilities.onExitRoom -= activeItem.WaitToUseRoom;
    }

    public void Activate()
    {
        if (itemActivatedThisRoom)
            return;

        Camera.main.GetComponent<CameraUtilities>().ShakeCamera(0.3f, 0.25f, EasingFunctions.EaseInQuint);
        AudioManager.Instance.PlaySound(AudioManager.Instance.HornOfBarbatosSFX);
        //add sfx here

        Hero hero = Utilities.Hero;
        foreach (var stat in hero.Stats.StatsName)
        {
            if (statToChange.Contains(stat))
            {
                float change = hero.Stats.GetCoeff(stat);
                change = change * increaseValue;
                changesList.Add(change);
                hero.Stats.IncreaseCoeffValue(stat, change);
            }
        }
        itemActivatedThisRoom = true;
    }

    private void ResetStat()
    {
        if (itemActivatedThisRoom)
        {
            Hero hero = Utilities.Hero;
            int i = 0;
            foreach (var stat in hero.Stats.StatsName)
            {
                if (statToChange.Contains(stat))
                {
                    hero.Stats.DecreaseCoeffValue(stat, changesList[i]);
                    i++;
                }
            }
        }
        changesList.Clear();
        itemActivatedThisRoom = false;
    }
}
