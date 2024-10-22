using UnityEngine;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class BleedingRing : ItemEffect, IPassiveItem 
{
    private readonly float bleedingChance = 0.1f;
    private readonly float bleedingDuration = 2.0f;
    int indexInStatus = 0;
    public void OnRetrieved()
    {
        indexInStatus = Utilities.Hero.StatusToApply.Count;
        Utilities.Hero.StatusToApply.Add(new Bleeding(bleedingDuration, bleedingChance));
    }

    public void OnRemove()
    {
        Utilities.Hero.StatusToApply.RemoveAt(indexInStatus);
    }
} 
