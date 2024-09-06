using UnityEngine; 
 
public class UrnOfAvarice : ItemEffect , IPassiveItem 
{
    readonly float bloodDamagesCoef = 0.005f;
    readonly float bloodDamagesMaxCoef = 0.5f;
    float previousBloodCoef = 0f;

    public void OnRetrieved() 
    {
        Inventory.OnAddOrRemoveBlood += AddBloodDamages;
    } 
 
    public void OnRemove() 
    {
        Inventory.OnAddOrRemoveBlood -= AddBloodDamages;
    }

    private void AddBloodDamages()
    {
        Utilities.Hero.Stats.DecreaseCoeffValue(Stat.ATK, previousBloodCoef);
        float newBloodCoef = Mathf.Min(Utilities.Hero.Inventory.Blood.Value * bloodDamagesCoef, bloodDamagesMaxCoef);
        Utilities.Hero.Stats.IncreaseCoeffValue(Stat.ATK, newBloodCoef);
        previousBloodCoef = newBloodCoef;
    }
} 
