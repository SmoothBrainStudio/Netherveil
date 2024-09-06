using FMODUnity;
using UnityEngine;

public class HealPotion : Consumable
{
    [SerializeField] int healValue;
    [SerializeField] int price;

    protected override void Start()
    {
        base.Start();
        Price = price;
    }

    public override void OnRetrieved()
    {
        Utilities.Hero.HealConsumable(healValue);
        Destroy(this.gameObject);
    }
}
