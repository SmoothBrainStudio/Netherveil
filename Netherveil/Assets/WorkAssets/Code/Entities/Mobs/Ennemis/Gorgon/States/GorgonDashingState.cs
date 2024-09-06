using StateMachine; // include all scripts about StateMachines
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GorgonDashingState : BaseState<GorgonStateMachine>
{
    public GorgonDashingState(GorgonStateMachine currentContext, StateFactory<GorgonStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool dashLaunched = false;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (!Context.IsDashing && dashLaunched)
        {
            bool isPlayerInRange = Vector3.SqrMagnitude(Context.Player.transform.position - Context.transform.position) < Context.Stats.GetValue(Stat.ATK_RANGE) * Context.Stats.GetValue(Stat.ATK_RANGE);

            if (isPlayerInRange) SwitchState(Factory.GetState<GorgonAttackingState>());
            else SwitchState(Factory.GetState<GorgonTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        dashLaunched = false;
        Context.CanLoseAggro = false;

        // Take a random point around the player pos in 2D then convert it in 3D
        Vector2 pointToReach2D = MathsExtension.GetRandomPointOnCircle(new Vector2(Context.Player.transform.position.x, Context.Player.transform.position.z), Context.Stats.GetValue(Stat.ATK_RANGE) * 0.9f);
        Vector3 pointToReach3D = new(pointToReach2D.x, Context.transform.position.y, pointToReach2D.y);

        // Replace the point on navMesh
        NavMesh.SamplePosition(pointToReach3D, out NavMeshHit hit, float.PositiveInfinity, NavMesh.AllAreas);
        pointToReach3D = hit.position;

        float nbDash = (pointToReach3D - Context.transform.position).magnitude / 5;
        List<Vector3> listDashes = Context.GetDashesPath(pointToReach3D, (int)nbDash + 1);
        Context.DashCoroutine = Context.StartCoroutine(Context.DashToPos(listDashes));

        dashLaunched = true;
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        dashLaunched = false;
        Context.CanLoseAggro = true;
        Context.DashCooldown = 0f;
        Context.StopCoroutine(Context.DashCoroutine);
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
