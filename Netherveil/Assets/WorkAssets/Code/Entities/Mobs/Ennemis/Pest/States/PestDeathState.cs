using StateMachine;
using UnityEngine;

public class PestDeathState : BaseState<PestStateMachine>
{
    public PestDeathState(PestStateMachine currentContext, StateFactory<PestStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }
        
    protected override void CheckSwitchStates()
    {

    }

    protected override void EnterState()
    {
        Context.Agent.isStopped = false;
    }

    protected override void ExitState()
    {

    }

    protected override void UpdateState()
    {
        Context.Agent.isStopped = true;
    }

    protected override void SwitchState(BaseState<PestStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.CurrentState = newState;
    }
}
