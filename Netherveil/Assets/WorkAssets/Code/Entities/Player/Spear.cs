using System;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.VFX;

public class Spear : MonoBehaviour
{
    Transform player;
    Hero hero;
    PlayerController playerController;
    Transform parent = null;
    Animator playerAnimator;
    public static Action<Spear> OnPlacedInWorld;
    public static Action<Spear> OnLatePlacedInWorld;
    public static Action OnPlacedInHand;

    [SerializeField] GameObject trailPf;
    public VisualEffect PhantomSpearVFX { get; set; }
    public BoxCollider SpearThrowCollider { get; set; } = null;
    GameObject trail;

    Quaternion initLocalRotation;
    Quaternion initMeshRotation;
    Vector3 initLocalPosition;

    Vector3 spearPosition;
    public bool IsThrown { get; private set; } = false;
    public bool IsThrowing { get; private set; } = false;
    Vector3 posToReach = new();
    MeshRenderer meshRenderer;
    readonly float SPEAR_SPEED = 5000f;
    readonly float SPEAR_WAIT_TIME = 0.15f;
    bool placedInWorld = false;
    public static int NbSpears = 1;

    LineRenderer thunderlinkLineRenderer;
    public LineRenderer ThunderLinkLineRenderer { get => thunderlinkLineRenderer; }
    VisualEffect thunderLinkVFX;
    public VisualEffect ThunderLinkVFX { get => thunderLinkVFX; }

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        SpearThrowCollider = playerController.SpearThrowCollider;
        hero = player.GetComponent<Hero>();
        initLocalRotation = transform.localRotation;
        initLocalPosition = transform.localPosition;
        playerAnimator = player.GetComponentInChildren<Animator>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        initMeshRotation = meshRenderer.gameObject.transform.localRotation;
    }

    private void OnDestroy()
    {
        if (trail) Destroy(trail);
        StopAllCoroutines();
    }

    void Update()
    {
        playerAnimator.SetBool(playerController.SpearThrowingHash, IsThrowing);
        playerAnimator.SetBool(playerController.SpearThrownHash, IsThrown);


        if (placedInWorld)
        {
            Vector3 updatePos = meshRenderer.transform.position;
            updatePos.y += Mathf.Sin(Time.time * Time.timeScale) * 0.0009f;
            meshRenderer.transform.position = updatePos;
            meshRenderer.transform.Rotate(new Vector3(0, 50f * Time.deltaTime, 0));
        }

        if (trail == null)
        {
            return;
        }

        if (CanPlaceSpearInWorld())
        {
            PlaceSpearInWorld();
        }
        else if (CanPlaceSpearInHand())
        {
            PlaceSpearInPlayerHand();
        }
    }

    private void PlaceSpearInPlayerHand()
    {
        transform.position = posToReach;
        // On réatache la lance à la main
        transform.SetParent(parent, true);
        // On réinit la local pos et la local rotation pour que la lance soit parfaitement dans la main du joueur comme elle l'était
        transform.SetLocalPositionAndRotation(initLocalPosition, initLocalRotation);
        meshRenderer.gameObject.transform.localRotation = initMeshRotation;
        parent = null;
        meshRenderer.enabled = true;
        IsThrowing = false;
        Destroy(trail);
        if (hero.State != (int)Hero.PlayerState.KNOCKBACK)
        {
            hero.State = (int)Entity.EntityState.MOVE;
        }
        SpearThrowCollider.gameObject.SetActive(false);
        OnPlacedInHand?.Invoke();
    }

    private void PlaceSpearInWorld()
    {
        Destroy(trail);
        meshRenderer.enabled = true;
        // We set position at the exact place ( the spear doesn't move, just tp )
        transform.SetPositionAndRotation(posToReach + Vector3.up,
            Quaternion.identity * Quaternion.Euler(-90f, 90f, 0));

        IsThrowing = false;
        if (hero.State != (int)Hero.PlayerState.KNOCKBACK)
        {
            hero.State = (int)Entity.EntityState.MOVE;
        }
        SpearThrowCollider.gameObject.SetActive(false);
        placedInWorld = true;

        OnPlacedInWorld?.Invoke(this);
        OnLatePlacedInWorld?.Invoke(this);
    }

    public void Throw(Vector3 _posToReach)
    {
        StartCoroutine(ThrowCoroutine(_posToReach));
    }


    IEnumerator ThrowCoroutine(Vector3 _posToReach)
    {
        IsThrown = true;
        IsThrowing = true;
        hero.State = (int)Entity.EntityState.ATTACK;

        yield return new WaitForSeconds(SPEAR_WAIT_TIME);

        AudioManager.Instance.PlaySound(player.GetComponent<PlayerController>().ThrowSpearSFX);
        meshRenderer.enabled = false;
        trail = Instantiate(trailPf, transform.position, Quaternion.identity);
        posToReach = _posToReach;
        posToReach.y += 0.5f;
        Vector3 playerToPosToReachVec = (posToReach - transform.position);

        trail.GetComponent<Rigidbody>().AddForce(playerToPosToReachVec.normalized * SPEAR_SPEED, ForceMode.Force);
        DeviceManager.Instance.ApplyVibrations(0.001f, 0.005f, 0.1f);

        //check if colliding with obstacle to stop the spear on collide
        RaycastHit[] hits = Physics.RaycastAll(transform.position, playerToPosToReachVec, playerToPosToReachVec.magnitude);
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (((1 << hit.collider.gameObject.layer) & LayerMask.GetMask("Map")) != 0)
                {
                    posToReach = new Vector3(hit.point.x, player.position.y, hit.point.z);
                    playerToPosToReachVec = (posToReach - transform.position);
                    break;
                }
            }
        }

        ApplyDamages(playerToPosToReachVec, posToReach, debugMode: false);

        // On set le parent que la lance avait ( la main du joueur ), puis on la retire tant qu'elle est lancée afin de la rendre indépendante 
        parent = transform.parent;
        transform.parent = null;
    }

    public void Return()
    {
        StartCoroutine(ReturnCoroutine());
    }

    IEnumerator ReturnCoroutine()
    {
        IsThrown = false;
        IsThrowing = true;
        hero.State = (int)Entity.EntityState.ATTACK;
        placedInWorld = false;

        yield return new WaitForSeconds(SPEAR_WAIT_TIME);

        AudioManager.Instance.PlaySound(player.GetComponent<PlayerController>().RetrieveSpearSFX);
        meshRenderer.enabled = false;
        trail = Instantiate(trailPf, posToReach, Quaternion.identity);
        // Spear position est la position où la lance était plantée avant de revenir vers le joueur
        spearPosition = posToReach;
        posToReach = parent.transform.position;
        trail.GetComponent<Rigidbody>().AddForce((posToReach - trail.transform.position).normalized * SPEAR_SPEED, ForceMode.Force);
        DeviceManager.Instance.ApplyVibrations(0.005f, 0.001f, 0.1f);

        //orient player in front of spear
        float angle = player.AngleOffsetToFaceTarget(new Vector3(spearPosition.x, player.position.y, spearPosition.z));
        if (angle != float.MaxValue)
        {
            player.GetComponent<PlayerController>().OffsetPlayerRotation(angle, true);
        }

        Vector3 playerToSpearVec = spearPosition - player.position;
        ApplyDamages(playerToSpearVec, spearPosition, debugMode: false);
    }

    void ApplyDamages(Vector3 playerToTargetPos, Vector3 lookAtPos, bool debugMode)
    {
        if (debugMode)
        {
            SpearThrowCollider.gameObject.SetActive(true);
        }
        bool corruptionNerfApplied = false;

        ScaleColliderToVector(playerToTargetPos);
        SpearThrowCollider.transform.parent.LookAt(lookAtPos + Vector3.up);

        Collider[] colliders = SpearThrowCollider.BoxOverlap();

        if (colliders.Length > 0)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject.TryGetComponent<IDamageable>(out var entity) && collider.gameObject != player.gameObject)
                {
                    if (!corruptionNerfApplied)
                    {
                        //hero.CorruptionNerf(/*entity, hero*/);
                        corruptionNerfApplied = true;
                    }

                    hero.Attack(entity);
                }
            }
        }
    }

    public void ScaleColliderToVector(Vector3 centerToSpearVec)
    {
        //offset so that the collide also takes the spear end spot
        float collideOffset = 0.2f;
        Vector3 scale = SpearThrowCollider.transform.localScale;
        scale.z = centerToSpearVec.magnitude;
        SpearThrowCollider.transform.localScale = scale;
        SpearThrowCollider.transform.localPosition = new Vector3(0f, 0f, scale.z / 2f + collideOffset);
    }

    public void SetThunderLinkVFX(VisualEffect vfx, LineRenderer lr)
    {
        thunderLinkVFX = vfx;
        thunderlinkLineRenderer = lr;
    }

    private bool CanPlaceSpearInHand()
    {
        return !IsThrown && parent != null && (spearPosition - posToReach).magnitude < (spearPosition - trail.transform.position).magnitude;
    }

    private bool CanPlaceSpearInWorld()
    {
        return IsThrown && (this.player.position - posToReach).magnitude < (this.player.position - trail.transform.position).magnitude;
    }
}
