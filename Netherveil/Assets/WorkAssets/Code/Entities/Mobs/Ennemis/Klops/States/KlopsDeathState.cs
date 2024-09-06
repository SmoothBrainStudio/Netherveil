using StateMachine;

public class KlopsDeathState : BaseState<KlopsStateMachine>
{
    public KlopsDeathState(KlopsStateMachine currentContext, StateFactory<KlopsStateMachine> currentFactory) : base(currentContext, currentFactory)
    {
    }

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
}
