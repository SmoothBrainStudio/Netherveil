using UnityEngine; 
 
public class ThunderAmulet : ItemEffect , IPassiveItem 
{
    private readonly float electricityChance = 0.1f;
    private readonly float electricityDuration = 6.0f;
    int indexInStatus = 0;
    public void OnRetrieved()
    {
        Hero hero = Utilities.Hero;
        indexInStatus = hero.StatusToApply.Count;
        hero.StatusToApply.Add(new Electricity(electricityDuration, electricityChance));
    }

    public void OnRemove()
    {
        Utilities.Hero.StatusToApply.RemoveAt(indexInStatus);
    }

} 
