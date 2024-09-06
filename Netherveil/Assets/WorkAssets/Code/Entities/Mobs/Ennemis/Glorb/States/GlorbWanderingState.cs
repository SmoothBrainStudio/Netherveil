// ---[ STATE ] ---
// replace "GlorbWanderingState_STATEMACHINE" by your state machine class name.
//
// Here you can see an exemple of CheckSwitchStates method:
// protected override void CheckSwitchStates()
// {
//      if (isRunning)
//      {
//          SwitchState(Factory.GetState<RunningState>());
//      }
// }

using StateMachine; // include all script about stateMachine
using Unity.VisualScripting;
using UnityEngine;

public class GlorbWanderingState : BaseState<GlorbStateMachine>
{
    public GlorbWanderingState(GlorbStateMachine currentContext, StateFactory<GlorbStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    Vector3 randomDirection;
    float idleTimer = 0f;

    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
        if (Context.Player != null)
        {
            SwitchState(Factory.GetState<GlorbTriggeredState>());
        }
    }

    // This method will be call only one time before the update.
    protected override void EnterState()
    {
        Context.WanderZoneCenter = Context.transform.position;
        idleTimer = Random.Range(-0.5f, 0.5f);

        if (Context.LifeBar.gameObject.activeSelf) Context.LifeBar.FadeOutOpacity(0.5f, 0.25f);
    }

    // This method will be call only one time after the last update.
    protected override void ExitState()
    {
        if (Context.LifeBar.gameObject.activeSelf) Context.LifeBar.TriggerHealthBar();
    }

    // This method will be call every frame.
    protected override void UpdateState()
    {

        if (Context.Agent.remainingDistance <= Context.Agent.stoppingDistance)
        {
            idleTimer += Time.deltaTime;
        }

        if (idleTimer > 1f)
        {
            float minRange = Context.Stats.GetValue(Stat.VISION_RANGE) / 4f;
            float maxRange = Context.Stats.GetValue(Stat.VISION_RANGE) / 2f;

            Context.MoveTo(Context.GetRandomPointOnWanderZone(Context.transform.position, minRange, maxRange));
            idleTimer = Random.Range(-0.5f, 0.5f);
        }
    }

    // This method will be call on state changement.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GlorbStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
