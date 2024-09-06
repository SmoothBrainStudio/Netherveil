using StateMachine; // include all script about stateMachine
using System.Linq;
using UnityEngine;

public class PestWanderingState : BaseState<PestStateMachine>
{
    public PestWanderingState(PestStateMachine currentContext, StateFactory<PestStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    private Vector3 randomDirection;

    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
        if (Context.Player != null)
        {
            SwitchState(Factory.GetState<PestTriggeredState>());
        }
        else if (Context.NearbyEntities.FirstOrDefault(x => x is IPest))
        {
            SwitchState(Factory.GetState<PestRegroupState>());
        }
    }

    // This method will be call only one time before the update.
    protected override void EnterState()
    {
        Context.WanderZoneCenter = Context.transform.position;
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
        if (Context.CanMove)
        {
            float maxRange = Context.Stats.GetValue(Stat.ATK_RANGE);
            float minRange = maxRange * 0.5f;

            Context.MoveTo(Context.GetRandomPointOnWanderZone(Context.transform.position, minRange, maxRange));
            Context.idleTimer = Random.Range(-0.5f, 0.5f);
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
