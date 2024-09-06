using StateMachine; // include all script about stateMachine
using System.Linq;
using UnityEngine;

public class PestTriggeredState : BaseState<PestStateMachine>
{
    public PestTriggeredState(PestStateMachine currentContext, StateFactory<PestStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    Vector3 lastPlayerPos;

    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
        if (Context.Player == null)
        {
            if ((lastPlayerPos - Context.transform.position).sqrMagnitude < 4f)
            {
                if (Context.NearbyEntities.FirstOrDefault(x => x is IPest))
                {
                    SwitchState(Factory.GetState<PestRegroupState>());
                }
                else
                {
                    SwitchState(Factory.GetState<PestWanderingState>());
                }
            }
        }
        else
        {
            if (Vector3.Distance(Context.transform.position, Context.Player.transform.position) <= Context.Stats.GetValue(Stat.ATK_RANGE))
            {
                SwitchState(Factory.GetState<PestAttackingState>());
            }
        }
    }

    // This method will be call only one time before the update.
    protected override void EnterState()
    { }

    // This method will be call only one time after the last update.
    protected override void ExitState()
    { }

    // This method will be call every frame.
    protected override void UpdateState()
    {
        if (Context.Player)
        {
            lastPlayerPos = Context.Player.position;
        }

        if (Context.CanMove)
        {
            Vector3 direction = (lastPlayerPos - Context.transform.position).normalized;
            Context.MoveTo(Context.transform.position + direction * Context.Stats.GetValue(Stat.ATK_RANGE));

            Context.idleTimer = 0f;
        }
    }

    // This method will be call on state changement.
    // No need to modify this method !
    protected override void SwitchState(BaseState<PestStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.CurrentState = newState;
    }
}
