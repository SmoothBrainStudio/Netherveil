// ---[ STATE ] ---
// replace "GorgonWanderingState_STATEMACHINE" by your state machine class name.
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

public class GorgonWanderingState : BaseState<GorgonStateMachine>
{
    public GorgonWanderingState(GorgonStateMachine currentContext, StateFactory<GorgonStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool canMove = false;
    float idleTimer = 0f;
    float MAX_IDLE_COOLDOWN = 2f;

    Vector3 randomPoint;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (Context.Player != null)
        {
            SwitchState(Factory.GetState<GorgonTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        Context.WanderZoneCenter = Context.transform.position;
        idleTimer = Random.Range(-0.5f, 0.5f);
        if (Context.LifeBar.gameObject.activeSelf) Context.LifeBar.FadeOutOpacity(0.5f, 0.25f);
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        if (Context.LifeBar.gameObject.activeSelf) Context.LifeBar.TriggerHealthBar();
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        if (Context.Agent.remainingDistance <= Context.Agent.stoppingDistance)
        {
            canMove = idleTimer >= MAX_IDLE_COOLDOWN;

            if (!canMove)
            {
                idleTimer += Time.deltaTime;
            }
            else
            {
                float minRange = Context.Stats.GetValue(Stat.ATK_RANGE) / 2f;
                float maxRange = Context.Stats.GetValue(Stat.ATK_RANGE);

                Context.MoveTo(Context.GetRandomPointOnWanderZone(Context.transform.position, minRange, maxRange));
                idleTimer = Random.Range(-0.5f, 0.5f);
            }
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
