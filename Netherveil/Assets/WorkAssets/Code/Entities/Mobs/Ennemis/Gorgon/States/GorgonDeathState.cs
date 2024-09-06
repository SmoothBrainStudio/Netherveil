using StateMachine; // include all scripts about StateMachines

public class GorgonDeathState : BaseState<GorgonStateMachine>
{
    public GorgonDeathState(GorgonStateMachine currentContext, StateFactory<GorgonStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }
        
    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        Context.Agent.isStopped = true;
    }
}
