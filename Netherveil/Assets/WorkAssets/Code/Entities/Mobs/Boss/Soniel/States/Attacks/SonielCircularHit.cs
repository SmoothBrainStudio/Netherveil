// ---[ STATE ] ---
// replace "SonielCircularHit_STATEMACHINE" by your state machine class name.
//
// Here you can see an exemple of the CheckSwitchStates method:
// protected override void CheckSwitchStates()
// {
//      if (isRunning)
//      {
//          SwitchState(Factory.GetState<RunningState>());
//      }
// }

using StateMachine; // include all scripts about StateMachines
using UnityEngine;

public class SonielCircularHit : BaseState<SonielStateMachine>
{
    public SonielCircularHit(SonielStateMachine currentContext, StateFactory<SonielStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    enum CircularStates
    {
        DASH,
        ATTACK,
        THRUST
    }

    // anim hash
    int dashHash = Animator.StringToHash("Charge");
    int circularHash = Animator.StringToHash("Circular");
    int thrustHash = Animator.StringToHash("Thrust");

    // state
    CircularStates currentState;

    // timers
    float attackDuration = 0f;
    float[] circularAttackChargeTimers = new float[3];
    readonly float[] MAX_CIRCULAR_ATTACK_CHARGE = { 0, 0.5f, 0.3f };
    bool[] attackLaunched = { false, false, false };

    // ranges
    float attackRange = 4f;
    float dashRange = 10f;

    bool attackEnded = false;

    //vfx
    bool isSlashPlayed = false;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (attackEnded)
        {
            SwitchState(Factory.GetState<SonielTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        Context.PlayerHit = false;
        Context.Agent.isStopped = true;
        attackDuration = 0f;

        attackEnded = false;
        Context.Animator.SetBool("Walk", false);
        Context.Animator.SetBool(thrustHash, false);

        isSlashPlayed = false;

        if (Vector3.SqrMagnitude(Context.Player.transform.position - Context.transform.position) > attackRange * attackRange)
        {
            currentState = CircularStates.DASH;
            Context.Animator.ResetTrigger(dashHash);
            Context.Animator.SetTrigger(dashHash);
        }
        else
        {
            currentState = CircularStates.ATTACK;
            Context.Animator.ResetTrigger(circularHash);
            Context.Animator.SetTrigger(circularHash);
        }
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.PlayerHit = false;
        Context.Agent.isStopped = false;

        Context.Animator.SetBool(thrustHash, false);

        Context.AttackCooldown = 1f + Random.Range(-0.25f, 0.25f);

        // DEBUG
        Context.DisableHitboxes();
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        UpdateTimers();

        switch (currentState)
        {
            case CircularStates.DASH:
                Dash();
                break;
            case CircularStates.ATTACK:
                Attack();
                break;
            case CircularStates.THRUST:
                Thrust();
                break;

            default:
                Debug.LogError("wtf");
                break;
        }

        attackDuration += Time.deltaTime;
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<SonielStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    #region Extra methods
    void UpdateTimers()
    {
        int currentAttack = (int)currentState;

        if (circularAttackChargeTimers[currentAttack] < MAX_CIRCULAR_ATTACK_CHARGE[currentAttack])
        {
            circularAttackChargeTimers[currentAttack] += Time.deltaTime;
        }
    }

    void Dash()
    {
        int currentAttack = (int)currentState;

        if (!attackLaunched[currentAttack])
        {
            Context.Agent.isStopped = false;

            // lance l'attaque
            if (circularAttackChargeTimers[currentAttack] >= MAX_CIRCULAR_ATTACK_CHARGE[currentAttack])
            {
                // augmente la speed
                Context.Stats.SetCoeffValue(Stat.SPEED, 1.75f);

                // se dirige vers le joueur
                Vector3 mobToPlayer = Context.Player.transform.position - Context.transform.position;
                float distanceToPlayer = mobToPlayer.magnitude - 0.5f;
                distanceToPlayer = Mathf.Clamp(distanceToPlayer, 0f, dashRange);
                Context.MoveTo(Context.transform.position + mobToPlayer.normalized * Mathf.Min(distanceToPlayer, dashRange));

                attackLaunched[currentAttack] = true;
            }
            else
            {
                Quaternion lookRotation = Quaternion.LookRotation(Context.Player.transform.position, Context.transform.position);
                lookRotation.x = 0;
                lookRotation.z = 0;

                Context.transform.rotation = Quaternion.Slerp(Context.transform.rotation, lookRotation, 5f * Time.deltaTime);
            }

        }
        else // effectue l'attaque
        {
            Context.Sounds.run.Play(Context.transform.position);

            // vérifie la collision avec la hitbox du dash
            if (!Context.PlayerHit)
            {
                Context.AttackCollide(Context.Attacks[(int)SonielStateMachine.SonielAttacks.CIRCULAR_CHARGE].data, debugMode: Context.DebugMode);
            }

            if (Context.Agent.remainingDistance <= Context.Agent.stoppingDistance || Context.PlayerHit)
            {
                Context.Agent.isStopped = true;

                // rétablit la speed
                Context.Stats.SetCoeffValue(Stat.SPEED, 1f);

                Context.Animator.ResetTrigger(circularHash);
                Context.Animator.SetTrigger(circularHash);

                Context.Sounds.run.Stop();

                SwitchAttack(currentAttack, CircularStates.ATTACK);

                // DEBUG
                if (Context.DebugMode)
                    Context.DisableHitboxes();
            }
        }
    }

    void Attack()
    {
        int currentAttack = (int)currentState;

        if (IsAttackLaunched(currentAttack)) // effectue l'attaque en elle même
        {
            if (!isSlashPlayed)
            {
                Context.SlashVFX.Play();
                isSlashPlayed = true;
            }

            // vérifie la collision
            if (!Context.PlayerHit)
            {
                Context.Sounds.slashVoid.Play(Context.transform.position);
                Context.AttackCollide(Context.Attacks[(int)SonielStateMachine.SonielAttacks.CIRCULAR_ATTACK].data, debugMode: Context.DebugMode);
            }
            else
            {
                Context.Sounds.slash.Play(Context.transform.position);
            }

            // fix animation
            if (attackDuration >= Context.Animator.GetCurrentAnimatorClipInfo(0).Length - 1f)
            {
                if (Context.HasLeftArm)
                {
                    if (Context.PlayerHit && Random.Range(0, 11) >= 0) // lance l'estoc avec 50% de chance s'il a déjà touché le joueur (et qu'il a son bras gauche)
                    {
                        Context.Animator.SetBool(thrustHash, true);
                    }
                }

                // à la fin de l'attaque, ...
                if (attackDuration >= Context.Animator.GetCurrentAnimatorClipInfo(0).Length)
                {
                    if (Context.Animator.GetBool(thrustHash))
                    {
                        SwitchAttack(currentAttack, CircularStates.THRUST);

                        Context.PlayerHit = false;

                        // DEBUG
                        if (Context.DebugMode)
                            Context.DisableHitboxes();
                    }
                    else attackEnded = true; // sort du state
                }
            }
        }
        else // rotation
        {
            Quaternion lookRotation = Quaternion.LookRotation(Context.Player.transform.position, Context.transform.position);
            lookRotation.x = 0;
            lookRotation.z = 0;

            Context.transform.rotation = Quaternion.Slerp(Context.transform.rotation, lookRotation, 5f * Time.deltaTime);
        }
    }

    void Thrust()
    {
        int currentAttack = (int)currentState;

        if (IsAttackLaunched(currentAttack))
        {
            Context.Sounds.thrust.Play(Context.transform.position);

            Context.Animator.SetBool(thrustHash, false);

            // vérifie la collision avec la hitbox de l'estoc
            if (!Context.PlayerHit)
            {
                Context.AttackCollide(Context.Attacks[(int)SonielStateMachine.SonielAttacks.CIRCULAR_THRUST].data, debugMode: Context.DebugMode);
            }

            if (attackDuration > Context.Animator.GetCurrentAnimatorClipInfo(0).Length)
            {
                // sort du state
                attackEnded = true;
            }
        }
    }

    bool IsAttackLaunched(int _currentAttack)
    {
        if (!attackLaunched[_currentAttack])
        {
            Context.LookAtTarget(Context.Player.transform.position);
            if (circularAttackChargeTimers[_currentAttack] >= MAX_CIRCULAR_ATTACK_CHARGE[_currentAttack]) // lance l'attaque après un délai
            {
                attackLaunched[_currentAttack] = true;
            }
        }

        return attackLaunched[_currentAttack];
    }

    void SwitchAttack(int _currentAttack, CircularStates _nextAttack)
    {
        attackLaunched[_currentAttack] = false;
        circularAttackChargeTimers[_currentAttack] = 0f;

        attackDuration = 0f;

        currentState = _nextAttack;
    }

    #endregion
}
