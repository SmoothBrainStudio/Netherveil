using System.Collections;
using TMPro;
using UnityEngine;

public class HudHandler : MonoBehaviour
{
    private static HudHandler instance;
    public static HudHandler current
    {
        get
        {
            if (instance == null)
                throw new System.Exception("No HUD Handler in the scene");

            return instance;
        }
    }
    [SerializeField] private GameObject GameOver;
    [SerializeField] private TMP_Text BloodTestMesh;
    [SerializeField] private MinMaxSlider corruptionSlider;
    [SerializeField] private TMP_Text corruptionText;

    [SerializeField] private CanvasGroup canvasGroupHUD;
    private Coroutine activeRoutine = null;

    [Header("HUD parts")]
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private QuestHUD questHUD;
    [SerializeField] private MapHUD mapHUD;
    [SerializeField] private DescriptionTabHUD descriptionTab;
    [SerializeField] private MessageInfoHUD messageInfoHUD;
    [SerializeField] private BuffHUD buffHUD;
    [SerializeField] private ItemBar itemBar;

    public PauseMenu PauseMenu => pauseMenu;
    public QuestHUD QuestHUD => questHUD;
    public MapHUD MapHUD => mapHUD;
    public DescriptionTabHUD DescriptionTab => descriptionTab;
    public MessageInfoHUD MessageInfoHUD => messageInfoHUD;
    public BuffHUD BuffHUD => buffHUD;
    public ItemBar ItemBar => itemBar;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Utilities.Hero.OnDeath += ActiveGameOver;
    }

    private void Update()
    {
        BloodTestMesh.text = Utilities.Hero.Inventory.Blood.Value.ToString();
        corruptionSlider.value = Utilities.Hero.Stats.GetValue(Stat.CORRUPTION);
        corruptionText.text = Mathf.Abs(Utilities.Hero.Stats.GetValue(Stat.CORRUPTION)).ToString();
    }

    public void SetActive(bool active, float duration = 0.0f)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        if (active)
            activeRoutine = StartCoroutine(ActiveRoutine(0.0f, 1.0f, duration));
        else
            activeRoutine = StartCoroutine(ActiveRoutine(1.0f, 0.0f, duration));
    }

    private IEnumerator ActiveRoutine(float from, float to, float duration)
    {
        float elapsed = 0.0f;

        canvasGroupHUD.alpha = from;

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;

            canvasGroupHUD.alpha = factor;

            yield return null;
        }

        canvasGroupHUD.alpha = to;
        activeRoutine = null;
    }

    // Not in the right place -_(O_O)_-
    public void ActiveGameOver(Vector3 _)
    {
        GameOver.SetActive(true);
        SetActive(false);
        GameOver.GetComponent<GameOver>().LaunchDeathCam();
    }
}
