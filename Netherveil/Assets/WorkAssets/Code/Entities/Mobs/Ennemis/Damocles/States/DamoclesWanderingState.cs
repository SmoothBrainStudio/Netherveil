using StateMachine; // include all script about stateMachine
using UnityEngine;

public class DamoclesWanderingState : BaseState<DamoclesStateMachine>
{
    public DamoclesWanderingState(DamoclesStateMachine currentContext, StateFactory<DamoclesStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    float idleTimer = 0f;

    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
        if (Context.Player != null)
        {
            SwitchState(Factory.GetState<DamoclesTriggeredState>());
        }
    }

    // This method will be call only one time before the update.
    protected override void EnterState()
    {
        Context.WanderZoneCenter = Context.transform.position;
        idleTimer = Random.Range(-0.5f, 0.5f);
        if (Context.LifeBar.gameObject.activeSelf) Context.LifeBar.FadeOutOpacity(0.5f, 0.25f);

        Context.IsInvincibleCount.Value = 1;
        Context.Animator.SetTrigger("BackToWalk");
    }

    // This method will be call only one time after the last update.
    protected override void ExitState()
    {
        if (Context.LifeBar.gameObject.activeSelf) Context.LifeBar.TriggerHealthBar();
    }

    // This method will be call every frame.
    protected override void UpdateState()
    {
        Context.IsInvincibleCount.Value = 1;
        if (Context.Agent.remainingDistance <= Context.Agent.stoppingDistance)
        {
            idleTimer += Time.deltaTime;
        }

        if (idleTimer >= 1f)
        {
            float minRange = Context.Stats.GetValue(Stat.ATK_RANGE) * 0.75f;
            float maxRange = Context.Stats.GetValue(Stat.ATK_RANGE) * 1.25f;

            Context.MoveTo(Context.GetRandomPointOnWanderZone(Context.transform.position, minRange, maxRange));
            idleTimer = Random.Range(-0.5f, 0.5f);
        }
    }

    // This method will be call on state changement.
    // No need to modify this method !
    protected override void SwitchState(BaseState<DamoclesStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
