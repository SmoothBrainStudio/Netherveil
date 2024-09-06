// ---[ STATE MACHINE ] ---
// "factory" is use to get all state possible
// "currentState" can be set in the start with : currentState = factory.GetState<YOUR_STATE>();

using StateMachine; // include all script about stateMachine
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using FMODUnity;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ErecrosStateMachine : Mobs, IFinalBoss
{
    [HideInInspector]
    public BaseState<ErecrosStateMachine> currentState;
    private StateFactory<ErecrosStateMachine> factory;

    [Serializable]
    public class ErecrosSounds
    {
        public Sound intro;
        public Sound miniHit; //
        public Sound miniDeath; //
        public Sound maxiHit; //
        public Sound maxiDeath; //
        public Sound teleport; //
        public Sound dash; //
        public Sound clone; //
        public Sound levitation;
        public Sound prison; //
        public Sound shieldHit; //
        public Sound shockwave;
        public Sound invocation; //
        public Sound throwWeapon; //
        public Sound walk;
    }

    public enum ErecrosColliders
    {
        DASH,
        SPINNING_ATTACK
    }

    private IAttacker.AttackDelegate onAttack;
    private IAttacker.HitDelegate onHit;
    Hero player = null;
    [SerializeField] ErecrosSounds sounds;
    [SerializeField] List<NestedList<Collider>> attackColliders;
    bool playerHit = false;
    float attackCooldown = 1f;
    float initialHP;
    CameraUtilities cameraUtilities;

    GameObject gameMusic;
    [SerializeField] private StudioEventEmitter musicEmitter;

    float height = 0f;

    bool toggleDebugMode = false;

    public int part;
    public int phase;

    [SerializeField] GameObject nextPartGO;

    [SerializeField] GameObject[] enemiesPrefabs;

    [SerializeField] Volume volume;
    [SerializeField] VisualEffect shieldVFX;
    [SerializeField] VisualEffect shockwaveVFX;
    [SerializeField] VisualEffect teleportVFX;
    [SerializeField] VisualEffect prisonVFX;
    [SerializeField] VisualEffect thunderVFX;

    [SerializeField] GameObject clonePrefab;
    [SerializeField] GameObject propsParent;
    [SerializeField] SphereCollider summonCollider;

    Rigidbody[] props;
    List<Collider> propsColliders = new();

    [SerializeField] Transform roomCenter;

    Type lastAttack = null;

    List<GameObject> clones = new();
    Coroutine currentCoroutine = null;

    // CINEMATICS
    [SerializeField] private BossCinematic cinematic;
    private bool isInCinematic = false;
    public bool IsInCinematic { get => isInCinematic; set => isInCinematic = value; }

    #region Getters/Setters
    public List<Status> StatusToApply { get => statusToApply; }
    public IAttacker.AttackDelegate OnAttack { get => onAttack; set => onAttack = value; }
    public IAttacker.HitDelegate OnAttackHit { get => onHit; set => onHit = value; }
    public Animator Animator { get => animator; }
    public List<NestedList<Collider>> Attacks { get => attackColliders; }
    public ErecrosSounds Sounds { get => sounds; }
    public Hero Player { get => player; }
    public bool PlayerHit { get => playerHit; set => playerHit = value; }
    public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }
    public CameraUtilities CameraUtilities { get => cameraUtilities; }
    public GameObject[] EnemiesPrefabs { get => enemiesPrefabs; }
    public int CurrentPart { get => part; }
    public int CurrentPhase { get => phase; }
    public float Height { get => 3.5f; }
    public Type LastAttack { get => lastAttack; set => lastAttack = value; }
    public Transform RoomCenter { get => roomCenter; }
    public List<GameObject> Clones { get => clones; set => clones = value; }
    public Coroutine CurrentCouroutine { get => currentCoroutine; set => currentCoroutine = value; }

    public Vignette Vignette
    {
        get
        {
            volume.profile.TryGet(out Vignette vignette);
            return vignette;
        }
    }
    public VisualEffect ShieldVFX { get => shieldVFX; }
    public VisualEffect ShockwaveVFX { get => shockwaveVFX; }
    public VisualEffect PrisonVFX { get => prisonVFX; }
    public VisualEffect TeleportVFX { get => teleportVFX; }
    public GameObject ClonePrefab { get => clonePrefab; }
    public GameObject PropsParent { get => propsParent; }
    public Rigidbody[] PropsRB { get => props; }
    public List<Collider> PropsColliders { get => propsColliders; }
    public SphereCollider SummonCollider { get => summonCollider; }

    public bool DebugMode { get => toggleDebugMode; set => toggleDebugMode = value; }

    public bool HasDoneSummoningPhase = false;
    #endregion

    protected override void Start()
    {
        base.Start();
        bossLifeBar.ValueChanged(stats.GetValue(Stat.HP));

        factory = new StateFactory<ErecrosStateMachine>(this);
        currentState = factory.GetState<ErecrosTriggeredState>();

        player = Utilities.Hero;
        initialHP = stats.GetValue(Stat.HP);
        cameraUtilities = Camera.main.GetComponent<CameraUtilities>();

        height = GetComponentInChildren<Renderer>().bounds.size.y;

        if (part > 1)
        {
            Destroy(Instantiate(GameResources.Get<GameObject>("VFX_Death"), animator.transform.parent.position, Quaternion.identity), 30f);
        }

        if (part == 1)
        {
            gameMusic = GameObject.FindGameObjectWithTag("GameMusic");
            if (gameMusic != null)
            {
                gameMusic.SetActive(false);
            }

            musicEmitter.Play();

            sounds.intro.Play(transform.position);

            // Cinematics
            cinematic.Play();
            isInCinematic = true;
        }
        else if (part == 2)
        {
            props = propsParent.GetComponentsInChildren<Rigidbody>();
            HasDoneSummoningPhase = false;
            bossLifeBar.ResetBars();

            foreach (Rigidbody prop in props)
            {
                propsColliders.Add(prop.gameObject.GetComponent<BoxCollider>());
            }
        }
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

        if (IsKnockbackable)
            IsKnockbackable = false;

        if (phase < 2)
        {
            if (stats.GetValue(Stat.HP) <= initialHP / 2f)
            {
                phase++;
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            toggleDebugMode = !toggleDebugMode;
        }

        base.Update();
        currentState.Update();
    }

    #region Mobs methods
    public void ApplyDamage(int _value, IAttacker attacker, bool notEffectDamage = true)
    {
        if (currentState is not ErecrosSummoningAttack)
        {
            ApplyDamagesMob(_value, part <= 1 ? sounds.miniHit : sounds.maxiHit, Death, notEffectDamage);
        }
        else
        {
            sounds.shieldHit.Play(transform.position, true);
            FloatingTextGenerator.CreateActionText(transform.position, "Blocked!");
        }
    }

    public void Attack(IDamageable damageable, int additionalDamages = 0)
    {
        int damages = (int)stats.GetValue(Stat.ATK);
        damages += additionalDamages;

        onHit?.Invoke(damageable, this);
        damageable.ApplyDamage(damages, this);
        //ApplyKnockback(damageable, this);
    }

    public void Death()
    {
        foreach (GameObject clone in clones)
        {
            Destroy(clone);
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        sounds.levitation.Stop();

        // Son
        if (part != 2) sounds.miniDeath.Play(transform.position); else sounds.maxiDeath.Play(transform.position);

        if (part < 3)
        {
            nextPartGO.SetActive(true);
            nextPartGO.transform.position = transform.position;
            Destroy(gameObject);
        }
        else
        {
            animator.speed = 1;
            OnDeath?.Invoke(transform.position);
            Utilities.Hero.OnKill?.Invoke(this);

            if (gameMusic != null)
                gameMusic.SetActive(true);

            musicEmitter.Stop();

            AudioManager.Instance.StopAllSounds();

            animator.ResetTrigger("Death");
            animator.SetTrigger("Death");

            currentState = factory.GetState<ErecrosDeathState>();
        }
    }

    public void MoveTo(Vector3 posToMove)
    {
        if (!agent.enabled || IsFreeze)
            return;

        agent.SetDestination(posToMove);
    }
    #endregion

    #region Extra methods
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
    #endregion

    #region EDITOR
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //if (!Selection.Contains(gameObject))
        //    return;

        DisplayAttackRange(360f);
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
