//Copyright 2024 Property of Olivier Maurin.All rights reserved.
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
