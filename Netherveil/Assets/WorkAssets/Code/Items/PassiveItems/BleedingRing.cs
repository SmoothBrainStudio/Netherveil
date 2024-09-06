using UnityEngine; 
 
public class BleedingRing : ItemEffect, IPassiveItem 
{
    private readonly float bleedingChance = 0.1f;
    private readonly float bleedingDuration = 2.0f;
    int indexInStatus = 0;
    public void OnRetrieved()
    {
        Hero hero = GameObject.FindWithTag("Player").GetComponent<Hero>();
        indexInStatus = hero.StatusToApply.Count;
        hero.StatusToApply.Add(new Bleeding(bleedingDuration, bleedingChance));
    }

    public void OnRemove()
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().StatusToApply.RemoveAt(indexInStatus);
    }
} 
