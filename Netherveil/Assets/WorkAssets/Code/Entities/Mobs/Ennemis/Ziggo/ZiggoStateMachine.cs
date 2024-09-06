using UnityEngine;
using StateMachine; // include all script about stateMachine
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.VFX;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ZiggoStateMachine : Mobs, IZiggo
{
    [System.Serializable]
    public class ZiggoSounds
    {
        public Sound moveSound;
        public Sound deathSound;
        public Sound hitSound;
        public Sound eatSound;
        public Sound spitSound;
        public Sound splatterSound;
    }

    public enum ZiggoAttacks
    {
        DASH,
        SPIT
    };

    // state machine variables
    [HideInInspector] public BaseState<ZiggoStateMachine> currentState;
    private StateFactory<ZiggoStateMachine> factory;

    // mobs variables
    private IAttacker.AttackDelegate onAttack;
    private IAttacker.HitDelegate onHit;
    [SerializeField] private ZiggoSounds ziggoSounds;
    [SerializeField, Range(0f, 360f)] private float originalVisionAngle = 180.0f;

    private Hero player;
    bool playerHit = false;

    // animation hash
    private int deathHash;

    // attacks
    float dashCooldown = 0f;
    float spitCooldown = 0f;
    GameObject projectile;
    [Header("Attacks")]
    [SerializeField] Collider[] attackColliders;
    Coroutine spitAttackCoroutine = null;

    #region Getters/setters
    public List<Status> StatusToApply { get => statusToApply; }
    public IAttacker.AttackDelegate OnAttack { get => onAttack; set => onAttack = value; }
    public IAttacker.HitDelegate OnAttackHit { get => onHit; set => onHit = value; }
    public BaseState<ZiggoStateMachine> CurrentState { get => currentState; set => currentState = value; }
    public Entity[] NearbyEntities { get => nearbyEntities; }
    public Animator Animator { get => animator; }
    public Collider[] AttackColliders { get => attackColliders; }
    public ZiggoSounds Sounds { get => ziggoSounds; }
    public Hero Player { get => player; }
    public GameObject Projectile { get => projectile; }
    public Coroutine SpitAttackCoroutine { set => spitAttackCoroutine = value; }
    public bool PlayerHit { get => playerHit; set => playerHit = value; }
    public float VisionRange { get => stats.GetValue(Stat.VISION_RANGE) * (currentState is not ZiggoWanderingState ? 1.25f : 1f); }
    public float VisionAngle { get => player ? 360 : originalVisionAngle; }
    public float DashCooldown { get => dashCooldown; set => dashCooldown = value; }
    public float SpitCooldown { get => spitCooldown; set => spitCooldown = value; }
    #endregion

    protected override void Start()
    {
        base.Start();

        factory = new StateFactory<ZiggoStateMachine>(this);
        currentState = factory.GetState<ZiggoWanderingState>();

        // projectile
        projectile = GetComponentInChildren<ZiggoProjectile>(includeInactive: true).gameObject;
        projectile.SetActive(false);

        // anim hash
        deathHash = Animator.StringToHash("Death");

        // cooldowns
        dashCooldown = 0f;
        spitCooldown = 0f;

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

        animator.SetBool("Walk", agent.remainingDistance > agent.stoppingDistance);

        if (agent.hasPath)
        {
            Vector3 posToLookAt = agent.pathEndPosition;
            posToLookAt.y = transform.position.y;

            // rotate
            //Quaternion lookRotation = Quaternion.LookRotation(posToLookAt, transform.position);
            //lookRotation.x = 0;
            //lookRotation.z = 0;
            //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
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
        ApplyDamagesMob(_value, ziggoSounds.hitSound, Death, notEffectDamage);

        if (currentState is ZiggoWanderingState || (currentState is ZiggoTriggeredState && !player))
        {
            currentState = factory.GetState<ZiggoTriggeredState>();
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

        ziggoSounds.eatSound.Play(transform.position);
    }

    public void Death()
    {
        animator.speed = 1;
        OnDeath?.Invoke(transform.position);
        Utilities.Hero.OnKill?.Invoke(this);
        ziggoSounds.deathSound.Play(transform.position);
        animator.SetBool(deathHash, true);
        if (spitAttackCoroutine != null) StopCoroutine(spitAttackCoroutine);

        if (projectile != null)
        {
            Destroy(projectile.GetComponent<ZiggoProjectile>().PoisonBallVFX.gameObject);
            projectile.GetComponent<ZiggoProjectile>().PoisonPuddleVFX.Stop();
            Destroy(projectile);
        }

        currentState = factory.GetState<ZiggoDeathState>();
    }

    public void MoveTo(Vector3 posToMove)
    {
        if (!agent.enabled || IsFreeze)
            return;

        agent.SetDestination(posToMove);
        ziggoSounds.moveSound.Play(transform.position);
    }

    public void Move(Vector3 direction)
    {
        if (!agent.enabled)
            return;

        agent.Move(direction);
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
                    if (col.gameObject.CompareTag("Player"))
                    {
                        Attack(col.gameObject.GetComponent<IDamageable>());
                        playerHit = true;
                    }
                }
            }
        }
    }

    public void DisableHitboxes()
    {
        foreach (Collider attackCollider in attackColliders)
        {
            attackCollider.gameObject.SetActive(false);
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
        DisplayAttackRange(VisionAngle);
        DisplayInfos();
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
