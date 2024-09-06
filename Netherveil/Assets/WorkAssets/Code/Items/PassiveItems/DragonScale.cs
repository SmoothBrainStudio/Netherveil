using UnityEngine; 
 
public class DragonScale : ItemEffect , IPassiveItem 
{
    readonly float coefValue = 0.2f;
    readonly float hpValue = 25f;

    public void OnRetrieved() 
    { 
        Hero hero = GameObject.FindWithTag("Player").GetComponent<Hero>();
        hero.Stats.IncreaseCoeffValue(Stat.ATK, coefValue);
        hero.Stats.IncreaseMaxValue(Stat.HP, hpValue);
        hero.Stats.IncreaseValue(Stat.HP, hpValue);
        if (hero.Stats.GetValue(Stat.HP) <= 0)
        {
            hero.Stats.SetValue(Stat.HP, 1);
        }
    } 
 
    public void OnRemove() 
    {
        Hero hero = GameObject.FindWithTag("Player").GetComponent<Hero>();
        hero.Stats.DecreaseCoeffValue(Stat.ATK, coefValue);
        hero.Stats.DecreaseMaxValue(Stat.HP, hpValue);
        hero.Stats.DecreaseValue(Stat.HP, hpValue);
    }
} 
