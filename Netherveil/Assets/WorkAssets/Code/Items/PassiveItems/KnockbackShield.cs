//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class KnockbackShield : ItemEffect , IPassiveItem 
{ 
    public void OnRetrieved() 
    {
        Utilities.Hero.IsKnockbackable = false;
    } 
 
    public void OnRemove() 
    {
        Utilities.Hero.IsKnockbackable = true;
    } 
 
} 
