using System;
using UnityEngine;

[Serializable]
public class ItemData : IComparable<ItemData>
{    
    public enum Rarity
    {
        COMMON,
        UNCOMMON,
        RARE,
        EPIC,
        LEGENDARY
    }
    public enum ItemType
    {
        PASSIVE,
        ACTIVE,
        PASSIVE_ACTIVE
    } 

    public Rarity RarityTier;
    public ItemType Type;
    public string idName;
    public Material mat;
    public Mesh mesh;
    [Multiline] public string Description;
    public Texture icon;
    public bool isInGame = true;

    public int CompareTo(ItemData other)
    {
        return this.idName.CompareTo(other.idName);
    }
}   
