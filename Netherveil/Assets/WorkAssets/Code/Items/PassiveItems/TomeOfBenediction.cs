//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class TomeOfBenediction : ItemEffect , IPassiveItem 
{
    readonly int value = 25;
    public void OnRetrieved() 
    {
        Utilities.Hero.Stats.DecreaseValue(Stat.CORRUPTION, value);
        Hero.CallCorruptionBenedictionText(-value);
    } 
 
    public void OnRemove() 
    {
        Utilities.Hero.Stats.IncreaseValue(Stat.CORRUPTION, value);
    }
} 
