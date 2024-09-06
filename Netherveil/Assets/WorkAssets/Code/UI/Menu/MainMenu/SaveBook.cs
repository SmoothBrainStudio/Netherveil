using Map.Generation;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class SaveBook : MonoBehaviour
{
    [SerializeField] private Transform closeTransform;
    [SerializeField] private Transform openTransform;
    [SerializeField] private GameObject closeObject;
    [SerializeField] private GameObject openObject;
    private Animator bookAnimator;

    [SerializeField] private GameObject savePart;
    [SerializeField] private GameObject notSavePart;

    [SerializeField] private TMP_Text nameTMP;
    [SerializeField] private TMP_Text seedTMP;
    [SerializeField] private TMP_InputField inputName;
    [SerializeField] private TMP_InputField inputSeed;

    [SerializeField] private int saveNumber = 0;
    private bool saveRegister = false;
    private float durationMovementIn = 1.0f;
    private float durationMovementOut = 1.0f;
    private Coroutine routine;

    public bool SaveRegister
    {
        get
        {
            return saveRegister;
        }
        set
        {
            saveRegister = value;
            if (saveRegister)
            {
                savePart.SetActive(true);
                notSavePart.SetActive(false);
            }
            else
            {
                savePart.SetActive(false);
                notSavePart.SetActive(true);
            }
        }
    }

    private void Start()
    {
        bookAnimator = openObject.GetComponent<Animator>();
    }

    public void Close()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(CloseRoutine());
    }

    public void Open()
    {
        SaveManager.SelectSave(saveNumber);

        SaveRegister = SaveManager.saveData.hasData;
        if (SaveManager.saveData.hasData)
        {
            nameTMP.text = "Name : " + SaveManager.saveData.Get<string>("name");
            seedTMP.text = "Seed : " + SaveManager.saveData.Get<string>("seed");
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(OpenRoutine());
    }

    public void StartBook()
    {
        if (SaveManager.saveData.hasData)
        {
            Seed.Set(SaveManager.saveData.Get<string>("seed"));
        }
        else
        {
            if (inputName.text.Any())
            {
                SaveManager.saveData.Set("name", inputName.text);
            }
            else
            {
                SaveManager.saveData.Set("name", "Hero");
            }

            if (inputSeed.text.Any())
            {
                SaveManager.saveData.Set("seed", inputSeed.text);
                Seed.Set(inputSeed.text);
            }
            else
            {
                Seed.RandomizeSeed();
                SaveManager.saveData.Set("seed", Seed.seed);
            }
        }
    }

    public void DeleteSave()
    {
        SaveRegister = false;
        SaveManager.EraseSave(saveNumber);
        SaveManager.SelectSave(saveNumber);
    }

    private IEnumerator CloseRoutine()
    {
        float elapsed = 0f;
        bookAnimator.SetBool("IsOpen", false);

        while (elapsed < durationMovementOut)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, durationMovementOut);
            float factor = elapsed / durationMovementOut;
            float ease = EasingFunctions.EaseInOutQuad(factor);
            Vector3 resultPosition = Vector3.Lerp(openTransform.position, closeTransform.position, ease);
            Quaternion resultRotation = Quaternion.Lerp(openTransform.rotation, closeTransform.rotation, ease);

            transform.position = resultPosition;
            transform.rotation = resultRotation;

            yield return null;
        }

        closeObject.SetActive(true);
        openObject.SetActive(false);

        transform.position = closeTransform.position;
        transform.rotation = closeTransform.rotation;

        routine = null;
    }

    private IEnumerator OpenRoutine()
    {
        float elapsed = 0.0f;

        while (elapsed < durationMovementIn)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, durationMovementIn);
            float factor = elapsed / durationMovementIn;
            float ease = EasingFunctions.EaseInOutQuad(factor);
            Vector3 resultPosition = Vector3.Lerp(closeTransform.position, openTransform.position, ease);
            Quaternion resultRotation = Quaternion.Lerp(closeTransform.rotation, openTransform.rotation, ease);

            transform.position = resultPosition;
            transform.rotation = resultRotation;

            yield return null;
        }

        closeObject.SetActive(false);
        openObject.SetActive(true);
        bookAnimator.SetBool("IsOpen", true);

        transform.position = openTransform.position;
        transform.rotation = openTransform.rotation;

        routine = null;
    }
}
