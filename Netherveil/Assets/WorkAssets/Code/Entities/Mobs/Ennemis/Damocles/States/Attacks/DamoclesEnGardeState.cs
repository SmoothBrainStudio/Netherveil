using StateMachine; // include all script about stateMachine
using UnityEngine;

public class DamoclesEnGardeState : BaseState<DamoclesStateMachine>
{
    public DamoclesEnGardeState(DamoclesStateMachine currentContext, StateFactory<DamoclesStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    private bool stateEnded = false;
    private float guardTime = 1.5f;

    float travelledTime = 0f;
    int directionFactor = 1;
    float stateTimer;

    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
        if (!Context.Player)
        {
            SwitchState(Factory.GetState<DamoclesWanderingState>());
        }
        else if (stateEnded)
        {
            if (Vector3.Distance(Context.transform.position, Context.Player.transform.position) >= Context.Stats.GetValue(Stat.VISION_RANGE) * 2 / 3)
            {
                SwitchState(Factory.GetState<DamoclesTriggeredState>());
            }
            else
            {
                SwitchState(Factory.GetState<DamoclesJumpAttackState>());
            }
        }
    }

    // This method will be call only one time before the update.
    protected override void EnterState()
    {
        stateTimer = 0f;
        stateEnded = false;
        Context.Stats.IncreaseValue(Stat.SPEED, 2);
        Context.IsInvincibleCount.Value = 1;
        Context.Animator.SetTrigger("Guard");
    }

    // This method will be call only one time after the last update.
    protected override void ExitState()
    {
        Context.Stats.DecreaseValue(Stat.SPEED, 2);
    }

    // This method will be call every frame.
    protected override void UpdateState()
    {
        stateTimer += Time.deltaTime;

        Vector3 playerToMob = Context.transform.position - Context.Player.transform.position;
        playerToMob.y = 0f;

        Vector3 lookPosition = Context.Player.transform.position;
        lookPosition.y = Context.transform.position.y;
        Context.transform.LookAt(lookPosition);

        travelledTime += Time.deltaTime;
        if (travelledTime >= 1f)
        {
            travelledTime = 0f;
            directionFactor = -directionFactor;
        }

        if (Physics.Raycast(Context.transform.position + new Vector3(0, 1, 0), new Vector3(-playerToMob.normalized.z, 0, playerToMob.normalized.x), directionFactor, LayerMask.GetMask("Map")))
        {
            travelledTime = 0f;
            directionFactor = -directionFactor;
        }

        Context.MoveTo(Context.Player.transform.position + playerToMob + new Vector3(-playerToMob.normalized.z, 0, playerToMob.normalized.x) * directionFactor);

        stateEnded = stateTimer >= guardTime;
    }

    // This method will be call on state changement.
    // No need to modify this method !
    protected override void SwitchState(BaseState<DamoclesStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
