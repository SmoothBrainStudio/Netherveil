// ---[ STATE ] ---
// replace "GlorbTriggeredState_STATEMACHINE" by your state machine class name.
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
using UnityEngine;

public class GlorbTriggeredState : BaseState<GlorbStateMachine>
{
    public GlorbTriggeredState(GlorbStateMachine currentContext, StateFactory<GlorbStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
        if (Context.Player == null)
        {
            if (Context.Agent.remainingDistance <= Context.Agent.stoppingDistance)
            {
                SwitchState(Factory.GetState<GlorbWanderingState>());
            }
        }
        else if (Vector3.Distance(Context.transform.position, Context.Player.transform.position) <= Context.AttackRange)
        {
            SwitchState(Factory.GetState<GlorbAttackingState>());
        }
    }

    // This method will be call only one time before the update.
    protected override void EnterState()
    {

    }

    // This method will be call only one time after the last update.
    protected override void ExitState()
    {

    }

    // This method will be call every frame.
    protected override void UpdateState()
    {
        if (Context.Player != null)
            Context.MoveTo(Context.Player.transform.position);
    }

    // This method will be call on state changement.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GlorbStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
