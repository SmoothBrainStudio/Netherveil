using StateMachine; // include all script about stateMachine
using UnityEngine;

public class DamoclesTriggeredState : BaseState<DamoclesStateMachine>
{
    public DamoclesTriggeredState(DamoclesStateMachine currentContext, StateFactory<DamoclesStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
        if (Context.Player == null)
        {
            if (Context.Agent.remainingDistance <= Context.Agent.stoppingDistance)
            {
                SwitchState(Factory.GetState<DamoclesWanderingState>());
                return;
            }
        }
        else
        {
            if (Vector3.Distance(Context.transform.position, Context.Player.transform.position) <= Context.Stats.GetValue(Stat.ATK_RANGE))
            {
                SwitchState(Factory.GetState<DamoclesEnGardeState>());
            }
        }
    }

    // This method will be call only one time before the update.
    protected override void EnterState()
    {
        Context.Animator.SetTrigger("BackToWalk");
        Context.IsInvincibleCount.Value = 1;
    }

    // This method will be call only one time after the last update.
    protected override void ExitState()
    {
    }

    // This method will be call every frame.
    protected override void UpdateState()
    {
        if (Context.Player)
            Context.MoveTo(Context.Player.transform.position);
    }

    // This method will be call on state changement.
    // No need to modify this method !
    protected override void SwitchState(BaseState<DamoclesStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
