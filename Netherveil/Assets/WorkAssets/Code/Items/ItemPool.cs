using Map.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ItemPool
{
    #region Debug

    private string[] MustHaveItem = { "BelzebuthBelt", "GhostlySpears", "ThunderLink", "SpearStrike", "RuneOfEnvy", "TearOfZeus" };

    private readonly bool debug = false;
   
    private List<Color> debugColors = new List<Color>()
    { Color.grey,
    Color.green,
    Color.blue,
    Color.magenta,
    Color.yellow};

    #endregion
    #region Tier Chance
    private const float CommonChance = 0.2f;
    private const float UncommonChance = 0.4f;
    private const float RareChance = 0.25f;
    private const float EpicChance = 0.1f;
    private const float LegendaryChance = 0.05f;
    private const string DefaultItem = "MonsterHeart";
    private List<float> rarityWeighting = new List<float>()
    {
        CommonChance,
        UncommonChance,
        RareChance,
        EpicChance,
        LegendaryChance
    };


    #endregion

    #region Members
    
    public List<List<string>> itemsPerTier;
    public Stack<string> itemPool;
    #endregion

    #region Methods
    public ItemPool()
    {
        ItemDatabase itemDatabase = GameResources.Get<ItemDatabase>("ItemDatabase");
        itemsPerTier = new List<List<string>>();
        itemPool = new Stack<string>();
        for (int i = 0; i < Enum.GetNames(typeof(ItemData.Rarity)).Length; i++)
        {
            // Adding pool for each Rarity
            itemsPerTier.Add(itemDatabase.datas.Where(x => (Convert.ToInt32(x.RarityTier) == i && x.isInGame)).Select(x => x.idName).ToList());
        }
        UpdateRarityWeight();
        Init();
    }

    public ItemPool(List<string> itemsId)
    {

    }

    public void Init()
    {
        ItemDatabase itemDatabase = GameResources.Get<ItemDatabase>("ItemDatabase");
        while (!IsPoolEmpty())
        {
            string item = GetRandomItemName();
            itemPool.Push(item);
            if(debug) Debug.Log("<color=#" + debugColors[(int)itemDatabase.GetItem(item).RarityTier].ToHexString() + ">" + item + "</color>");
        }
        itemPool.Reverse();
    }
    public void Init(params string[] firstItems)
    {
        ItemDatabase itemDatabase = GameResources.Get<ItemDatabase>("ItemDatabase");
        while (!IsPoolEmpty())
        {
            string item = GetRandomItemName();
            if (firstItems.Contains(item)) continue;
            itemPool.Push(item);
            if (debug) Debug.Log("<color=#" + debugColors[(int)itemDatabase.GetItem(item).RarityTier].ToHexString() + ">" + item + "</color>");
        }
        itemPool.Reverse();
        if (firstItems.Length > 0)
        {
            firstItems.Reverse();
            foreach (var item in firstItems)
            {
                itemPool.Push(item);
            }
        }
        
    }
    public bool IsPoolEmpty()
    {
        for(int i = 0; i < itemsPerTier.Count; i++)
        {
            if (itemsPerTier[i].Count > 0)
                return false;
        }
        return true;
    }

    public void UpdateRarityWeight(int indexEmpty)
    {
        float toShare = rarityWeighting[indexEmpty];
        rarityWeighting.RemoveAt(indexEmpty);
        toShare /= rarityWeighting.Count;
        for (int i = 0; i < rarityWeighting.Count; i++)
        {
            rarityWeighting[i] += toShare;
        }
    }
    public void UpdateRarityWeight()
    {
        float toShare = 0;
        for (int i = rarityWeighting.Count - 1; i >= 0; i--)
        {
            if (itemsPerTier[i].Count == 0)
            {
                toShare += rarityWeighting[i];
                rarityWeighting.RemoveAt(i);
                itemsPerTier.RemoveAt(i);
            }
        }
        toShare /= rarityWeighting.Count;
        for (int i = 0; i < rarityWeighting.Count; i++)
        {
            rarityWeighting[i] += toShare;
        }
    }

    private string GetRandomItemName()
    {
        if (IsPoolEmpty()) return DefaultItem;

        float randomRarity = Seed.Range();
        float currentChance = 0;
        int indexRarity = 0;
        for (int i = rarityWeighting.Count - 1; i >= 0; i--)
        {
            currentChance += rarityWeighting[i];
            if (randomRarity <= currentChance)
            {
                indexRarity = i;
                break;
            }
        }
        int randomItemIndex = Seed.Range(0, itemsPerTier[indexRarity].Count);
        string toReturn =  itemsPerTier[indexRarity][randomItemIndex];
        RemoveItemFromPool(indexRarity, toReturn);
        return toReturn;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Get precise item if it still in the ItemPool</returns>
    private string GetPreciseItemInPool(string name)
    {
        if(IsPoolEmpty()) return DefaultItem;
        for(int i = 0; i < itemsPerTier.Count; i++)
        {
            if (itemsPerTier[i].Contains(name))
            {
                RemoveItemFromPool(i, name);
                return name;
            }
        }
        return DefaultItem;
    }
    public string GetItem()
    {
        return itemPool.Pop();
    }

    private void RemoveItemFromPool(int rarityIndex, string name)
    {
        itemsPerTier[rarityIndex].Remove(name);
        if (itemsPerTier[rarityIndex].Count == 0)
        {
            UpdateRarityWeight(rarityIndex);
            itemsPerTier.RemoveAt(rarityIndex);
        }
    }
    private void RemoveItemFromPool(string name)
    {
        for(int i = 0; i < itemsPerTier.Count;i++)
        {
            if (itemsPerTier[i].Contains(name))
            {
                itemsPerTier[i].Remove(name);
                if (itemsPerTier[i].Count == 0)
                {
                    UpdateRarityWeight(i);
                    itemsPerTier.RemoveAt(i);
                }
            }
        }
        
    }
    #endregion
}
