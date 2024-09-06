using StateMachine; // include all script about stateMachine
using UnityEngine;

public class GlorbDeathState : BaseState<GlorbStateMachine>
{
    public GlorbDeathState(GlorbStateMachine currentContext, StateFactory<GlorbStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }
        
    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
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
        Context.Agent.isStopped = true;
    }

    // This method will be call on state changement.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GlorbStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
