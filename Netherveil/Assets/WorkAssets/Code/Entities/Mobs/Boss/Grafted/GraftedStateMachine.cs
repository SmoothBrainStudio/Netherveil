//--- [STATE MACHINE]-- -
//"factory" is use to get all state possible
// "currentState" can be set in the start with : currentState = factory.GetState<YOUR_STATE>();

using StateMachine; // include all script about stateMachine
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class GraftedStateMachine : Mobs, IGrafted
{
    public enum Attacks
    {
        THRUST,
        DASH,
        AOE
    }

    [System.Serializable]
    public class GraftedSounds
    {
        public Sound deathSound;
        public Sound hitSound;
        public Sound projectileLaunchedSound;
        public Sound projectileHitMapSound;
        public Sound thrustSound;
        public Sound introSound;
        public Sound retrievingProjectileSound;
        public Sound fallSound;
        public Sound dashSound;
        public Sound walkingSound;
        public Sound thrustMapSound;
        public Sound music;

        public void StopAllSounds()
        {
            deathSound.Stop();
            deathSound.Stop();
            hitSound.Stop();
            projectileLaunchedSound.Stop();
            projectileHitMapSound.Stop();
            thrustSound.Stop();
            introSound.Stop();
            retrievingProjectileSound.Stop();
            fallSound.Stop();
            dashSound.Stop();
            walkingSound.Stop();
            thrustMapSound.Stop();
            music.Stop();
        }
    }

    [HideInInspector]
    public BaseState<GraftedStateMachine> currentState;
    private StateFactory<GraftedStateMachine> factory;

    private IAttacker.AttackDelegate onAttack;
    private IAttacker.HitDelegate onHit;

    GameObject gameMusic;

    [Header("Sounds")]
    [SerializeField] private GraftedSounds sounds;

    Hero player = null;
    bool playerHit = false;
    float attackCooldown = 0;
    float height;

    [SerializeField] float rotationSpeed = 5f;

    [Header("Boss Attack Hitboxes")]
    [SerializeField] List<NestedList<Collider>> attackColliders;

    [Header("Thrust")]
    [SerializeField] float thrustCharge = 1f;
    [SerializeField] float thrustDuration = 1f;

    [Header("Dash")]
    [SerializeField] float aoeDuration;
    [SerializeField] float dashSpeed = 5f;
    [SerializeField] float dashRange;

    [Header("Range")]
    [SerializeField] GameObject projectilePrefab;
    GraftedProjectile projectile;

    [Header("VFXs")]
    [SerializeField] VisualEffect dashVFX;
    [SerializeField] VisualEffect tripleThrustVFX;

    CameraUtilities cameraUtilities;

    bool freezeRotation = false;

    // CINEMATICS
    [SerializeField] private BossCinematic cinematic;
    private bool isInCinematic = false;
    public bool IsInCinematic { get => isInCinematic; set => isInCinematic = value; }

    #region Getters/Setters
    public IAttacker.AttackDelegate OnAttack { get => onAttack; set => onAttack = value; }
    public IAttacker.HitDelegate OnAttackHit { get => onHit; set => onHit = value; }
    public List<Status> StatusToApply => statusToApply;
    public Hero Player { get => player; }
    public Animator Animator { get => animator; }
    public List<NestedList<Collider>> AttackColliders { get => attackColliders; }
    public GraftedProjectile Projectile { get => projectile; set => projectile = value; }
    public GraftedSounds Sounds { get => sounds; }
    public GameObject ProjectilePrefab { get => projectilePrefab; }
    public VisualEffect TripleThrustVFX { get => tripleThrustVFX; }
    public VisualEffect DashVFX { get => dashVFX; }
    public CameraUtilities CameraUtilities { get => cameraUtilities; }
    public bool PlayerHit { get => playerHit; set => playerHit = value; }
    public float Cooldown { get => attackCooldown; set => attackCooldown = value; }
    public float Height { get => height; }
    public bool FreezeRotation { set => freezeRotation = value; }

    public float ThrustCharge { get => thrustCharge; }
    public float ThrustDuration { get => thrustDuration; }
    public float AOEDuration { get => aoeDuration; }
    public float DashSpeed { get => dashSpeed; }
    public float DashRange { get => dashRange; }

    #endregion

    // DEBUG
    protected override void Awake()
    {
        base.Awake();
        gameMusic = GameObject.FindGameObjectWithTag("GameMusic");
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (gameMusic != null)
        {
            gameMusic.SetActive(false);
        }

        sounds.music.Play();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (gameMusic != null)
        {
            gameMusic.SetActive(true);
        }
        sounds.introSound.Stop();
        sounds.music.Stop();

        sounds.StopAllSounds();

        StopAllCoroutines();
    }

    protected override void Start()
    {
        base.Start();

        factory = new StateFactory<GraftedStateMachine>(this);
        currentState = factory.GetState<GraftedTriggeredState>();

        cameraUtilities = Camera.main.GetComponent<CameraUtilities>();

        height = GetComponentInChildren<Renderer>().bounds.size.y;

        tripleThrustVFX.transform.parent = null;
        tripleThrustVFX.transform.rotation = Quaternion.identity;
        tripleThrustVFX.Play();
        player = FindObjectOfType<Hero>();

        // Cinematics
        cinematic.Play();
        isInCinematic = true;
    }

    protected override void Update()
    {
        if (isInCinematic)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Utilities.Hero.transform.position);
            return;
        }

        if (IsFreeze || IsSpawning)
            return;

        base.Update();
        currentState.Update();

        if (!freezeRotation && agent.velocity.sqrMagnitude == 0f)
        {
            Vector3 mobToPlayer = player.transform.position - transform.position;
            mobToPlayer.y = 0f;

            Quaternion lookRotation = Quaternion.LookRotation(mobToPlayer);
            lookRotation.x = 0;
            lookRotation.z = 0;

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }

    #region Mob methods


    public void Attack(IDamageable damageable, int additionalDamages = 0)
    {
        int damages = (int)stats.GetValue(Stat.ATK);
        damages += additionalDamages;

        onHit?.Invoke(damageable, this);
        damageable.ApplyDamage(damages, this);
        //ApplyKnockback(damageable, this);

        //sounds.hitSound.Play(transform.position);
    }

    public void ApplyDamage(int _value, IAttacker attacker, bool notEffectDamage = true)
    {
        //if ((Vector3.Dot(player.transform.position - transform.position, transform.forward) < 0 && !hasProjectile)
        //    || currentAttack == Attacks.RANGE)
        //{
        //    _value *= 2;
        //}

        ApplyDamagesMob(_value, sounds.hitSound, Death, notEffectDamage);
    }

    public void Death()
    {
        animator.speed = 1;

        OnDeath?.Invoke(transform.position);
        Utilities.Hero.OnKill?.Invoke(this);

        animator.SetBool("Death", true);

        sounds.StopAllSounds();
        sounds.deathSound.Play(transform.position);

        agent.isStopped = true;

        freezeRotation = true;

        if (projectile != null && projectile.gameObject != null)
        {
            Destroy(projectile.gameObject);
        }

        currentState = factory.GetState<GraftedDeathState>();
    }

    public void MoveTo(Vector3 _pos)
    {
        if (!agent.enabled || IsFreeze)
            return;

        agent.SetDestination(_pos);
    }

    public void AttackCollide(List<Collider> colliders, bool _kb = false, bool debugMode = true)
    {
        if (debugMode)
        {
            foreach (Collider collider in colliders)
            {
                collider.gameObject.SetActive(true);
            }
        }

        Vector3 rayOffset = Vector3.up / 2;

        foreach (Collider attackCollider in colliders)
        {
            Collider[] tab = PhysicsExtensions.CheckAttackCollideRayCheck(attackCollider, transform.position + rayOffset, "Player", LayerMask.GetMask("Map"));
            if (tab.Length > 0)
            {
                foreach (Collider col in tab)
                {
                    if (col.gameObject.GetComponent<Hero>() != null)
                    {
                        IDamageable damageable = col.gameObject.GetComponent<IDamageable>();
                        Attack(damageable);

                        if (_kb)
                        {
                            Vector3 knockbackDirection = new Vector3(-transform.forward.z, 0, transform.forward.x);

                            if (Vector3.Cross(transform.forward, player.transform.position - transform.position).y > 0)
                            {
                                knockbackDirection = -knockbackDirection;
                            }

                            ApplyKnockback(damageable, this, knockbackDirection);
                        }

                        playerHit = true;
                        return;
                    }
                }
            }
        }
    }

    public void DisableHitboxes()
    {
        foreach (NestedList<Collider> attack in attackColliders)
        {
            foreach (Collider attackCollider in attack.data)
            {
                attackCollider.gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region EDITOR
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //if (!Selection.Contains(gameObject))
        //    return;

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
