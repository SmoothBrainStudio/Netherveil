using UnityEngine; 
 
public class PoisonAmulet : ItemEffect, IPassiveItem 
{
    private readonly float poisonChance = 0.1f;
    private readonly float poisonDuration = 2.0f;
    int indexInStatus = 0;
    public void OnRetrieved()
    {
        Hero hero = GameObject.FindWithTag("Player").GetComponent<Hero>();
        indexInStatus = hero.StatusToApply.Count;
        hero.StatusToApply.Add(new Poison(poisonDuration, poisonChance));
    }

    public void OnRemove()
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().StatusToApply.RemoveAt(indexInStatus);
    }
} 
