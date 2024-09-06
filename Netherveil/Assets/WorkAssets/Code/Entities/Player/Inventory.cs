using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory : ISavable
{
    public static event Action OnAddOrRemoveBlood;

    public class BloodClass
    {
        private int value = 0;

        public int Value
        {
            get { return value; }
        }

        public void Add(int value, bool hasText = false)
        {
            this.value += value;
            OnAddOrRemoveBlood?.Invoke();
            if (hasText)
            {
                FloatingTextGenerator.CreateActionText(Utilities.Player.transform.position, $"+{value}<size=50><sprite name=\"blood\">", Color.red);
            }
        }

        public static BloodClass operator +(BloodClass blood, int increment)
        {
            blood.value += increment;
            OnAddOrRemoveBlood?.Invoke();
            FloatingTextGenerator.CreateActionText(Utilities.Player.transform.position, $"+{increment}<size=50><sprite name=\"blood\">", Color.red);
            return blood;
        }

        public static BloodClass operator -(BloodClass blood, int decrement)
        {
            blood.value -= decrement;
            OnAddOrRemoveBlood?.Invoke();
            FloatingTextGenerator.CreateActionText(Utilities.Player.transform.position, $"-{decrement}<size=50><sprite name=\"blood\">", Color.red);
            return blood;
        }
    }


    public static GameObject ActiveItemGameObject;
    IActiveItem activeItem = null;
    List<IPassiveItem> passiveItems = new List<IPassiveItem>();
    public IActiveItem ActiveItem { get { return activeItem; } }
    public List<IPassiveItem> PassiveItems { get { return passiveItems; } }
    public List<IItem> AllItems
    {
        get
        {
            List<IItem> itemList = new List<IItem>
            {
                activeItem
            };
            itemList.AddRange(passiveItems);
            return itemList;
        }
    }
    public bool HasActiveItem
    {
        get => activeItem != null;
    }
    public BloodClass Blood = new BloodClass();

    public int Keys = 0;
    private void AddActiveItem(IActiveItem item)
    {
        activeItem?.OnRemove();
        activeItem = item;
    }

    private void AddPassiveItem(IPassiveItem item)
    {
        passiveItems.Add(item);
    }

    public void RemoveItem(IPassiveItem item)
    {
        item.OnRemove();
        passiveItems.Remove(item);
    }

    public void RemoveAllItems(Vector3 _)
    {
        if (passiveItems.Count > 0)
        {
            for (int i = passiveItems.Count - 1; i >= 0; --i)
            {
                RemoveItem(passiveItems[i]);
            }
        }
        if (activeItem != null)
        {
            activeItem?.OnRemove();
            activeItem = null;
        }
    }
    public void AddItem(Item item)
    {
        ItemEffect itemEffect = item.ItemEffect;
        if ((itemEffect as IActiveItem) != null)
        {
            if (activeItem == null)
            {
                if (item.gameObject.TryGetComponent<ItemInteractionMerchant>(out var itemInteraction) && itemInteraction.enabled)
                {
                    item.gameObject.AddComponent<ItemInteraction>();
                    itemInteraction.enabled = false;
                }
                ActiveItemGameObject = GameObject.Instantiate(item.gameObject);
                ActiveItemGameObject.name = item.idItemName;
                ActiveItemGameObject.SetActive(false);
            }
            else
            {
                var go = GameObject.Instantiate(ActiveItemGameObject, item.gameObject.transform.position, Quaternion.identity);
                go.SetActive(true);
                go.GetComponent<Item>().idItemName = ActiveItemGameObject.name;
                go.GetComponentInChildren<ItemDescription>().RemovePriceText();
                go.GetComponent<Item>().CreateItem();
                go.GetComponent<Item>().ItemEffect.HasBeenRetreived = true;
                go.GetComponent<Item>().ItemEffect.CurrentEnergy = (activeItem as ItemEffect).CurrentEnergy;
                go.name = "item";
                GameObject.Destroy(ActiveItemGameObject);
                if (item.gameObject.TryGetComponent<ItemInteractionMerchant>(out var itemInteraction) && itemInteraction.enabled)
                {
                    item.gameObject.AddComponent<ItemInteraction>();
                    itemInteraction.enabled = false;
                }
                ActiveItemGameObject = GameObject.Instantiate(item.gameObject);
                ActiveItemGameObject.name = item.idItemName;
                ActiveItemGameObject.SetActive(false);
            }
            AddActiveItem(itemEffect as IActiveItem);

            if (!itemEffect.HasBeenRetreived)
            {
                itemEffect.CurrentEnergy = (itemEffect as IActiveItem).Cooldown;
            }
            else
            {
                if (itemEffect.CurrentEnergy < (itemEffect as IActiveItem).Cooldown)
                {
                    CoroutineManager.Instance.StartCoroutine((itemEffect as IActiveItem).WaitToUse());
                }
            }
        }
        else if (itemEffect as IPassiveItem != null)
        {
            AddPassiveItem(itemEffect as IPassiveItem);
        }
        (itemEffect as IItem)?.OnRetrieved();
        itemEffect.HasBeenRetreived = true;
    }

    public void AddItem(string idName)
    {
        if (string.IsNullOrEmpty(idName)) return;
        ItemEffect itemEffect = Item.LoadClass(idName);

        if ((itemEffect as IActiveItem) != null)
        {
            GameObject item = GameResources.Get<GameObject>("Item");
            ActiveItemGameObject = GameObject.Instantiate(item);
            ActiveItemGameObject.name = idName;
            ActiveItemGameObject.SetActive(false);
            AddActiveItem(itemEffect as IActiveItem);
            itemEffect.CurrentEnergy = (itemEffect as IActiveItem).Cooldown;
        }
        else if (itemEffect as IPassiveItem != null)
        {
            AddPassiveItem(itemEffect as IPassiveItem);
        }
        (itemEffect as IItem)?.OnRetrieved();
        itemEffect.HasBeenRetreived = true;
    }

    public void Save(SaveData saveData)
    {
        // Active Item
        if (ActiveItem != null)
        {
            saveData.Set("activeItemName", ActiveItem.GetType().ToString());
            saveData.Set("activeItemCooldown", ActiveItem.Cooldown);
        }
        else
        {
            saveData.Set("activeItemName", string.Empty);
        }

        // Passive Items
        saveData.Set("passiveItemCount", PassiveItems.Count);
        for (int i = 0; i < PassiveItems.Count; i++)
        {
            saveData.Set("passiveItem" + i, PassiveItems[i].GetType().ToString());
        }

        saveData.Set("inventoryBlood", Blood.Value);
    }

    public void LoadSave()
    {
        SaveData saveData = SaveManager.saveData;

        if (saveData.Get<string>("activeItemName") != string.Empty)
        {
            AddItem(saveData.Get<string>("activeItemName"));
            ActiveItem.Cooldown = saveData.Get<float>("activeItemCooldown");
            Item.InvokeOnRetrieved(ActiveItem as ItemEffect);
        }

        int passiveItemCount = saveData.Get<int>("passiveItemCount");
        for (int i = 0; i < passiveItemCount; i++)
        {
            AddItem(saveData.Get<string>("passiveItem" + i));
        }

        foreach (var item in PassiveItems)
        {
            Item.InvokeOnRetrieved(item as ItemEffect);
        }

        Blood.Add(saveData.Get<int>("inventoryBlood"));
    }
}
