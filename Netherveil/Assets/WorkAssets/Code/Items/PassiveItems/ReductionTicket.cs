using UnityEngine; 
 
public class ReductionTicket : ItemEffect , IPassiveItem 
{
    readonly float coefValue = 0.5f;

    public void OnRetrieved() 
    {
        Item.PriceCoef *= coefValue;
    }

    public void OnRemove() 
    {
        Item.PriceCoef /= coefValue;
    } 
 
} 
