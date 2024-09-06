// ---[ STATE ] ---
// replace "GraftedTripleThrustAttack_STATEMACHINE" by your state machine class name.
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

public class GraftedTripleThrustAttack : BaseState<GraftedStateMachine>
{
    public GraftedTripleThrustAttack(GraftedStateMachine currentContext, StateFactory<GraftedStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    enum State
    {
        CHARGE,
        ATTACK,
        RECOVERY
    }

    bool attackEnded = false;

    State attackState;

    int thrustCounter = 0;
    float thrustChargeTimer = 0f;
    float thrustDurationTimer = 0f;
    readonly float THRUST_LENGTH = 8f;
    readonly float THRUST_SPEED = 23f;

    Coroutine tripleThrustCoroutine = null;

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
        Context.Animator.ResetTrigger("Thrust");
        Context.Animator.SetTrigger("Thrust");

        Context.Agent.isStopped = true;
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Cooldown = 2f + Random.Range(-0.25f, 0.25f);

        Context.PlayerHit = false;

        Context.Agent.isStopped = false;
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        switch (attackState)
        {
            case State.CHARGE:

                if (thrustChargeTimer < (thrustCounter == 0 ? Context.ThrustCharge : 0.5f))
                {
                    thrustChargeTimer += Time.deltaTime;
                }
                else
                {
                    thrustChargeTimer = 0;

                    Context.Sounds.thrustSound.Play(Context.transform.position, true);
                    Context.Sounds.thrustMapSound.Play(Context.transform.position, true);

                    DeviceManager.Instance.ApplyVibrations(0.8f, 0.8f, 0.25f);
                    Context.CameraUtilities.ShakeCamera(0.3f, 0.25f, EasingFunctions.EaseInQuint);

                    Context.FreezeRotation = true;

                    attackState = State.ATTACK;
                }
                break;

            case State.ATTACK:

                if (thrustDurationTimer < Context.ThrustDuration)
                {
                    if (thrustDurationTimer == 0f)
                    {
                        if (tripleThrustCoroutine == null)
                        {
                            tripleThrustCoroutine = Context.StartCoroutine(ThrustAttack());
                        }
                        else if (tripleThrustCoroutine != null)
                        {
                            Context.StopCoroutine(tripleThrustCoroutine);
                            tripleThrustCoroutine = Context.StartCoroutine(ThrustAttack());
                        }
                    }

                    thrustDurationTimer += Time.deltaTime;
                }
                else
                {
                    // DEBUG
                    Context.DisableHitboxes();
                    thrustDurationTimer = 0;
                    attackState = State.RECOVERY;
                }
                break;

            case State.RECOVERY:

                thrustCounter++;
                Context.FreezeRotation = false;
                Context.PlayerHit = false;

                if (thrustCounter < 3)
                {
                    attackState = State.CHARGE;
                }
                else
                {
                    thrustCounter = 0;
                    attackEnded = true;
                }
                break;
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GraftedStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    #region Extra Methods

    private IEnumerator ThrustAttack()
    {
        Transform thrustTransform = Context.AttackColliders[(int)GraftedStateMachine.Attacks.THRUST].data[0].transform;
        Transform vfxTransform = Context.TripleThrustVFX.transform;
        thrustTransform.position = new Vector3(Context.transform.position.x, 0f, Context.transform.position.z);
        vfxTransform.position = new Vector3(Context.transform.position.x, 0f, Context.transform.position.z);

        Vector3 endPos = thrustTransform.position + Context.transform.forward * THRUST_LENGTH;
        endPos.y = 0f;

        while (Vector3.Distance(thrustTransform.position, endPos) > 0.2f)
        {
            thrustTransform.position = Vector3.MoveTowards(thrustTransform.position, endPos, THRUST_SPEED * Time.deltaTime);
            vfxTransform.position = Vector3.MoveTowards(vfxTransform.position, endPos, THRUST_SPEED * Time.deltaTime);

            if (!Context.PlayerHit)
                Context.AttackCollide(Context.AttackColliders[(int)GraftedStateMachine.Attacks.THRUST].data, debugMode: false);

            yield return null;
        }

        thrustTransform.position = new Vector3(Context.transform.position.x , 0f, Context.transform.position.z);
        vfxTransform.position = new Vector3(Context.transform.position.x , 0f, Context.transform.position.z);
    }

    #endregion
}