using UnityEngine; 
 
public class LockPickingKit : ItemEffect , IPassiveItem 
{ 
    public void OnRetrieved() 
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().canTriggerTraps = false; 
    } 
 
    public void OnRemove() 
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().canTriggerTraps = true;
    } 
 
} 
