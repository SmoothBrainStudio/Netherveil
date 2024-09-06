using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

public class ItemDescription : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text activePassiveText;
    private Item item;
    private float scaleDuration = 0.25f;
    private Coroutine displayRoutine;

    private void Start()
    {
        Inventory.OnAddOrRemoveBlood += UpdatePriceText;
        Item.OnChangePriceCoef += UpdatePriceText;
    }

    private void OnDestroy()
    {
        Inventory.OnAddOrRemoveBlood -= UpdatePriceText;
        Item.OnChangePriceCoef -= UpdatePriceText;
    }

    private void OnDisable()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            canvasRectTransform.localScale = Vector3.zero;
        }
    }

    public void SetDescription(string id)
    {
        item = GetComponent<Item>();

        nameText.text = item.idItemName.SeparateAllCase();
        nameText.color = item.RarityColor;

        if (priceText != null)
        {
            UpdatePriceText();
        }

        activePassiveText.text = (item.ItemEffect as IPassiveItem) != null ? "<color=grey>Passive</color>" : "<color=grey>Active</color>";

       
        ItemEffect itemEffect = Assembly.GetExecutingAssembly().CreateInstance(id.GetPascalCase()) as ItemEffect;

        string descriptionToDisplay = item.Database.GetItem(id).Description;
        char[] separators = new char[] { ' ', '\n' };
        string[] splitDescription = descriptionToDisplay.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        string finalDescription = string.Empty;
        FieldInfo[] fieldOfItem = itemEffect.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        for (int i = 0; i < splitDescription.Length; i++)
        {
            if (splitDescription[i].Length > 0 && splitDescription[i].Contains('{'))
            {
                int indexEntry = splitDescription[i].IndexOf("{") + 1;
                int indexOut = splitDescription[i].IndexOf("}");
                int length = indexOut - indexEntry;
                string valueToFind = splitDescription[i].Substring(indexEntry, length);
                FieldInfo valueInfo = fieldOfItem.FirstOrDefault(x => x.Name == valueToFind);
                if(valueInfo != null)
                {
                    var memberValue = valueInfo.GetValue(itemEffect);
                    if (splitDescription[i].Contains("%"))
                    {
                        memberValue = Convert.ToSingle(valueInfo.GetValue(itemEffect)) * 100;
                    }
                    splitDescription[i] = splitDescription[i].Replace("{" + valueToFind + "}", memberValue.ToString());
                }
                else
                {
                    splitDescription[i] = "N/A";
                    Debug.LogWarning($"value : {valueToFind}, has not be found", gameObject);
                }
            }

            finalDescription += splitDescription[i] + " ";
        }

        descriptionText.text = finalDescription;
    }

    public static string GetDescription(string id)
    {
        ItemDatabase itemDb = GameResources.Get<ItemDatabase>("ItemDatabase");
        ItemEffect itemEffect = Assembly.GetExecutingAssembly().CreateInstance(id.GetPascalCase()) as ItemEffect;
        string descriptionToDisplay = itemDb.GetItem(id).Description;
        char[] separators = new char[] { ' ', '\n' };
        string[] splitDescription = descriptionToDisplay.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        string finalDescription = string.Empty;
        FieldInfo[] fieldOfItem = itemEffect.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        for (int i = 0; i < splitDescription.Length; i++)
        {
            if (splitDescription[i].Length > 0 && splitDescription[i].Contains('{'))
            {
                int indexEntry = splitDescription[i].IndexOf("{") + 1;
                int indexOut = splitDescription[i].IndexOf("}");
                int length = indexOut - indexEntry;
                string valueToFind = splitDescription[i].Substring(indexEntry, length);
                FieldInfo valueInfo = fieldOfItem.FirstOrDefault(x => x.Name == valueToFind);
                if (valueInfo != null)
                {
                    var memberValue = valueInfo.GetValue(itemEffect);
                    if (splitDescription[i].Contains("%"))
                    {
                        memberValue = Convert.ToSingle(valueInfo.GetValue(itemEffect)) * 100;
                    }
                    splitDescription[i] = splitDescription[i].Replace("{" + valueToFind + "}", memberValue.ToString());
                }
                else
                {
                    splitDescription[i] = "N/A";
                    Debug.LogWarning($"value : {valueToFind}, has not be found");
                }
            }

            finalDescription += splitDescription[i] + " ";
        }

        return finalDescription;
    }

    private void UpdatePriceText()
    {
        if (priceText == null)
            return;

        priceText.text = "Cost: " + (int)(item.Price * Item.PriceCoef) + " <size=50><sprite name=\"blood\">";

        if (Utilities.Hero.Inventory.Blood.Value >= item.Price)
        {
            priceText.color = Color.white;
        }
        else
        {
            priceText.color = Color.red;
        }
    }

    public void TogglePanel(bool toggle)
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        displayRoutine = StartCoroutine(toggle ? canvasRectTransform.UpScaleCoroutine(scaleDuration, 0.01f) : canvasRectTransform.DownScaleCoroutine(scaleDuration, 0.01f));
    }

    public void RemovePriceText()
    {
        if (priceText != null)
        {
            priceText.text = string.Empty;
            priceText = null;
        }
    }
}
