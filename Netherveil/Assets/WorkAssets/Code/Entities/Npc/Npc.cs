using UnityEngine;
using UnityEngine.UI;

public abstract class Npc : Entity, IInterractable
{
    [SerializeField] public Image rangeImage;
    public Image RangeImage { get => rangeImage; }
    PlayerInteractions playerInteractions;
    Hero hero;
    private bool isSelect = false;
    private Coroutine rangeRoutine;

    public virtual void Interract()
    {
        throw new System.NotImplementedException();
    }

    protected override void Start()
    {
        base.Start();
        playerInteractions = GameObject.FindWithTag("Player").GetComponent<PlayerInteractions>();
        hero = playerInteractions.GetComponent<Hero>();
    }

    protected override void Update()
    {
        base.Update();
    }

    private void FixedUpdate()
    {
        Interraction();
    }

    private void OnDisable()
    {
        if (rangeRoutine != null)
        {
            StopCoroutine(rangeRoutine);
            rangeImage.rectTransform.localScale = Vector3.zero;
        }
    }

    private void Interraction()
    {
        bool isInRange = Vector3.Distance(playerInteractions.transform.position, transform.position)
            <= hero.Stats.GetValue(Stat.CATCH_RADIUS);

        Vector3 npcToPlayerVec = (playerInteractions.transform.position - transform.position);
        bool isTouchingMapBetween = Physics.Raycast(transform.position, npcToPlayerVec.normalized, npcToPlayerVec.magnitude, LayerMask.GetMask("Map"));
        Debug.DrawRay(transform.position, npcToPlayerVec, Color.yellow, Time.deltaTime * 2);

        if (isInRange && !isTouchingMapBetween && !playerInteractions.InteractablesInRange.Contains(this))
        {
            playerInteractions.InteractablesInRange.Add(this);
        }
        else if ((!isInRange || isTouchingMapBetween) && playerInteractions.InteractablesInRange.Contains(this))
        {
            playerInteractions.InteractablesInRange.Remove(this);
            Deselect();       
        }
    }

    public void ToggleRangeImage(bool toggle)
    {
        float durationScale = 0.15f;

        if (rangeRoutine != null)
            StopCoroutine(rangeRoutine);

        rangeRoutine = StartCoroutine(toggle ? rangeImage.rectTransform.UpScaleCoroutine(durationScale) : rangeImage.rectTransform.DownScaleCoroutine(durationScale));
    }

    public void Select()
    {
        if (isSelect)
            return;

        isSelect = true;
        ToggleRangeImage(true);
    }

    public void Deselect()
    {
        if (!isSelect)
            return;

        isSelect = false;
        ToggleRangeImage(false);
    }
}
