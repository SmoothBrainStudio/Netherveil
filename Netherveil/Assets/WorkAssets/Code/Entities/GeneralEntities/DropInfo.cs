using System;
using UnityEngine;

[Serializable]
public class DropInfo
{
    public GameObject loot;
    [Range(0f, 1f)]
    public float chance;

    public int maxQuantity;
    public int minQuantity;

    [Tooltip("If chance is shared that means all quantity will drop with one chance shared.\n\n" +
        "For exemple an item ( loot = potion, chance = 0.5, quantity = 2 ) with isChanceShared true. This item has 50% chance to drop two instances of the item and can't drop only one\n\n" +
        "but if isChanceShared false, each potion has 50% chance to drop. So, for a quantity of 2, there is 25% ( 50% * 50% ) chance to drop")]
    public bool isChanceShared;

    [Tooltip("For how many % do the chances of dropping decrease after the min drops have been spawned")]
    public float decreasingValuePerDrop;

}
