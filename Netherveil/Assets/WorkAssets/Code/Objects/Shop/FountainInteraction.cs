using System;
using UnityEngine;

namespace Fountain
{
    [RequireComponent(typeof(FountainDisplay), typeof(Fountain))]
    public class FountainInteraction : MonoBehaviour, IInterractable
    {
        private Fountain fountain;
        private FountainDisplay display;
        private Hero hero;
        private PlayerInteractions interactions;
        private Outline outline;
        public static event Action onAddBenedictionCorruption;

        private bool isSelect = false;

        private void Start()
        {
            fountain = GetComponent<Fountain>();
            display = GetComponent<FountainDisplay>();
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
            if(hero.State != (int)Hero.PlayerState.MOTIONLESS)
            {
                int price = fountain.BloodPrice;
                int trade = fountain.ValueTrade;

                if (price > hero.Inventory.Blood.Value || fountain.GotMaxInAnyAlignment())
                    return;

                hero.Inventory.Blood -= price;

                if(fountain.Type == FountainType.Blessing)
                {
                    hero.Stats.DecreaseValue(Stat.CORRUPTION, trade);
                    fountain.fountaineSFX.Play(transform.position);
                }
                else
                {
                    hero.Stats.IncreaseValue(Stat.CORRUPTION, trade);
                    fountain.altarSFX.Play(transform.position);
                }

                hero.GetComponent<PlayerController>().PlayBloodPouringAnim();

                Hero.CallCorruptionBenedictionText(fountain.Type == FountainType.Blessing ? -fountain.ValueTrade : fountain.ValueTrade);
                onAddBenedictionCorruption?.Invoke();
            }
        }
    }
}