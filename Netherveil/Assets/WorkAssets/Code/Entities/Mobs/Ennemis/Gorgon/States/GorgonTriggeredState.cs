// ---[ STATE ] ---
// replace "GorgonTriggeredState_STATEMACHINE" by your state machine class name.
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

public class GorgonTriggeredState : BaseState<GorgonStateMachine>
{

    const float coeffDeltaToDash = 1.15f;
    public GorgonTriggeredState(GorgonStateMachine currentContext, StateFactory<GorgonStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        float sqrGorgonBombDiameter = 0f;
        float thisToPlayerSqrMagnitude = 0f;
        float sqrAtkRange = 0f;
        if (Context.Player)
        {
            thisToPlayerSqrMagnitude = Vector3.SqrMagnitude(Context.Player.transform.position - Context.transform.position);
            sqrAtkRange = Context.Stats.GetValue(Stat.ATK_RANGE) * Context.Stats.GetValue(Stat.ATK_RANGE);
            if(Context.gameObject.GetComponentInChildren<ExplodingBomb>() == null)
            {
                SwitchState(Factory.GetState<GorgonWanderingState>());
                return;
            }
            sqrGorgonBombDiameter = Context.gameObject.GetComponentInChildren<ExplodingBomb>().BlastDiameter * Context.gameObject.GetComponentInChildren<ExplodingBomb>().BlastDiameter;
        }

        if (!Context.Player)
        {
            if (Context.Agent.remainingDistance <= Context.Agent.stoppingDistance)
            {
                SwitchState(Factory.GetState<GorgonWanderingState>());
                return;
            }
        }
        else if (thisToPlayerSqrMagnitude <= sqrAtkRange && thisToPlayerSqrMagnitude > sqrGorgonBombDiameter)
        {
            if (Context.IsAttackAvailable)
            {
                SwitchState(Factory.GetState<GorgonAttackingState>());
                return;
            }
            else if (Context.IsFleeAvailable && Vector3.Distance(Context.Player.transform.position, Context.transform.position) < Context.Stats.GetValue(Stat.ATK_RANGE) * coeffDeltaToDash)
            {
                SwitchState(Factory.GetState<GorgonFleeingState>());
                return;
            }
        }
        else if (Context.IsAttackAvailable && Context.IsDashAvailable)
        {
            SwitchState(Factory.GetState<GorgonDashingState>());
            return;
        }
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
        if (Context.Player && !Context.Agent.hasPath)
        {
            Vector3 direction = Context.transform.position - Context.Player.transform.position;
            Vector3 pointToReach = Context.transform.position.GetRandomPointOnCone(direction, 2, 45);
            if(Physics.Raycast(Context.transform.position, direction, out var hit, (pointToReach - Context.transform.position).magnitude))
            {
                pointToReach = Context.transform.position.GetRandomPointOnCone(hit.normal, 2, 45);
            }
            Context.MoveTo(pointToReach);
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GorgonStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
