using StateMachine; // include all script about stateMachine
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class SonielStateMachine : Mobs, ISoniel
{
    [Serializable]
    public class SonielSounds
    {
        public Sound hit;
        public Sound walk;
        public Sound death;
        public Sound run;
        public Sound launchSword;
        public Sound retrieveSword;
        public Sound swordSpinning;
        public Sound swordHitMap;
        public Sound slash;
        public Sound slashVoid;
        public Sound thrust;
        public Sound bossHitMap;
        public Sound multipleSlash;
        public Sound music;
    }

    public enum SonielAttacks
    {
        CIRCULAR_CHARGE,
        CIRCULAR_ATTACK,
        CIRCULAR_THRUST,
        BERSERK,
        SPINNING_SWORDS_LEFT,
        SPINNING_SWORDS_RIGHT
    }

    [HideInInspector]
    public BaseState<SonielStateMachine> currentState;
    private StateFactory<SonielStateMachine> factory;

    // mob parameters
    private IAttacker.AttackDelegate onAttack;
    private IAttacker.HitDelegate onHit;
    Hero player = null;
    [SerializeField] SonielSounds sounds;
    public VisualEffect SlashVFX;
    [SerializeField] List<NestedList<Collider>> attackColliders;
    bool phaseTwo = false;
    bool playerHit = false;
    float attackCooldown = 1f;
    float initialHP;
    CameraUtilities cameraUtilities;

    [Header("Spinning swords")]
    [SerializeField] Transform[] wrists;
    [SerializeField] SonielProjectile[] swords;
    bool[] tiedArms = { true, true };
    float collisionImmuneTimer = 0f;
    readonly float MAX_COLLISION_IMMUNE_COOLDOWN = 0.3f;

    // anim hash
    int deathHash;

    GameObject gameMusic;

    // CINEMATICS
    [SerializeField] private BossCinematic cinematic;
    private bool isInCinematic = false;
    public bool IsInCinematic { get => isInCinematic; set => isInCinematic = value; }

    // DEBUG
    bool debugMode = false;

    #region getters/setters
    public List<Status> StatusToApply { get => statusToApply; }
    public IAttacker.AttackDelegate OnAttack { get => onAttack; set => onAttack = value; }
    public IAttacker.HitDelegate OnAttackHit { get => onHit; set => onHit = value; }
    public Animator Animator { get => animator; }
    public SonielSounds Sounds { get => sounds; }
    public List<NestedList<Collider>> Attacks { get => attackColliders; }
    public Hero Player { get => player; }
    public bool PhaseTwo { get => phaseTwo; }
    public bool PlayerHit { get => playerHit; set => playerHit = value; }
    public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }
    public CameraUtilities CameraUtilities { get => cameraUtilities; }

    // spinning swords
    public bool HasLeftArm { get => tiedArms[0]; set => tiedArms[0] = value; }
    public bool HasRightArm { get => tiedArms[1]; set => tiedArms[1] = value; }
    public Transform[] Wrists { get => wrists; }
    public SonielProjectile[] Swords { get => swords; }

    // DEBUG
    public bool DebugMode { get => debugMode; }

    #endregion

    protected override void OnDisable()
    {
        base.OnDisable();
        if (gameMusic != null)
        {
            gameMusic.SetActive(true);
        }
        sounds.music.Stop();

        StopAllCoroutines();
    }

    protected override void Start()
    {
        base.Start();

        gameMusic = GameObject.FindGameObjectWithTag("GameMusic");
        if (gameMusic != null)
        {
            gameMusic.SetActive(false);
        }

        factory = new StateFactory<SonielStateMachine>(this);
        currentState = factory.GetState<SonielTriggeredState>();
  
        animator.SetBool("Walk", true);

        // animation hash
        deathHash = Animator.StringToHash("Death");

        player = Utilities.Hero;

        initialHP = stats.GetValue(Stat.HP);

        cameraUtilities = Camera.main.GetComponent<CameraUtilities>();

        // null ref de con
        swords[0].SetSounds(sounds.swordHitMap, sounds.swordSpinning);
        swords[1].SetSounds(sounds.swordHitMap, sounds.swordSpinning);

        sounds.music.Play(false);

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

        if (sounds.music.GetState() == FMOD.Studio.PLAYBACK_STATE.STOPPING)
        {
            sounds.music.Play(false);
        }

        phaseTwo = stats.GetValue(Stat.HP) <= initialHP / 2f;

        currentState.Update();

        if ((!tiedArms[0] || !tiedArms[1]) && currentState is not SonielDeathState)
        {
            UpdateProjectiles();
        }
    }

    #region MOB_METHODS
    public void ApplyDamage(int _value, IAttacker attacker, bool notEffectDamage = true)
    {
        ApplyDamagesMob(_value, sounds.hit, Death, notEffectDamage);
    }

    public void Attack(IDamageable damageable, int additionalDamages = 0)
    {
        int damages = (int)stats.GetValue(Stat.ATK);
        damages += additionalDamages;

        onHit?.Invoke(damageable, this);
        damageable.ApplyDamage(damages, this);
        //ApplyKnockback(damageable, this);

        sounds.hit.Play(transform.position);
    }

    public void Death()
    {
        animator.speed = 1;
        OnDeath?.Invoke(transform.position);
        Utilities.Hero.OnKill?.Invoke(this);

        sounds.death.Play(transform.position);
        sounds.walk.Stop();
        sounds.run.Stop();
        sounds.multipleSlash.Stop();
        sounds.music.Stop();

        if (gameMusic != null)
            gameMusic.SetActive(true);

        animator.ResetTrigger(deathHash);
        animator.SetTrigger(deathHash);

        for (int i = 0; i < 2; i++)
        {
            swords[i].transform.parent = null;
            swords[i].enabled = false;

            Rigidbody rb = swords[i].GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            Destroy(swords[i].gameObject, 4.07f + .5f);
        }

        currentState = factory.GetState<SonielDeathState>();
    }

    public void MoveTo(Vector3 posToMove)
    {
        if (!agent.enabled || IsFreeze)
            return;

        agent.SetDestination(posToMove);
    }
    #endregion

    #region Extra methods
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

    void UpdateProjectiles()
    {
        if (collisionImmuneTimer >= MAX_COLLISION_IMMUNE_COOLDOWN)
        {
            if (currentState is not SonielCircularHit)
            {
                if (!HasLeftArm)
                {
                    AttackCollide(Attacks[(int)SonielAttacks.SPINNING_SWORDS_LEFT].data, debugMode: debugMode);
                }
                if (!HasRightArm)
                {
                    AttackCollide(Attacks[(int)SonielAttacks.SPINNING_SWORDS_RIGHT].data, debugMode: debugMode);
                }

                if (PlayerHit)
                {
                    collisionImmuneTimer = 0f;
                    PlayerHit = false;

                    // DEBUG
                    if (debugMode)
                        DisableHitboxes();
                }
            }
        }
        else collisionImmuneTimer += Time.deltaTime;
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
