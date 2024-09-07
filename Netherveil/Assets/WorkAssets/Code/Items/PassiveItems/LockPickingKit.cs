//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class LockPickingKit : ItemEffect , IPassiveItem 
{ 
    public void OnRetrieved() 
    {
        Utilities.Hero.canTriggerTraps = false; 
    } 
 
    public void OnRemove() 
    {
        Utilities.Hero.canTriggerTraps = true;
    } 
 
} 
