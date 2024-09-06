// ---[ STATE ] ---
// replace "ZiggoCirclingState_STATEMACHINE" by your state machine class name.
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

public class ZiggoDashAttack : BaseState<ZiggoStateMachine>
{
    public ZiggoDashAttack(ZiggoStateMachine currentContext, StateFactory<ZiggoStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool attackEnded = false;
    Vector3 direction;
    float dashRange;
    bool dashed = false;

    Vector3 pointToGo;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (attackEnded)
        {
            SwitchState(Context.Player ? Factory.GetState<ZiggoTriggeredState>() : Factory.GetState<ZiggoWanderingState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        attackEnded = false;

        Vector3 mobPos = Context.transform.position;
        Vector3 playerPos = Context.Player.transform.position;

        pointToGo = playerPos - Context.Player.transform.forward * (playerPos - mobPos).magnitude;
        Context.MoveTo(pointToGo);
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.PlayerHit = false;

        Context.Stats.DecreaseCoeffValue(Stat.SPEED, 1.5f);
        Context.DashCooldown = 20f;

        attackEnded = false;

        // DEBUG
        Context.DisableHitboxes();
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        //// rotate
        //Quaternion lookRotation = Quaternion.LookRotation(pointToGo, Context.transform.position);
        //lookRotation.x = 0;
        //lookRotation.z = 0;
        //Context.transform.rotation = Quaternion.Slerp(Context.transform.rotation, lookRotation, 20f * Time.deltaTime);

        if (Context.Agent.remainingDistance <= Context.Agent.stoppingDistance)
        {
            if (!dashed)
            {
                dashed = true;

                direction = Utilities.Hero.transform.position - Context.transform.position;
                dashRange = Mathf.Min(direction.magnitude, Context.Stats.GetValue(Stat.ATK_RANGE)) + 1f;
                direction.y = 0;
                direction.Normalize();

                Context.Animator.ResetTrigger("Dash");
                Context.Animator.SetTrigger("Dash");

                pointToGo = Context.transform.position + direction * dashRange;
                Context.MoveTo(pointToGo);

                Context.Stats.IncreaseCoeffValue(Stat.SPEED, 1.5f);
            }
            else
            {
                attackEnded = true;
            }
        }

        if (dashed)
        {
            if (!Context.PlayerHit)
            {
                Context.AttackCollide(Context.AttackColliders[(int)ZiggoStateMachine.ZiggoAttacks.DASH], false);
                if (Context.PlayerHit)
                {
                    Context.Sounds.eatSound.Play(Context.transform.position, true);
                }
            }
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<ZiggoStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
