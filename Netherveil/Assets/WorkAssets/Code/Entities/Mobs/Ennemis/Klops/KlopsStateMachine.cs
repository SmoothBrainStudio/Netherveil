using UnityEngine;
using StateMachine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine.VFX;

public class KlopsStateMachine : Mobs, IKlops
{
    [System.Serializable]
    public class KlopsSounds
    {
        public Sound deathSound;
        public Sound attackSound;
        public Sound hitSound;
    }

    [HideInInspector]
    // state machine variables
    public BaseState<KlopsStateMachine> currentState;
    private StateFactory<KlopsStateMachine> factory;

    // mobs variables
    private IAttacker.AttackDelegate onAttack;
    private IAttacker.HitDelegate onHit;
    [SerializeField] private KlopsSounds klopsSounds;
    [SerializeField] float defaultVisionAngle = 145f;
    [SerializeField] GameObject fireballPrefab;
    [SerializeField] Transform fireballSpawn;
    [SerializeField] VisualEffect explodingVFX;
    public GameObject Fireball { get; set; }
    Hero player = null;

    // animation hash
    private int deathHash;

    // getters and setters
    public GameObject FireballPrefab { get => fireballPrefab; }
    public Transform FireballSpawn { get => fireballSpawn; }
    public float VisionRange { get => stats.GetValue(Stat.VISION_RANGE) * (currentState is not KlopsWanderingState ? 1.25f : 1f); }
    public float VisionAngle { get => player ? 360 : defaultVisionAngle; }
    public float FleeRange { get => stats.GetValue(Stat.ATK_RANGE); }
    public Hero Player { get => player; }
    public List<Status> StatusToApply { get => statusToApply; }
    public IAttacker.AttackDelegate OnAttack { get => onAttack; set => onAttack = value; }
    public IAttacker.HitDelegate OnAttackHit { get => onHit; set => onHit = value; }
    public BaseState<KlopsStateMachine> CurrentState { get => currentState; set => currentState = value; }
    public Entity[] NearbyEntities { get => nearbyEntities; }
    public Animator Animator { get => animator; }
    public KlopsSounds KlopsSound { get => klopsSounds; }
    public VisualEffect ExplodingVFX { get => explodingVFX; }

    protected override void Start()
    {
        base.Start();

        canTriggerTraps = false;
        factory = new StateFactory<KlopsStateMachine>(this);
        currentState = factory.GetState<KlopsWanderingState>();

        // hashing animation
        deathHash = Animator.StringToHash("Death");

        // opti variables
        maxFrameUpdate = 10;
        frameToUpdate = entitySpawn % maxFrameUpdate;
    }

    protected override void Update()
    {
        if (IsFreeze || IsSpawning)
            return;

        base.Update();
        currentState.Update();
    }

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

    public void Attack(IDamageable damageable, int additionalDamages = 0)
    {
        int damages = (int)stats.GetValue(Stat.ATK);
        damages += additionalDamages;

        onHit?.Invoke(damageable, this);
        damageable.ApplyDamage(damages, this);
        ApplyKnockback(damageable, this);

        klopsSounds.attackSound.Play(transform.position);
    }

    public void MoveTo(Vector3 posToMove)
    {
        if (!agent.enabled || IsFreeze)
            return;

        agent.SetDestination(posToMove);
    }

    public void ApplyDamage(int _value, IAttacker attacker, bool notEffectDamage = true)
    {
        ApplyDamagesMob(_value, klopsSounds.hitSound, Death, notEffectDamage);

        if (currentState is KlopsWanderingState || (currentState is KlopsTriggeredState && !player))
        {
            currentState = factory.GetState<KlopsTriggeredState>();
            player = Utilities.Hero;
        }
    }

    public void Death()
    {
        animator.speed = 1;
        OnDeath?.Invoke(transform.position);
        Utilities.Hero.OnKill?.Invoke(this);
        klopsSounds.deathSound.Play(transform.position);

        animator.ResetTrigger(deathHash);
        animator.SetTrigger(deathHash);

        currentState = factory.GetState<KlopsDeathState>();

        if(Fireball != null && !Fireball.GetComponent<Fireball>().CanBeReflected)
        {
            Destroy(Fireball);
        }

        Animator.ResetTrigger("Death");
        Animator.SetTrigger("Death");
    }

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

