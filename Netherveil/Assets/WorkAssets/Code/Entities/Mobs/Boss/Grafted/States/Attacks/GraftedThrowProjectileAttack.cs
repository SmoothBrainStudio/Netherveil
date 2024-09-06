// ---[ STATE ] ---
// replace "GraftedThrowProjectileAttack_STATEMACHINE" by your state machine class name.
//
// Here you can see an exemple of the CheckSwitchStates method:
// protected override void CheckSwitchStates()
// {
//      if (isRunning)
//      {
//          SwitchState(Factory.GetState<RunningState>());
//      }
// }

using StateMachine;
using UnityEngine; // include all scripts about StateMachines

public class GraftedThrowProjectileAttack : BaseState<GraftedStateMachine>
{
    public GraftedThrowProjectileAttack(GraftedStateMachine currentContext, StateFactory<GraftedStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool attackEnded = false;
    float throwingTimer = 0f;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (attackEnded)
        {
            SwitchState(Factory.GetState<GraftedTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        Context.Agent.isStopped = true;
        Context.FreezeRotation = true;

        attackEnded = false;
        throwingTimer = 0.7f;

        Context.Animator.ResetTrigger("Throw");
        Context.Animator.SetTrigger("Throw");
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Agent.isStopped = false;
        Context.FreezeRotation = false;

        Context.Cooldown = 2f + Random.Range(-0.25f, 0.25f);
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        throwingTimer -= Time.deltaTime;

        if (throwingTimer <= 0)
        {
            Context.Projectile = Object.Instantiate(Context.ProjectilePrefab, Context.transform.position + new Vector3(0, Context.Height / 6f, 0), Quaternion.identity).GetComponent<GraftedProjectile>();
            Context.Projectile.Initialize(Context);
            Vector3 direction = Context.Player.transform.position - Context.transform.position;
            direction.y = 0;
            Context.Projectile.SetDirection(direction);

            Context.Sounds.projectileLaunchedSound.Play(Context.transform.position);

            attackEnded = true;
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GraftedStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }
}
