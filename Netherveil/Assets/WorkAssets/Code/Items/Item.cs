using Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.VFX;

// This class is the item that is rendered in the 3D world
[Serializable]
public class Item : MonoBehaviour
{

    public static event Action<ItemEffect> OnRetrieved;
    public static event Action OnLateRetrieved;
    public static event Action OnChangePriceCoef;

    private static float priceCoef = 1.0f;
    public static float PriceCoef
    {
        get => priceCoef; 
        set
        {
            priceCoef = value;
            OnChangePriceCoef?.Invoke();
        }
    }
    public const int PRICE_PER_RARITY = 30;

    [SerializeField] private bool isRandomized = true;
    [SerializeField] private ItemDatabase database;
    [SerializeField] VisualEffect auraVFX;

    private ItemEffect itemEffect;
    private Color rarityColor = Color.white;
    public string idItemName = string.Empty;
    private int price;

    private ItemDescription itemDescription;
    public Color RarityColor => rarityColor;
    public ItemEffect ItemEffect => itemEffect;
    public ItemDatabase Database => database;

    public int Price => price;

    private void Awake()
    {
        if (itemEffect == null)
        {
            if (isRandomized)
            {
                RandomizeItem(this);
            }
            else
            {
                CreateItem();
            }
        }
        MapUtilities.onFinishStage += DestroySelf;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        MapUtilities.onFinishStage -= DestroySelf;
    }

    private void DestroySelf()
    {
        Destroy(this.gameObject);
    }
    public static void InvokeOnRetrieved(ItemEffect effect)
    {
        OnRetrieved?.Invoke(effect);
        OnLateRetrieved?.Invoke();
        AudioManager.Instance.PlaySound(AudioManager.Instance.PickUpItemSFX, Utilities.Player.transform.position);
    }

    private ItemEffect LoadClass()
    {
        return Assembly.GetExecutingAssembly().CreateInstance(idItemName.GetPascalCase()) as ItemEffect;
    }

    static public ItemEffect LoadClass(string idName)
    {
        return Assembly.GetExecutingAssembly().CreateInstance(idName.GetPascalCase()) as ItemEffect;
    }
    static public void RandomizeItem(Item item)
    {
        item.StartCoroutine(item.CursedCoroutine());
    }
    public void RandomizeItem()
    {
        List<string> allItems = new();
        foreach (var itemInDb in database.datas)
        {
            allItems.Add(itemInDb.idName);
        }
        int indexRandom = UnityEngine.Random.Range(0, allItems.Count - 1);
        idItemName = allItems[indexRandom];
    }

    public void CreateItem()
    {
        itemEffect = LoadClass();

        ItemData data = database.GetItem(idItemName);
        Material matToRender = data.mat;
        Mesh meshToRender = data.mesh;
        price = (int)(data.RarityTier + 1) * PRICE_PER_RARITY;

        rarityColor = database.GetItemRarityColor(idItemName);

        this.GetComponentInChildren<MeshRenderer>().material = matToRender != null ? matToRender : this.GetComponentInChildren<MeshRenderer>().material;
        this.GetComponentInChildren<MeshFilter>().mesh = meshToRender != null ? meshToRender : this.GetComponentInChildren<MeshFilter>().mesh;

        itemDescription = GetComponent<ItemDescription>();
        itemDescription.SetDescription(idItemName);
        auraVFX.SetFloat("Orbs amount", (float)(data.RarityTier + 1));
        auraVFX.SetVector4("Color", rarityColor);
        auraVFX.Play();
    }

    private IEnumerator CursedCoroutine()
    {
        if (InGameManager.ItemPool == null)
        {
            yield return new WaitUntil(() => InGameManager.ItemPool != null);
        }

        idItemName = InGameManager.ItemPool.GetItem();
        CreateItem();
        yield return null;
    }
}