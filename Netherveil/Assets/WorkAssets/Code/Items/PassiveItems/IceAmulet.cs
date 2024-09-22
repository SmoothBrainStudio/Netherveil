using UnityEngine; 
 
public class IceAmulet : ItemEffect , IPassiveItem 
{
    private readonly float iceChance = 0.1f;
    private readonly float iceDuration = 2.0f;
    int indexInStatus = 0;
    public void OnRetrieved()
    {
        Hero hero = Utilities.Hero;
        indexInStatus = hero.StatusToApply.Count;
        hero.StatusToApply.Add(new Freeze(iceDuration, iceChance));
    }

    public void OnRemove()
    {
        Utilities.Hero.StatusToApply.RemoveAt(indexInStatus);
    }

} 
