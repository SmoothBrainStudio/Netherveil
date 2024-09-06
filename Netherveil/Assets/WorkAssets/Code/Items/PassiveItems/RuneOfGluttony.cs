using UnityEngine;

public class RuneOfGluttony : ItemEffect, IPassiveItem
{
    public void OnRemove()
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.DivideValue(Stat.HEAL_COEFF, 2f, false);
    }

    public void OnRetrieved() 
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.MultiplyValue(Stat.HEAL_COEFF, 2f, false);
    } 
} 
