using StateMachine; // include all scripts about StateMachines
using System.Collections;
using UnityEngine;

public class DamoclesVulnerableState : BaseState<DamoclesStateMachine>
{
    public DamoclesVulnerableState(DamoclesStateMachine currentContext, StateFactory<DamoclesStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    private bool stateEnded = false;
    private float elapsedTimeMovement = 0.0f;
    private float vulnerableTime = 2.5f;
    private Vector3 basePos;
    private Vector3 wantedPos;
    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (stateEnded)
        {
            if (Context.Player)
                SwitchState(Factory.GetState<DamoclesTriggeredState>());
            else
                SwitchState(Factory.GetState<DamoclesWanderingState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        stateEnded = false;
        elapsedTimeMovement = Time.time;
        Context.IsInvincibleCount.Value = 0;
        basePos = Context.transform.position;
        wantedPos = new Vector3(basePos.x, Utilities.Hero.transform.position.y, basePos.z);
        Context.StartCoroutine(TryToUnlock());
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Agent.enabled = true;
        Context.Player = Utilities.Hero;

        Context.Stats.SetValue(Stat.SPEED, 2);
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        // Delay
        if (Time.time - elapsedTimeMovement < vulnerableTime)
            return;

        elapsedTimeMovement = Time.time;

        stateEnded = true;
        Context.DamoclesSound.destuckSound.Play(Context.transform.position);
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<DamoclesStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    IEnumerator TryToUnlock()
    {
        float timer = 0f;
        while (timer < 1.0f)
        {
            if (Context == null) yield break;
            timer += Time.deltaTime / vulnerableTime;
            Context.transform.position = Vector3.Lerp(basePos, wantedPos, timer);
            yield return null;
        }
        Context.Animator.SetTrigger("BackToWalk");
        yield break;
    }
}
