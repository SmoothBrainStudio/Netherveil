using UnityEngine;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class RuneOfGluttony : ItemEffect, IPassiveItem
{
    public void OnRemove()
    {
        Utilities.Hero.Stats.DivideValue(Stat.HEAL_COEFF, 2f, false);
    }

    public void OnRetrieved() 
    {
        Utilities.Hero.Stats.MultiplyValue(Stat.HEAL_COEFF, 2f, false);
    } 
} 
