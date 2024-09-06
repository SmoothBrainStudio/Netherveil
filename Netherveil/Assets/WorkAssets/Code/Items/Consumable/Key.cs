using UnityEngine;

public class Key : Consumable
{
    public override void OnRetrieved()
    {
        GameObject.FindWithTag("Player").GetComponent<Hero>().Inventory.Keys++;
        Destroy(gameObject);
    }
}
