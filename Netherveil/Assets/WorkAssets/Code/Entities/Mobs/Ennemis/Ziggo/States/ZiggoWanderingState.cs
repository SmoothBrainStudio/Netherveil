using StateMachine; // include all scripts about StateMachines
using UnityEngine;

public class ZiggoWanderingState : BaseState<ZiggoStateMachine>
{
    public ZiggoWanderingState(ZiggoStateMachine currentContext, StateFactory<ZiggoStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    float idleTimer = 0f;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (Context.Player != null)
        {
            SwitchState(Factory.GetState<ZiggoTriggeredState>());
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
            idleTimer += Time.deltaTime;
            Context.Sounds.moveSound.Stop();
        }
        else
        {
            Context.Sounds.moveSound.Play(Context.transform.position);
        }

        if (idleTimer >= 1f)
        {
            float minRange = Context.Stats.GetValue(Stat.VISION_RANGE) / 4f;
            float maxRange = Context.Stats.GetValue(Stat.VISION_RANGE) / 2f;

            Context.MoveTo(Context.GetRandomPointOnWanderZone(Context.transform.position, minRange, maxRange));
            idleTimer = Random.Range(-0.5f, 0.5f);
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<ZiggoStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
