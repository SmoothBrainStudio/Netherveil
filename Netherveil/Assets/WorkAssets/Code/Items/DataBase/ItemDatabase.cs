using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

[CreateAssetMenu(menuName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [HideInInspector]
    public List<ItemData> datas = new();

    public ItemData GetItem(string name)
    {
        return datas.Where(x => x.idName == name).FirstOrDefault();
    }

    public Color GetItemRarityColor(string name)
    {
        ItemData datas = GetItem(name);

        switch (datas.RarityTier)
        {
            case ItemData.Rarity.COMMON:
                return new Color(0.63f, 0.64f, 0.63f);
            case ItemData.Rarity.UNCOMMON:
                return new Color(0.01f, 0.63f, 0.02f);
            case ItemData.Rarity.RARE:
                return new Color(0.04f, 0.12f, 0.75f);
            case ItemData.Rarity.EPIC:
                return new Color(0.5f, 0.02f, 0.67f);
            case ItemData.Rarity.LEGENDARY:
                return new Color(0.88f, 0.36f, 0.04f);
        }

        return Color.white;
    }

#if UNITY_EDITOR
    public void DeleteInDatabase(ItemData item)
    {
        datas.Remove(item);
        string itemName = item.idName.GetCamelCase();
        string itemType = string.Empty;

        switch (item.Type)
        {
            case ItemData.ItemType.PASSIVE:
                itemType = "PassiveItems";
                break;
            case ItemData.ItemType.ACTIVE:
                itemType = "ActiveItems";
                break;
            case ItemData.ItemType.PASSIVE_ACTIVE:
                itemType = "ActivePassiveItems";
                break;
            default:
                break;
        }
        string path = Application.dataPath + "/SampleSceneAssets/Code/Items/" + itemType + $"/{itemName}.cs";
        File.Delete(path);
        AssetDatabase.Refresh();
    }
#endif
}
