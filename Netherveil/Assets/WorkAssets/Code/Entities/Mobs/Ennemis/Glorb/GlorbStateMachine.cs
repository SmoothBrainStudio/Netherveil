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

public class GlorbStateMachine : Mobs, IGlorb
{
    [Serializable]
    public class GlorbSounds
    {
        public Sound shockwaveSFX;
        public Sound punchSFX;
        public Sound hitSFX;
        public Sound deathSFX;
    }

    // state machine variables
    [HideInInspector]
    public BaseState<GlorbStateMachine> currentState;
    private StateFactory<GlorbStateMachine> factory;

    // mob parameters
    private IAttacker.AttackDelegate onAttack;
    private IAttacker.HitDelegate onHit;
    [SerializeField] GlorbSounds glorbSounds;
    [SerializeField] CapsuleCollider shockwaveCollider;
    [SerializeField] Collider[] attackColliders;
    [SerializeField] float defaultVisionAngle = 100f;

    VFXStopper vfxStopper;
    Hero player = null;

    bool speAttackAvailable = true;
    float specialAttackTimer = 0f;
    readonly float SPECIAL_ATTACK_COOLDOWN = 3.5f;

    bool basicAttackAvailable = true;
    float basicAttackTimer = 0f;
    readonly float BASIC_ATTACK_COOLDOWN = 1f;

    // animation Hash
    int deathHash;
    int walkHash;

    CameraUtilities cameraUtilities;

    #region Getters/Setters
    public List<Status> StatusToApply { get => statusToApply; }
    public IAttacker.AttackDelegate OnAttack { get => onAttack; set => onAttack = value; }
    public IAttacker.HitDelegate OnAttackHit { get => onHit; set => onHit = value; }
    public CapsuleCollider ShockwaveCollider { get => shockwaveCollider; }
    public Collider[] AttackColliders { get => attackColliders; }
    public Hero Player { get => player; }
    public VFXStopper VFX { get => vfxStopper; }
    public Animator Animator { get => animator; }
    public GlorbSounds Sounds { get => glorbSounds; }
    public CameraUtilities CameraUtilities { get => cameraUtilities; }
    public float VisionAngle { get => (currentState is GlorbTriggeredState || currentState is GlorbAttackingState) && Player != null ? 360 : defaultVisionAngle; }
    public float VisionRange { get => Stats.GetValue(Stat.VISION_RANGE) * (currentState is GlorbTriggeredState || currentState is GlorbAttackingState ? 1.25f : 1f); }
    public bool IsSpeAttackAvailable { get => speAttackAvailable; }
    public bool IsBasicAttackAvailable { get => basicAttackAvailable; }
    public float AttackRange { get => speAttackAvailable ? shockwaveCollider.gameObject.transform.localScale.z / 2f : Stats.GetValue(Stat.ATK_RANGE); }
    public float SpecialAttackTimer { get => specialAttackTimer; set => specialAttackTimer = value; }
    public float BasicAttackTimer { get => basicAttackTimer; set => basicAttackTimer = value; }
    #endregion

    protected override void Start()
    {
        base.Start();

        factory = new StateFactory<GlorbStateMachine>(this);
        currentState = factory.GetState<GlorbWanderingState>();

        player = null;
        vfxStopper = GetComponent<VFXStopper>();

        // animation Hash
        deathHash = Animator.StringToHash("Death");
        walkHash = Animator.StringToHash("Walk");

        // opti variables
        frameToUpdate = entitySpawn % maxFrameUpdate;

        cameraUtilities = Camera.main.GetComponent<CameraUtilities>();

        OnFreeze += GlorbStateMachine_OnFreeze;
    }

    private void GlorbStateMachine_OnFreeze()
    {
    }

    protected override void Update()
    {
        if (IsFreeze || IsSpawning)
            return;

        base.Update();

        animator.SetBool(walkHash, currentState is GlorbAttackingState ? false : agent.remainingDistance > agent.stoppingDistance);

        if (currentState is not GlorbWanderingState)
        {
            UpdateAttacksTimers();
        }

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
        ApplyDamagesMob(_value, glorbSounds.hitSFX, Death, notEffectDamage);

        if (currentState is not GlorbAttackingState && currentState is not GlorbDeathState && !player)
        {
            currentState = factory.GetState<GlorbTriggeredState>();
            player = Utilities.Hero;
        }
    }

    public void Attack(IDamageable damageable, int additionalDamages = 0)
    {
        int damages = (int)stats.GetValue(Stat.ATK);
        damages += additionalDamages;

        onHit?.Invoke(damageable, this);
        damageable.ApplyDamage(damages, this);
        ApplyKnockback(damageable, this);

        glorbSounds.hitSFX.Play(transform.position);
    }

    public void Death()
    {
        animator.speed = 1;
        OnDeath?.Invoke(transform.position);
        Utilities.Hero.OnKill?.Invoke(this);

        glorbSounds.deathSFX.Play(transform.position);

        animator.ResetTrigger(deathHash);
        animator.SetTrigger(deathHash);

        currentState = factory.GetState<GlorbDeathState>();
    }

    public void MoveTo(Vector3 posToMove)
    {
        if (!agent.enabled || IsFreeze)
            return;

        agent.SetDestination(posToMove);
        //AudioManager.Instance.PlaySound(glorbSounds.walkSFX, transform.position);
    }

    public void AttackCollide(Collider _collider, bool debugMode = false)
    {
        if (debugMode)
        {
            _collider.gameObject.SetActive(true);
        }

        Collider[] tab = null;

        if (_collider is CapsuleCollider)
            tab = PhysicsExtensions.CapsuleOverlap(_collider as CapsuleCollider, LayerMask.GetMask("Entity"));
        else if (_collider is BoxCollider)
            tab = PhysicsExtensions.BoxOverlap(_collider as BoxCollider, LayerMask.GetMask("Entity"));
        else
            Debug.LogError("Type de collider non reconnu.");

        if (tab != null)
        {
            if (tab.Length > 0)
            {
                foreach (Collider col in tab)
                {
                    //if (col.gameObject.GetComponent<IDamageable>() != null && col.gameObject != gameObject)
                    if (col.CompareTag("Player"))
                    {
                        Attack(col.gameObject.GetComponent<IDamageable>());
                    }
                }
            }
        }
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
        DisplayAttackRange(VisionAngle, AttackRange);
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
    void UpdateAttacksTimers()
    {
        if (!speAttackAvailable) specialAttackTimer += Time.deltaTime;
        speAttackAvailable = specialAttackTimer >= SPECIAL_ATTACK_COOLDOWN;

        if (!basicAttackAvailable) basicAttackTimer += Time.deltaTime;
        basicAttackAvailable = basicAttackTimer >= BASIC_ATTACK_COOLDOWN;
    }
    #endregion
}
