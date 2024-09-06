using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Samples.RebindUI;
using UnityEngine.UI;

public class ItemBar : MonoBehaviour
{
    private Coroutine cooldownActiveRoutine;
    private Coroutine cooldownSpecialRoutine;
    private Coroutine displayRoutine;
    private bool toggleOn;

    [Header("General")]
    [SerializeField] private KeybindingsIcons iconsList;
    [SerializeField] private ItemDatabase database;
    [SerializeField] private Transform itemPassiveTransform;
    [SerializeField] private ItemFrame framePf;
    [SerializeField] private GameObject panelToToggle;
    [SerializeField] private GameObject textNoItems;
    [SerializeField] GameObject gameObjectItem;

    [Header("Backgrounds")]
    [SerializeField] private Sprite backDamnation;
    [SerializeField] private Sprite backDivine;
    [SerializeField] private Sprite[] rarityBackItemSprite;
    [SerializeField] private Sprite[] backItemActiveNormal;

    [Header("Sprites")]
    [SerializeField] private Sprite damnationSprite;
    [SerializeField] private Sprite divineSprite;

    [Header("Specials frames")]
    [SerializeField] private SpecialItemFrame specialAbilityFrame;
    [SerializeField] private SpecialItemFrame specialItemFrame;

    [Header("Bidings")]
    [SerializeField] private InputActionReference keyboardActive;
    [SerializeField] private InputActionReference keyboardAbility;
    [SerializeField] private InputActionReference gamepadActive;
    [SerializeField] private InputActionReference gamepadAbility;

    public bool ToggleOn { get { return toggleOn; } }

    private void Start()
    {
        toggleOn = false;
        if (DeviceManager.Instance.IsPlayingKB())
            UpdateKeyboardBiding();
        else
            UpdateGamepadBiding();
    }

    private void OnEnable()
    {
        Item.OnRetrieved += OnItemAdd;
        Utilities.Hero.OnBenedictionMaxUpgrade += OnSpecialAbilityAdd;
        Utilities.Hero.OnCorruptionMaxUpgrade += OnSpecialAbilityAdd;
        Utilities.Hero.OnCorruptionMaxDrawback += OnSpecialAbilityRemove;
        Utilities.Hero.OnBenedictionMaxDrawback += OnSpecialAbilityRemove;
        IActiveItem.OnActiveItemCooldownStartedTimeBased += ActiveItemCooldown;
        ISpecialAbility.OnSpecialAbilityActivated += SpecialAbilityCooldown;
        DeviceManager.OnChangedToKB += UpdateKeyboardBiding;
        DeviceManager.OnChangedToGamepad += UpdateGamepadBiding;
        PauseMenu.OnUnpause += UpdateBinding;

        IActiveItem.OnActiveItemCooldownUpdatedRoomBased += ActivateItemCooldownRoomBased;
    }

    private void OnDisable()
    {
        Item.OnRetrieved -= OnItemAdd;
        IActiveItem.OnActiveItemCooldownStartedTimeBased -= ActiveItemCooldown;
        ISpecialAbility.OnSpecialAbilityActivated -= SpecialAbilityCooldown;
        DeviceManager.OnChangedToKB -= UpdateKeyboardBiding;
        DeviceManager.OnChangedToGamepad -= UpdateGamepadBiding;
        PauseMenu.OnUnpause -= UpdateBinding;

        IActiveItem.OnActiveItemCooldownUpdatedRoomBased -= ActivateItemCooldownRoomBased;

        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);

            RectTransform rectTransform = itemPassiveTransform.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector3(-rectTransform.sizeDelta.x, 0.0f, 0.0f);
        }
    }

    private void UpdateBinding()
    {
        if (DeviceManager.Instance.IsPlayingKB())
        {
            UpdateKeyboardBiding();
        }
        else
        {
            UpdateGamepadBiding();
        }
    }

    private void UpdateKeyboardBiding()
    {
        string keyActive = keyboardActive.action.bindings.First().path.Split("/").Last();
        if (specialItemFrame != null)
            specialItemFrame.SetKey(iconsList.kb.GetSprite(keyActive), keyActive.ToUpper());

        string keyAbility = keyboardAbility.action.bindings.First().path.Split("/").Last();
        if (specialAbilityFrame != null)
            specialAbilityFrame.SetKey(iconsList.kb.GetSprite(keyAbility), keyAbility.ToUpper());
    }

    private void UpdateGamepadBiding()
    {
        string keyActive = gamepadActive.action.bindings.First().path.Split("/").Last();
        Sprite activeSprite = DeviceManager.Instance.CurrentDevice is DualShockGamepad ? iconsList.ps4.GetSprite(keyActive) : iconsList.xbox.GetSprite(keyActive);
        specialItemFrame.SetKey(activeSprite, string.Empty);

        string keyAbility = gamepadAbility.action.bindings.First().path.Split("/").Last();
        Sprite abilitySprite = DeviceManager.Instance.CurrentDevice is DualShockGamepad ? iconsList.ps4.GetSprite(keyAbility) : iconsList.xbox.GetSprite(keyAbility);
        specialAbilityFrame.SetKey(abilitySprite, string.Empty);
    }

    private void OnItemAdd(ItemEffect itemAdd)
    {
        if (textNoItems.activeSelf == true)
        {
            textNoItems.SetActive(false);
        }

        ItemData data = database.GetItem(itemAdd.Name);
        Sprite item = Sprite.Create((Texture2D)data.icon, new Rect(0.0f, 0.0f, data.icon.width, data.icon.height), new Vector2(0.5f, 0.5f), 100.0f);

        if (itemAdd is IPassiveItem)
        {
            ItemFrame frame = Instantiate(framePf, itemPassiveTransform);
            frame.SetFrame(rarityBackItemSprite[(int)data.RarityTier], item);
            frame.GetComponent<ItemSelector>().SetPanelDescriton(itemAdd.Name, data.Type.ToString(), ItemDescription.GetDescription(data.idName));
        }
        else if (itemAdd is IActiveItem)
        {
            specialItemFrame.SetFrame(backItemActiveNormal[(int)data.RarityTier], item);
            specialItemFrame.GetComponent<ItemSelector>().SetPanelDescriton(itemAdd.Name, data.Type.ToString(), ItemDescription.GetDescription(data.idName));
        }
    }

    private void OnSpecialAbilityAdd(ISpecialAbility ability)
    {

        if (ability as DamnationVeil != null)
        {
            specialAbilityFrame.SetFrame(backDamnation, damnationSprite);
        }
        else if (ability as DivineShield != null)
        {
            specialAbilityFrame.SetFrame(backDivine, divineSprite);
        }
    }

    private void OnSpecialAbilityRemove()
    {
        specialAbilityFrame.SetFrame(rarityBackItemSprite.First(), null);
    }

    private void ActiveItemCooldown(ItemEffect itemEffect)
    {
        ItemData data = database.GetItem(itemEffect.Name);
        Sprite item = Sprite.Create((Texture2D)data.icon, new Rect(0.0f, 0.0f, data.icon.width, data.icon.height), new Vector2(0.5f, 0.5f), 100.0f);
        specialItemFrame.SetFrame(rarityBackItemSprite[(int)data.RarityTier], item);

        float cooldown = (itemEffect as IActiveItem).Cooldown;

        if (cooldownActiveRoutine != null)
            StopCoroutine(cooldownActiveRoutine);

        cooldownActiveRoutine = StartCoroutine(CooldownRoutine(cooldown, specialItemFrame));
    }

    private void ActivateItemCooldownRoomBased(ItemEffect itemEffect)
    {
        ItemData data = database.GetItem(itemEffect.Name);
        if(!(itemEffect as IActiveItem).TimeBased)
        {
            Sprite item = Sprite.Create((Texture2D)data.icon, new Rect(0.0f, 0.0f, data.icon.width, data.icon.height), new Vector2(0.5f, 0.5f), 100.0f);
            specialItemFrame.SetFrame(rarityBackItemSprite[(int)data.RarityTier], item);
            specialItemFrame.ToggleCooldown(true);
            specialItemFrame.SetCooldown(itemEffect.CurrentEnergy, (itemEffect as IActiveItem).Cooldown);
            if (itemEffect.CurrentEnergy / (itemEffect as IActiveItem).Cooldown == 1)
            {
                specialItemFrame.ToggleCooldown(false);
            }
        }
    }

    private void SpecialAbilityCooldown()
    {
        float cooldown = Utilities.Player.GetComponent<PlayerController>().SpecialAbility.Cooldown;

        if (cooldownSpecialRoutine != null)
            StopCoroutine(cooldownSpecialRoutine);

        cooldownSpecialRoutine = StartCoroutine(CooldownRoutine(cooldown, specialAbilityFrame));
    }

    private IEnumerator CooldownRoutine(float duration, SpecialItemFrame frame)
    {
        float elapsed = 0.0f;

        frame.ToggleCooldown(true);

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            frame.SetCooldown(elapsed, duration);

            yield return null;
        }

        frame.ToggleCooldown(false);
    }

    public void Toggle(float delay = 0.0f)
    {
        toggleOn = !toggleOn;

        RectTransform rectTransform = itemPassiveTransform.GetComponent<RectTransform>();

        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        if (toggleOn)
            displayRoutine = StartCoroutine(MovementRoutine(rectTransform, new Vector3(-rectTransform.sizeDelta.x, 0.0f, 0.0f), Vector3.zero, 0.1f, delay));
        else
            displayRoutine = StartCoroutine(MovementRoutine(rectTransform, Vector3.zero, new Vector3(-rectTransform.sizeDelta.x, 0.0f, 0.0f), 0.1f, delay));
    }

    private IEnumerator MovementRoutine(RectTransform toMove, Vector3 from, Vector3 to, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;

            toMove.anchoredPosition = Vector3.Lerp(from, to, factor);

            yield return null;
        }
    }

    public void SelectPassiveItemInUI()
    {
        Selectable items = gameObjectItem.GetComponentInChildren<Selectable>();

        if (items != null)
        {
            if (EventSystem.current.currentSelectedGameObject == items.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            EventSystem.current.SetSelectedGameObject(items.gameObject);
        }
    }
}