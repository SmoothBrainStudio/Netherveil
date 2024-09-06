using StateMachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PestStateMachine : Mobs, IPest
{
    [System.Serializable]
    private class PestSounds
    {
        public Sound deathSound;
        public Sound takeDamageSound;
        public Sound attackHitSound;
        public Sound moveSound;
    }

    // state machine variables
    private BaseState<PestStateMachine> currentState;
    private StateFactory<PestStateMachine> factory;

    // mobs variables
    private IAttacker.AttackDelegate onAttack;
    private IAttacker.HitDelegate onHit;
    [SerializeField] private PestSounds pestSounds;
    [SerializeField, Range(0f, 360f)] private float angle = 180.0f;
    [SerializeField] private BoxCollider attackCollider;
    private Transform player;
    float dashTimer = 0f;

    // animation hash
    private int chargeInHash;
    private int chargeOutHash;
    private int deathHash;

    // charge attaque
    [SerializeField] float attackChargeDuration = 0.65f;

    bool playerHit = false;

    // getters and setters
    public List<Status> StatusToApply { get => statusToApply; }
    public IAttacker.AttackDelegate OnAttack { get => onAttack; set => onAttack = value; }
    public IAttacker.HitDelegate OnAttackHit { get => onHit; set => onHit = value; }
    public BaseState<PestStateMachine> CurrentState { get => currentState; set => currentState = value; }
    public Entity[] NearbyEntities { get => nearbyEntities; }
    public Animator Animator { get => animator; }
    public int ChargeInHash { get => chargeInHash; }
    public int ChargeOutHash { get => chargeOutHash; }
    public BoxCollider AttackCollider { get => attackCollider; }
    public Transform Player { get => player; set => player = value; }
    public float DashSpeed { get => Stats.GetValue(Stat.SPEED) * 2f; }
    public float VisionAngle { get => (currentState is PestTriggeredState || currentState is PestAttackingState) && Player != null ? 360 : angle; }
    public float VisionRange { get => Stats.GetValue(Stat.VISION_RANGE) * (currentState is PestTriggeredState || currentState is PestAttackingState ? 1.25f : 1f); }
    public float idleTimer { set => dashTimer = value; }
    public float MovementDelay { get => (currentState is PestTriggeredState ? 1.5f : 1.8f); }
    public bool CanMove { get => dashTimer > MovementDelay; }
    public float AttackChargeDuration { get => attackChargeDuration; }
    public bool PlayerHit { get => playerHit; set => playerHit = value; }

    protected override void Start()
    {
        base.Start();

        factory = new StateFactory<PestStateMachine>(this);
        currentState = factory.GetState<PestWanderingState>();

        // common initialization
        GetComponent<Knockback>().onObstacleCollide += ApplyDamage;

        // hashing animation
        chargeInHash = Animator.StringToHash("ChargeIn");
        chargeOutHash = Animator.StringToHash("ChargeOut");
        deathHash = Animator.StringToHash("Death");

        // opti variables
        frameToUpdate = entitySpawn % maxFrameUpdate;
        
        idleTimer = MovementDelay / 2f;

        OnFreeze += PestStateMachine_OnFreeze;
    }

    private void PestStateMachine_OnFreeze()
    {
    }

    protected override void Update()
    {
        if (IsFreeze || IsSpawning)
            return;

        base.Update();
        currentState.Update();

        if (!CanMove) { dashTimer += Time.deltaTime; }
    }

    #region MOBS METHODS
    protected override IEnumerator EntityDetection()
    {
        while (true)
        {
            if (!agent.enabled)
            {
                yield return null;
                continue;
            }

            nearbyEntities = PhysicsExtensions.OverlapVisionCone(transform.position, VisionAngle, VisionRange, transform.forward, LayerMask.GetMask("Entity"))
                    .Select(x => x.GetComponent<Entity>())
                    .Where(x => x != null && x != this)
                    .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
                    .ToArray();

            Entity playerEntity = nearbyEntities.FirstOrDefault(x => x.GetComponent<Hero>());
            player = playerEntity ? playerEntity.transform : null;

            if (!player)
            {
                Hero tempPlayer = Utilities.Hero;
                if (Vector3.SqrMagnitude(tempPlayer.transform.position - transform.position) <= 4f)
                {
                    player = tempPlayer.transform;
                }
            }

            yield return new WaitUntil(() => Time.frameCount % maxFrameUpdate == frameToUpdate);
        }
    }

    public void ApplyDamage(int _value, IAttacker attacker, bool notEffectDamage = true)
    {
        ApplyDamagesMob(_value, pestSounds.takeDamageSound, Death, notEffectDamage);

        if (currentState is not PestAttackingState && currentState is not PestDeathState && !player)
        {
            currentState = factory.GetState<PestTriggeredState>();
            player = Utilities.Hero.transform;
        }
    }

    public void Attack(IDamageable damageable, int additionalDamages = 0)
    {
        int damages = (int)stats.GetValue(Stat.ATK);
        damages += additionalDamages;

        onHit?.Invoke(damageable, this);
        damageable.ApplyDamage(damages, this);
        //ApplyKnockback(damageable, this);

        pestSounds.attackHitSound.Play(transform.position);
    }

    public void Death()
    {
        animator.speed = 1;
        OnDeath?.Invoke(transform.position);
        Utilities.Hero.OnKill?.Invoke(this);
        pestSounds.deathSound.Play(transform.position);
        animator.ResetTrigger(deathHash);
        animator.SetTrigger(deathHash);

        
        currentState = factory.GetState<PestDeathState>();
    }

    public void MoveTo(Vector3 posToMove)
    {
        if (!agent.enabled || IsFreeze)
            return;

        agent.SetDestination(posToMove);
        pestSounds.moveSound.Play(transform.position);
    }
    #endregion

    #region EDITOR
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Selection.Contains(gameObject))
            return;

        DisplayVisionRange(VisionAngle, VisionRange);
        DisplayVisionRange(360f, 2f);
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
}