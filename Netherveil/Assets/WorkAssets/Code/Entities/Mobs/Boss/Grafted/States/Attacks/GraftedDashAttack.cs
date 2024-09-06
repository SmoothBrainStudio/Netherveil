// ---[ STATE ] ---
// replace "GrafedDashAttack_STATEMACHINE" by your state machine class name.
//
// Here you can see an exemple of the CheckSwitchStates method:
// protected override void CheckSwitchStates()
// {
//      if (isRunning)
//      {
//          SwitchState(Factory.GetState<RunningState>());
//      }
// }

using StateMachine;
using UnityEngine; // include all scripts about StateMachines

public class GraftedDashAttack : BaseState<GraftedStateMachine>
{
    public GraftedDashAttack(GraftedStateMachine currentContext, StateFactory<GraftedStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool attackEnded = false;

    float dashChargeTimer = 0f;
    float travelledDistance = 0f;
    float AOETimer = 0f;
    bool triggerAOE = false;

    float fallTimer = 0f;
    bool fell = false;

    bool charging = true;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (attackEnded)
        {
            SwitchState(Factory.GetState<GraftedTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        Context.Animator.ResetTrigger("Dash");
        Context.Animator.SetTrigger("Dash");
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Cooldown = 1.5f + Random.Range(-0.25f, 0.25f);

        Context.PlayerHit = false;

        Context.Animator.ResetTrigger("FallEnded");
        Context.Animator.SetTrigger("FallEnded");

        Context.FreezeRotation = false;

        // DEBUG
        Context.DisableHitboxes();
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        if (!fell)
        {
            fallTimer += Time.deltaTime;

            if (fallTimer >= 0.5f)
            {
                Context.Sounds.dashSound.Play(Context.transform.position);
                Context.DashVFX.GetComponent<VFXStopper>().PlayVFX();

                DeviceManager.Instance.ApplyVibrations(0.8f, 0.8f, 0.3f);
                Context.CameraUtilities.ShakeCamera(0.5f, 0.3f, EasingFunctions.EaseInQuint);
                fell = true;
            }
        }

        if (charging)
        {
            dashChargeTimer += Time.deltaTime;

            if (dashChargeTimer >= 0.3f)
            {
                Context.Animator.ResetTrigger("Fall");
                Context.Animator.SetTrigger("Fall");

                Context.FreezeRotation = true;
                charging = false;
            }
        }
        else
        {
            travelledDistance += Time.deltaTime * Context.DashSpeed;

            if (!triggerAOE && !Context.PlayerHit)
            {
                Context.AttackCollide(Context.AttackColliders[(int)GraftedStateMachine.Attacks.DASH].data, debugMode: false);
            }

            if (travelledDistance <= Context.DashRange)
            {
                Context.Agent.Warp(Context.transform.position + Context.transform.forward * Time.deltaTime * Context.DashSpeed);
            }
            else if (!triggerAOE)
            {
                // DEBUG
                Context.DisableHitboxes();
                Context.Sounds.fallSound.Play(Context.transform.position);

                Context.PlayerHit = false;
                triggerAOE = true;
            }
            else
            {
                if (!Context.PlayerHit)
                {
                    Context.AttackCollide(Context.AttackColliders[(int)GraftedStateMachine.Attacks.DASH + 1].data, debugMode: false);
                }

                AOETimer += Time.deltaTime;
                Context.Agent.Warp(Context.transform.position + Context.transform.forward * Time.deltaTime * Context.DashSpeed * 0.15f);
                if (AOETimer >= Context.AOEDuration)
                {
                    attackEnded = true;
                }
            }
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GraftedStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
