using StateMachine; // include all script about stateMachine

public class DamoclesDeathState : BaseState<DamoclesStateMachine>
{
    public DamoclesDeathState(DamoclesStateMachine currentContext, StateFactory<DamoclesStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

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

    }

    protected override void SwitchState(BaseState<DamoclesStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.CurrentState = newState;
    }
}
