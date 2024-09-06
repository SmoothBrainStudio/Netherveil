using UnityEngine;

public class TomeOfCorruption : ItemEffect , IPassiveItem 
{
    readonly int value = 25;
    public void OnRetrieved()
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().Stats.IncreaseValue(Stat.CORRUPTION, value);
        Hero.CallCorruptionBenedictionText(value);
    }

    public void OnRemove()
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().Stats.DecreaseValue(Stat.CORRUPTION, value);
    }
} 
