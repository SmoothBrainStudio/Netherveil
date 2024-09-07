//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class BloodCrown : ItemEffect , IPassiveItem 
{ 
    public void OnRetrieved() 
    {
        BloodDrop.BloodDropCoeff += 1.0f;
    } 
 
    public void OnRemove() 
    { 
        BloodDrop.BloodDropCoeff -= 1.0f;
    } 
 
} 
