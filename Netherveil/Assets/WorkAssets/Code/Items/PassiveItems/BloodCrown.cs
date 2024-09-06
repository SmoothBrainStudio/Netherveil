
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
