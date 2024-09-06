using StateMachine; // include all scripts about StateMachines

public class ZiggoDeathState : BaseState<ZiggoStateMachine>
{
    public ZiggoDeathState(ZiggoStateMachine currentContext, StateFactory<ZiggoStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    // This method will be called every Update to check whether or not to switch states.

    protected override void CheckSwitchStates()
    {

    }

    protected override void EnterState()
    {
        
    }

    protected override void ExitState()
    {

    }

    protected override void UpdateState()
    {
        Context.Agent.isStopped = true;
    }

    protected override void SwitchState(BaseState<ZiggoStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.CurrentState = newState;
    }
}
