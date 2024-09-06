using Map;
using UnityEngine;

public abstract class Consumable : MonoBehaviour, IConsumable
{
    public int Price { get; protected set; } = 0;
    public bool CanBeRetrieved { get; protected set; } = true;
    protected Hero player;
    protected GameObject model;
    float lerpTimer = 0f;
    readonly float ATTRACTION_DISTANCE = 6f;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Hero>();
        MapUtilities.onFinishStage += DestroyOnChangeStage;
    }

    protected virtual void Start()
    {
        model = GetComponentInChildren<MeshRenderer>().gameObject;
        Destroy(gameObject, 60);
    }

    protected virtual void Update()
    {
        RetrieveConsumable();
        FloatingAnimation();
    }

    private void OnDestroy()
    {
        MapUtilities.onFinishStage -= DestroyOnChangeStage;
    }

    private void DestroyOnChangeStage()
    {
        Destroy(gameObject);
    }

    private void FloatingAnimation()
    {
        model.transform.Rotate(new Vector3(0, 50f * Time.deltaTime, 0));
    }

    private void RetrieveConsumable()
    {
        if (!CanBeRetrieved)
            return;

        float distance = Vector2.Distance(player.transform.position.ToCameraOrientedVec2(), transform.position.ToCameraOrientedVec2());
        if (distance <= ATTRACTION_DISTANCE)
        {
            lerpTimer += Time.deltaTime / 5.5f;
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, lerpTimer);
            if (distance <= 1f)
            {
                AudioManager.Instance.PlaySound(AudioManager.Instance.PickUpCollectibleSFX);
                OnRetrieved();
            }
        }
        else
        {
            lerpTimer = 0f;
        }
    }

    public abstract void OnRetrieved();
}
