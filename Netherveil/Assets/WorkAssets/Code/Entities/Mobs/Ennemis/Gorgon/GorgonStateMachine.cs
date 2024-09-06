// ---[ STATE MACHINE ] ---
// "factory" is use to get all state possible
// "currentState" can be set in the start with : currentState = factory.GetState<YOUR_STATE>();

using StateMachine; // include all script about stateMachine
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class GorgonStateMachine : Mobs, IGorgon
{
    [Serializable]
    public class GorgonSounds
    {
        public Sound hitSFX;
        public Sound deathSFX;
    }

    // state machine variables
    [HideInInspector]
    public BaseState<GorgonStateMachine> currentState;
    private StateFactory<GorgonStateMachine> factory;

    // mob variables
    private IAttacker.HitDelegate onHit;
    private IAttacker.AttackDelegate onAttack;
    [Header("Range Parameters")]
    [SerializeField] private float timeBetweenAttack;
    [SerializeField] private float timeBetweenFleeing;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform hand;
    [SerializeField] float defaultVisionAngle = 145f;
    public VisualEffect dashVFX;
    [SerializeField] GorgonSounds gorgonSounds;

    Hero player = null;
    private int deathHash;
    Coroutine dashCoroutine;

    ///
    private bool isDashing = false;
    private bool isSmoothCoroutineOn = false;
    private bool hasLaunchAnim = false;
    private bool hasRemoveHead = false;

    bool canLoseAggro = true;
    float attackCooldown = 0f;

    float dashCooldown = 0f;
    float MAX_DASH_COOLDOWN = 2f;

    float fleeCooldown = 0f;

    #region getters/setters
    public IAttacker.HitDelegate OnAttackHit { get => onHit; set => onHit = value; }
    public IAttacker.AttackDelegate OnAttack { get => onAttack; set => onAttack = value; }
    public Hero Player { get => player; }
    public Animator Animator { get => animator; }
    public bool HasLaunchAnim { get => hasLaunchAnim; set => hasLaunchAnim = value; }
    public bool HasRemovedHead { get => hasRemoveHead; set => hasRemoveHead = value; }
    //private float DistanceToFlee { get => stats.GetValue(Stat.ATK_RANGE) / 1.5f; }
    public GameObject BombPrefab { get => bombPrefab; }
    public Transform Hand { get => hand; }
    public Coroutine DashCoroutine { get => dashCoroutine; set => dashCoroutine = value; }
    public float VisionAngle { get => !canLoseAggro ? 360 : (currentState is GorgonTriggeredState || currentState is GorgonAttackingState) && player != null ? 360 : defaultVisionAngle; }
    public float VisionRange { get => !canLoseAggro ? Stats.GetValue(Stat.VISION_RANGE) * 1.25f : Stats.GetValue(Stat.VISION_RANGE) * (currentState is GorgonTriggeredState || currentState is GorgonAttackingState ? 1.25f : 1f); }
    public bool CanLoseAggro { set => canLoseAggro = value; }
    public bool IsAttackAvailable { get => attackCooldown >= timeBetweenAttack; }
    public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }
    public bool IsDashAvailable { get => dashCooldown >= MAX_DASH_COOLDOWN; }
    public float DashCooldown { get => dashCooldown; set => dashCooldown = value; }
    public bool IsFleeAvailable { get => fleeCooldown >= timeBetweenFleeing; }
    public float FleeCooldown { get => fleeCooldown; set => fleeCooldown = value; }
    public float TimeBetweenAttacks { get => timeBetweenAttack; }
    public bool IsDashing { get => isDashing; }
    #endregion

    public List<Status> StatusToApply { get => statusToApply; }

    protected override void Start()
    {
        base.Start();

        canTriggerTraps = false;
        factory = new StateFactory<GorgonStateMachine>(this);
        currentState = factory.GetState<GorgonWanderingState>();

        // hashing animation
        deathHash = Animator.StringToHash("Death");

        // opti variables
        frameToUpdate = entitySpawn % maxFrameUpdate;
    }

    protected override void Update()
    {
        if (IsFreeze || IsSpawning)
            return;

        base.Update();

        if (currentState is not GorgonAttackingState)
            if (attackCooldown < timeBetweenAttack) attackCooldown += Time.deltaTime;

        if (currentState is not GorgonDashingState)
            if (dashCooldown < MAX_DASH_COOLDOWN) dashCooldown += Time.deltaTime;

        if (currentState is not GorgonFleeingState)
            if (fleeCooldown < timeBetweenFleeing) fleeCooldown += Time.deltaTime;

        currentState.Update();
    }

    #region MOB_METHODS
    protected override IEnumerator EntityDetection()
    {
        while (true)
        {
            if (!agent.enabled)
            {
                yield return null;
                continue;
            }

            if (!canLoseAggro)
            {
                player = Utilities.Hero;
                yield return null;
                continue;
            }

            nearbyEntities = PhysicsExtensions.OverlapVisionCone(transform.position, VisionAngle, VisionRange, transform.forward, LayerMask.GetMask("Entity"))
                    .Select(x => x.GetComponent<Hero>())
                    .Where(x => x != null && x != this)
                    .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
                    .ToArray();

            Entity playerEntity = nearbyEntities.FirstOrDefault(x => x.GetComponent<Hero>());
            player = playerEntity != null ? playerEntity.GetComponent<Hero>() : null;

            if (!player)
            {
                Hero tempPlayer = Utilities.Hero;
                if (Vector3.SqrMagnitude(tempPlayer.transform.position - transform.position) <= 4f)
                {
                    player = tempPlayer;
                }
            }

            yield return new WaitUntil(() => Time.frameCount % maxFrameUpdate == frameToUpdate);
        }
    }

    public void ApplyDamage(int _value, IAttacker attacker, bool notEffectDamage = true)
    {
        ApplyDamagesMob(_value, gorgonSounds.hitSFX, Death, notEffectDamage);

        if (!player)
        {
            currentState = factory.GetState<GorgonTriggeredState>();
            player = Utilities.Hero;
        }
    }

    public void Death()
    {
        animator.speed = 1;
        OnDeath?.Invoke(transform.position);
        Utilities.Hero.OnKill?.Invoke(this);
        animator.ResetTrigger(deathHash);
        animator.SetTrigger(deathHash);

        gorgonSounds.deathSFX.Play(transform.position);
        if (dashCoroutine != null) StopCoroutine(dashCoroutine);

        currentState = factory.GetState<GorgonDeathState>();
    }

    public void Attack(IDamageable damageable, int additionalDamages = 0)
    {
        int damages = (int)stats.GetValue(Stat.ATK);
        damages += additionalDamages;
        onHit?.Invoke(damageable, this);
        damageable.ApplyDamage(damages, this);
    }

    public void MoveTo(Vector3 posToMove)
    {
        if (!agent.enabled || IsFreeze)
            return;

        agent.SetDestination(posToMove);
    }
    #endregion

    #region EDITOR
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Selection.Contains(gameObject))
            return;

        DisplayVisionRange(VisionAngle, VisionRange);
        DisplayVisionRange(360, 2f);
        DisplayAttackRange(VisionAngle);
        DisplayInfos();
        DisplayWanderZone();
    }

    protected override void DisplayInfos()
    {
        Handles.Label(
        transform.position + transform.up,
        stats.GetEntityName() +
        "\n - Health : " + stats.GetValue(Stat.HP) +
        "\n - Speed : " + stats.GetValue(Stat.SPEED) +
        "\n - State : " + currentState?.ToString(),
        new GUIStyle()
        {
            alignment = TextAnchor.MiddleLeft,
            normal = new GUIStyleState()
            {
                textColor = Color.black
            }
        });
    }
#endif
    #endregion

    #region Extra methods

    public IEnumerator DashToPos(List<Vector3> listDashes)
    {
        isDashing = true;

        for (int i = 1; i < listDashes.Count; i++)
        {
            if (this.Stats.GetValue(Stat.SPEED) > 0)
            {
                StartCoroutine(GoSmoothToPosition(listDashes[i]));
                animator.ResetTrigger("Dash");
                animator.SetTrigger("Dash");
            }
            yield return new WaitUntil(() => isSmoothCoroutineOn == false);
        }

        isDashing = false;
    }

    private IEnumerator GoSmoothToPosition(Vector3 posToReach)
    {
        isSmoothCoroutineOn = true;
        NavMesh.SamplePosition(posToReach, out NavMeshHit hit, float.PositiveInfinity, NavMesh.AllAreas);
        posToReach = hit.position;
        float timer = 0;
        Vector3 basePos = this.transform.position;
        Vector3 newPos;
        // Face to his next direction
        this.transform.forward = posToReach - basePos;
        while (timer < 1f && this.IsAlive && !this.IsFreeze)
        {
            newPos = Vector3.Lerp(basePos, posToReach, timer);
            agent.Warp(newPos);
            timer += Time.deltaTime * 5;
            timer = timer > 1 ? 1 : timer;
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);

        isSmoothCoroutineOn = false;
    }

    public List<Vector3> GetDashesPath(Vector3 posToReach, int nbDash)
    {
        List<Vector3> path = new()
        {
            transform.position
        };

        NavMeshPath navPath = new();
        NavMesh.CalculatePath(transform.position, posToReach, -1, navPath);
        // First corner is initPos and last is endPos
        for (int i = 1; i < navPath.corners.Length - 1; i++)
        {
            path.Add(navPath.corners[i]);
        }
        float distance = Vector3.Distance(transform.position, posToReach);
        // If it's a straight line
        if (path.Count == 1)
        {
            for (int i = 1; i < nbDash; i++)
            {
                // We avoid y value because we only move in x and z
                Vector2 posToReach2D = new(posToReach.x, posToReach.z);

                // Virtually get the "current" position of the dasher ( get the position he reached after his previous dash )
                Vector2 curPos2D = new(path[i - 1].x, path[i - 1].z);

                Vector2 direction = posToReach2D - curPos2D;

                Vector2 posOnCone = transform.position;

                if (direction != Vector2.zero)
                    posOnCone = MathsExtension.GetRandomPointOnCone(curPos2D, direction, distance / nbDash, 60);

                Vector3 posOnCone3D = new(posOnCone.x, transform.position.y, posOnCone.y);
                if (NavMesh.SamplePosition(posOnCone3D, out var hit, 10, NavMesh.AllAreas))
                {
                    posOnCone3D = hit.position;
                    path.Add(posOnCone3D);
                }
            }
            //path.Add(posToReach);
        }

        // We finally add the position that we want to reach after every dash
        path.Add(posToReach);
        return path;
    }
    #endregion
}