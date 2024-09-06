// ---[ STATE ] ---
// replace "ErecrosShockwave_STATEMACHINE" by your state machine class name.
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

public class ErecrosShockwaveAttack : BaseState<ErecrosStateMachine>
{
    public ErecrosShockwaveAttack(ErecrosStateMachine currentContext, StateFactory<ErecrosStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool attackEnded = false;
    float angle;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (attackEnded)
        {
            SwitchState(Factory.GetState<ErecrosTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        Context.Agent.isStopped = true;
        Context.ShockwaveVFX.gameObject.SetActive(true);

        angle = 0f;

        Vector3 vfxVector = Context.transform.forward * 4f;
        vfxVector = Quaternion.AngleAxis(angle, Vector3.up) * vfxVector;
        vfxVector.y = 0f;

        Context.ShockwaveVFX.transform.position = Context.transform.position + vfxVector;

        Context.Animator.ResetTrigger("Shockwave");
        Context.Animator.SetTrigger("Shockwave");

        Context.CurrentCouroutine = Context.StartCoroutine(ShockwaveCoroutine());
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Agent.isStopped = false;
        Context.ShockwaveVFX.gameObject.SetActive(false);

        Context.PlayerHit = false;

        Context.AttackCooldown = Random.Range(0, 10) < 3 ? 1.25f + Random.Range(-0.25f, 0.25f) : 0f;
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {

    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<ErecrosStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    IEnumerator ShockwaveCoroutine()
    {
        yield return new WaitForSeconds(1);

        DeviceManager.Instance.ApplyVibrations(0.8f, 0.8f, 0.25f);
        Context.CameraUtilities.ShakeCamera(0.3f, 1f, EasingFunctions.EaseInQuint);

        Context.Sounds.shockwave.Play(Context.transform.position);

        while (angle < 360)
        {
            angle += 600f * Time.deltaTime;

            Vector3 vfxVector = Context.transform.forward * 4f;
            vfxVector = Quaternion.AngleAxis(angle, Vector3.up) * vfxVector;
            vfxVector.y = 0f;

            Context.ShockwaveVFX.transform.position = Vector3.MoveTowards(Context.ShockwaveVFX.transform.position, Context.transform.position + vfxVector, 50f * Time.deltaTime);

            if (!Context.PlayerHit)
            {
                Context.AttackCollide(Context.Attacks[(int)ErecrosStateMachine.ErecrosColliders.SPINNING_ATTACK].data, debugMode: Context.DebugMode);
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.25f);

        angle = 0f;
        Context.DisableHitboxes();
        attackEnded = true;

        Context.CurrentCouroutine = null;
        yield return null;
    }
}
