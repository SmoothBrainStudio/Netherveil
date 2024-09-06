using Fountain;
using UnityEngine;

public class MerchantCounterInteraction : MonoBehaviour, IInterractable
{
    private MerchantCounter merchantCounter;
    private MerchantCounterDisplay display;
    private Hero hero;
    private PlayerInteractions interactions;
    private Outline outline;

    private bool isSelect = false;

    private void Start()
    {
        merchantCounter = GetComponent<MerchantCounter>();
        display = GetComponent<MerchantCounterDisplay>();
        hero = FindObjectOfType<Hero>();
        interactions = hero.GetComponent<PlayerInteractions>();
        outline = GetComponent<Outline>();
    }

    private void Update()
    {
        DetectInterctable();
    }

    private void DetectInterctable()
    {
        bool isInRange = Vector2.Distance(interactions.transform.position.ToCameraOrientedVec2(), transform.position.ToCameraOrientedVec2()) <= hero.Stats.GetValue(Stat.CATCH_RADIUS);

        if (isInRange && !interactions.InteractablesInRange.Contains(this))
        {
            interactions.InteractablesInRange.Add(this);
        }
        else if (!isInRange && interactions.InteractablesInRange.Contains(this))
        {
            interactions.InteractablesInRange.Remove(this);
            Deselect();
        }
    }

    public void Deselect()
    {
        if (!isSelect)
            return;

        isSelect = false;
        display.Undisplay();
        outline.DisableOutline();
    }

    public void Select()
    {
        if (isSelect)
            return;

        isSelect = true;
        display.Display();
        outline.EnableOutline();
    }

    public void Interract()
    {
        int price = merchantCounter.BloodPrice;
        int trade = merchantCounter.ValueTrade;

        if (price > hero.Inventory.Blood.Value || merchantCounter.NbPurchases >= merchantCounter.MAX_PURCHASE)
            return;

        merchantCounter.BloodPrice += merchantCounter.PURCHASE_STEP_AUGMENTATION;
        merchantCounter.NbPurchases++;
        hero.Inventory.Blood -= price;
        hero.HealConsumable(trade);
    }
}
