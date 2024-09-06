using TMPro;
using UnityEngine;

public class MerchantCounterDisplay : MonoBehaviour
{
    private MerchantCounter merchantCounter;

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TMP_Text displayTextMesh;
    private Coroutine displayRoutine;
    private float displayDuration = 0.2f;
    private float originalSize;
    private float iconSize;

    private void Awake()
    {
        merchantCounter = GetComponent<MerchantCounter>();
    }

    private void Start()
    {
        originalSize = displayTextMesh.fontSize;
        iconSize = originalSize + 10;
        rectTransform.localScale = Vector3.zero;
       
    }

    private void OnEnable()
    {
        Inventory.OnAddOrRemoveBlood += SetText;
    }

    private void OnDisable()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            rectTransform.localScale = Vector3.zero;
        }
        Inventory.OnAddOrRemoveBlood -= SetText;
    }

    public void Display()
    {
        SetText();

        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        displayRoutine = StartCoroutine(rectTransform.UpScaleCoroutine(displayDuration, 0.01f));
    }

    public void Undisplay()
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);
        displayRoutine = StartCoroutine(rectTransform.DownScaleCoroutine(displayDuration, 0.01f));
    }

    private void SetText()
    {
        string bloodPrice = 
            (Utilities.Hero.Inventory.Blood.Value < merchantCounter.BloodPrice ? $"<color=red>{merchantCounter.BloodPrice}</color>" : 
            $"{merchantCounter.BloodPrice}");

        string blood = $"{bloodPrice}<size={iconSize}><sprite name=\"blood\"><size={originalSize}>";

        if(merchantCounter.NbPurchases >= merchantCounter.MAX_PURCHASE)
        {
            displayTextMesh.text = $"<color=red>Max Purchase Reached...</color>";
        }
        else
        {
            displayTextMesh.text = $"Use {blood} to gain <color=green>{merchantCounter.ValueTrade} HP</color>.";
        }
    }
}
