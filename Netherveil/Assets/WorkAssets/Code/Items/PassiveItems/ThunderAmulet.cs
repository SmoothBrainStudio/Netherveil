using UnityEngine; 
 
public class ThunderAmulet : ItemEffect , IPassiveItem 
{
    private readonly float electricityChance = 0.1f;
    private readonly float electricityDuration = 6.0f;
    int indexInStatus = 0;
    public void OnRetrieved()
    {
        Hero hero = GameObject.FindWithTag("Player").GetComponent<Hero>();
        indexInStatus = hero.StatusToApply.Count;
        hero.StatusToApply.Add(new Electricity(electricityDuration, electricityChance));
    }

    public void OnRemove()
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().StatusToApply.RemoveAt(indexInStatus);
    }

} 
