// ---[ STATE ] ---
// replace "GorgonAttackingState_STATEMACHINE" by your state machine class name.
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
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GorgonAttackingState : BaseState<GorgonStateMachine>
{
    public GorgonAttackingState(GorgonStateMachine currentContext, StateFactory<GorgonStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool attackFinished;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (attackFinished)
        {
            SwitchState(Factory.GetState<GorgonTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        Context.Agent.isStopped = true;
        attackFinished = false;
        Context.StartCoroutine(LongRangeAttack());
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Agent.isStopped = false;
        Context.AttackCooldown = 0f;
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        if (Context.Player)
        {
            Quaternion lookRotation = Quaternion.LookRotation(Context.Player.transform.position - Context.transform.position);
            lookRotation.x = 0;
            lookRotation.z = 0;

            Context.transform.rotation = Quaternion.Slerp(Context.transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GorgonStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    #region Extra methods
    private IEnumerator LongRangeAttack()
    {
        if (!Context.Player)
            yield break;

        Context.Animator.SetTrigger("Attack");
        float timeToThrow = 0.7f;
        yield return new WaitWhile(() => Context.HasRemovedHead == false);
        Context.HasRemovedHead = false;
        Vector2 pointToReach2D = MathsExtension.GetRandomPointOnCircle(new Vector2(Context.Player.transform.position.x, Context.Player.transform.position.z), 1f);
        Vector3 pointToReach3D = new(pointToReach2D.x, Context.Player.transform.position.y, pointToReach2D.y);
        if (NavMesh.SamplePosition(pointToReach3D, out var hit, 3, -1))
        {
            pointToReach3D = hit.position;
        }

        if (Context.gameObject != null)
        {
            GameObject bomb = Context.gameObject.GetComponentInChildren<ExplodingBomb>().gameObject;
            yield return new WaitWhile(() => Context.HasLaunchAnim == false);
            Context.HasLaunchAnim = false;
            bomb.transform.rotation = Quaternion.identity;
            bomb.transform.parent = null;

            ExplodingBomb exploBomb = bomb.GetComponent<ExplodingBomb>();

            exploBomb.ThrowToPos(Context, pointToReach3D, timeToThrow);
            exploBomb.SetTimeToExplode(timeToThrow * 1.25f);
            exploBomb.SetBlastDamages((int)Context.Stats.GetValue(Stat.ATK));
            exploBomb.Activate();

            yield return new WaitForSeconds(0.5f);
            attackFinished = true;
        }
    }
    #endregion
}
