using UnityEngine;

public class BloodDrop : Consumable
{
    public static float BloodDropCoeff = 1.0f;
    [SerializeField] int bloodQuantity = 0;

    public override void OnRetrieved()
    {
        player.Inventory.Blood += (int)(bloodQuantity * BloodDropCoeff);
        Destroy(gameObject);
    }
}
