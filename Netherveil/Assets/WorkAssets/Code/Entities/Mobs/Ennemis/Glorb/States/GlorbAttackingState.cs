// ---[ STATE ] ---
// replace "GlorbAttackingState_STATEMACHINE" by your state machine class name.
//
// Here you can see an exemple of CheckSwitchStates method:
// protected override void CheckSwitchStates()
// {
//      if (isRunning)
//      {
//          SwitchState(Factory.GetState<RunningState>());
//      }
// }

using StateMachine; // include all script about stateMachine
using System.Collections;
using UnityEngine;

public class GlorbAttackingState : BaseState<GlorbStateMachine>
{
    public GlorbAttackingState(GlorbStateMachine currentContext, StateFactory<GlorbStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    enum Attacks
    {
        BASIC,
        SPECIAL,
        NONE
    }

    Attacks currentAttack = Attacks.NONE;
    float stompDelay = 0f;
    float punchDelay = 0f;
    bool CoroutineBasicAttackStarted = false;
    const float timeBeforePrepare = 0.15f;
    const float timeBeforePunch = 0.85f;

    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
        if (currentAttack == Attacks.NONE)
        {
            if (Context.Player != null)
            {
                if (Vector3.Distance(Context.transform.position, Context.Player.transform.position) > Context.AttackRange)
                {
                    SwitchState(Factory.GetState<GlorbTriggeredState>());
                }
            }
            else
            {
                SwitchState(Factory.GetState<GlorbWanderingState>());
            }
        }
    }

    // This method will be call only one time before the update.
    protected override void EnterState()
    {
        Context.Agent.isStopped = true;
        stompDelay = 0f;
        punchDelay = 0f;
    }

    // This method will be call only one time after the last update.
    protected override void ExitState()
    {
        Context.Agent.isStopped = false;
        stompDelay = 0f;
        punchDelay = 0f;
    }

    // This method will be call every frame.
    protected override void UpdateState()
    {
        if (Context.IsSpeAttackAvailable && currentAttack != Attacks.BASIC)
        {
            if (stompDelay == 0f)
            {
                Context.Animator.ResetTrigger("Shockwave");
                Context.Animator.SetTrigger("Shockwave");
                currentAttack = Attacks.SPECIAL;
            }

            else if (stompDelay >= 1.1f)
            {
                stompDelay = 0f;
                Context.SpecialAttackTimer = 0f;
                Context.BasicAttackTimer -= 0.5f;

                Context.Sounds.shockwaveSFX.Play(Context.transform.position, true);

                DeviceManager.Instance.ApplyVibrations(0.3f, 0.3f, 0.25f);
                Context.CameraUtilities.ShakeCamera(0.3f, 0.25f, EasingFunctions.EaseInQuint);

                Context.VFX.PlayVFX();
                Context.AttackCollide(Context.AttackColliders[(int)Attacks.SPECIAL]);
                currentAttack = Attacks.NONE;
                return;
            }

            stompDelay += Time.deltaTime;
        }
        
        else if (Context.IsBasicAttackAvailable && currentAttack != Attacks.SPECIAL)
        {
            if(!CoroutineBasicAttackStarted)
            {
                Context.StartCoroutine(BasicAttack());
            }
            else if (punchDelay >= 0.85f)
            {
                return;
            }
            else
            {
                Context.LookAtTarget(Context.Player.transform.position);
            }

            punchDelay += Time.deltaTime;
        }
    }

    // This method will be call on state changement.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GlorbStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    private IEnumerator BasicAttack()
    {
        CoroutineBasicAttackStarted = true;
        Context.Stats.SetValue(Stat.KNOCKBACK_DISTANCE, 0f);
        Context.Animator.ResetTrigger("Punch");
        Context.Animator.SetTrigger("Punch");
        currentAttack = Attacks.BASIC;

        yield return new WaitForSeconds(timeBeforePrepare);
        Context.StartCoroutine(Context.PrepareAttack());

        yield return new WaitForSeconds(0.1f);
        Context.Animator.speed = 2f;

        yield return new WaitForSeconds(timeBeforePunch - timeBeforePrepare - 0.1f);
        punchDelay = 0f;
        Context.BasicAttackTimer = 0f;
        Context.SpecialAttackTimer -= 0.5f;

        Context.Sounds.punchSFX.Play(Context.transform.position, true);

        Context.AttackCollide(Context.AttackColliders[(int)Attacks.BASIC]);
        currentAttack = Attacks.NONE;

        Context.Stats.SetValue(Stat.KNOCKBACK_DISTANCE, 5f);
        Context.Animator.speed = 1f;
        CoroutineBasicAttackStarted = false;
    }
}
