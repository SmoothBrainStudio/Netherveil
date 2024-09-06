// ---[ STATE ] ---
// replace "GorgonFleeingState_STATEMACHINE" by your state machine class name.
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
using System.Collections.Generic;
using UnityEngine;

public class GorgonFleeingState : BaseState<GorgonStateMachine>
{
    public GorgonFleeingState(GorgonStateMachine currentContext, StateFactory<GorgonStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool dashLaunched = false;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (!Context.IsDashing && dashLaunched)
        {
            SwitchState(Factory.GetState<GorgonTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        Context.CanLoseAggro = false;
        Vector3 posToReach = Utilities.Player.transform.position.GetRandomPointInCircle(Context.Stats.GetValue(Stat.ATK_RANGE), Context.Stats.GetValue(Stat.ATK_RANGE) * 1.3f);
        float nbDash = (posToReach - Context.transform.position).magnitude / 5;
        if((int)nbDash == 0) nbDash = 1;
        List<Vector3> dash = Context.GetDashesPath(posToReach, (int)nbDash);

        Context.StartCoroutine(Context.DashToPos(dash));
        dashLaunched = true;
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.CanLoseAggro = true;
        dashLaunched = false;
        Context.FleeCooldown = 0f;
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GorgonStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
