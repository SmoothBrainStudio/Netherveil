using UnityEngine;

public class MerchantCounter : MonoBehaviour
{
    [SerializeField] private int bloodPrice = 10;
    [SerializeField] private int valueTrade = 1;

    public int BloodPrice {get => bloodPrice; set => bloodPrice = value; }
    public int ValueTrade => valueTrade;
    public byte NbPurchases { get; set; } = 0;
    public readonly byte MAX_PURCHASE = 3;
    public readonly byte PURCHASE_STEP_AUGMENTATION = 10;

    public Color Color => new Color(0.62f, 0.34f, 0.76f, 1.0f);
}
