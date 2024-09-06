using UnityEngine; 
 
public class KnockbackShield : ItemEffect , IPassiveItem 
{ 
    public void OnRetrieved() 
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().IsKnockbackable = false;
    } 
 
    public void OnRemove() 
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().IsKnockbackable = true;
    } 
 
} 
