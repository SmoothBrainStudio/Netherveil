using StateMachine;
using System.Collections;
using UnityEngine;

public class KlopsAttackState : BaseState<KlopsStateMachine>
{

    const float BeginToAttackTime = 0.2f;
    const float TimeBeforeChargingFireball = BeginToAttackTime + 0.15f;
    const float LaunchTime = 0.85f;

    readonly Vector3 fireballScale = new(0.6f, 0.6f, 0.6f);

    float timeBeforeChangeState = 1.5f;
    float currentTime = 0f;
    float scalingTimer = 0f;

    bool endState = false;
    bool hasShot = false;
    bool hasAnim = false;
    public KlopsAttackState(KlopsStateMachine currentContext, StateFactory<KlopsStateMachine> currentFactory) : base(currentContext, currentFactory)
    {
    }

    protected override void CheckSwitchStates()
    {
        if (endState)
        {
            SwitchState(Factory.GetState<KlopsWanderingState>());
        }
    }

    protected override void EnterState()
    {
        currentTime = 0f;

        hasShot = false;
        hasAnim = false;
        endState = false;

        Context.Agent.isStopped = true;

        timeBeforeChangeState = Random.Range(1.5f, 3f);
    }

    protected override void ExitState()
    {
        Context.Agent.isStopped = false;
    }

    protected override void UpdateState()
    {
        if (Context.IsFreeze) return;
        currentTime += Time.deltaTime;
        Context.transform.LookAt(Utilities.Player.transform);


        if (!hasAnim && currentTime >= BeginToAttackTime)
        {
            Context.Fireball = GameObject.Instantiate(Context.FireballPrefab, Context.FireballSpawn.position, Quaternion.identity);
            Context.Fireball.transform.localScale = Vector3.zero;
            Context.Animator.ResetTrigger("Attack");
            Context.Animator.SetTrigger("Attack");
            hasAnim = true;
        }
        else if (!hasShot && currentTime >= TimeBeforeChargingFireball && Context.Fireball != null)
        {
            scalingTimer += Time.deltaTime / (LaunchTime - TimeBeforeChargingFireball);
            Context.Fireball.transform.localScale = Vector3.Lerp(Vector3.zero, fireballScale, scalingTimer);
            Context.Fireball.transform.position = Context.FireballSpawn.position;
        }
        if (!hasShot && currentTime >= LaunchTime && Context.Fireball != null)
        {
            Context.KlopsSound.attackSound.Play(Context.transform.position);
            Context.Fireball.GetComponent<Fireball>().CanBeReflected = true;
            Context.Fireball.GetComponent<Fireball>().Direction = Utilities.Player.transform.position - Context.transform.position;
            Context.Fireball.GetComponent<Fireball>().launcher = Context;
            Context.Fireball.transform.LookAt(Utilities.Player.transform);
            GameObject.Destroy(Context.Fireball, 3f);
            hasShot = true;
        }
        if (currentTime >= timeBeforeChangeState)
        {
            endState = true;
        }
    }

    protected override void SwitchState(BaseState<KlopsStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

}
