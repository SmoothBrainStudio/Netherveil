// ---[ STATE ] ---
// replace "SonielSpinningSwords_STATEMACHINE" by your state machine class name.
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
using UnityEngine;

public class SonielSpinningSwords : BaseState<SonielStateMachine>
{
    public SonielSpinningSwords(SonielStateMachine currentContext, StateFactory<SonielStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    // anim hash
    int throwLeftHash = Animator.StringToHash("ThrowLeft");
    int throwRightHash = Animator.StringToHash("ThrowRight");
    int getLeftHash = Animator.StringToHash("GetLeft");
    int getRightHash = Animator.StringToHash("GetRight");
    int throwToIdleHash = Animator.StringToHash("ThrowToIdle");

    float attackDuration = 0f;
    float timeToRetrieve = 0f;
    bool retrieved = false;

    enum Action
    {
        THROW_LEFT,
        THROW_RIGHT,
        RETRIEVE_LEFT,
        RETRIEVE_RIGHT,
        NONE
    }
    Action currentAction;

    float yPos;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (currentAction == Action.NONE)
        {
            SwitchState(Factory.GetState<SonielTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        Context.Agent.isStopped = true;
        yPos = Context.transform.position.y + 1f;
        timeToRetrieve = 0f;
        attackDuration = 0f;
        retrieved = false;

        for (int i = 0; i < 2; i++)
        {
            Context.Swords[i].SetParent(Context.Wrists[i], yPos);
        }

        currentAction = GetCurrentAction();

        Context.Animator.SetBool(throwToIdleHash, false);
        Context.Animator.SetBool(getLeftHash, false);
        Context.Animator.SetBool(getRightHash, false);

        if (currentAction == Action.THROW_LEFT || currentAction == Action.RETRIEVE_LEFT)
        {
            Context.Animator.ResetTrigger(throwLeftHash);
            Context.Animator.SetTrigger(throwLeftHash);
        }
        else if (currentAction != Action.NONE)
        {
            Context.Animator.ResetTrigger(throwRightHash);
            Context.Animator.SetTrigger(throwRightHash);
        }
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Agent.isStopped = false;
        Context.Animator.SetBool(getLeftHash, false);
        Context.Animator.SetBool(getRightHash, false);

        Context.Animator.SetBool(throwToIdleHash, true);

        Context.AttackCooldown = 1f + Random.Range(-0.5f, 0.5f);

        // DEBUG
        if (Context.DebugMode)
            Context.DisableHitboxes();
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        attackDuration += Time.deltaTime;

        switch (currentAction)
        {
            case Action.THROW_LEFT:
                Throw(0);
                break;
            case Action.THROW_RIGHT:
                Throw(1);
                break;
            case Action.RETRIEVE_LEFT:
                Retrieve(0);
                break;
            case Action.RETRIEVE_RIGHT:
                Retrieve(1);
                break;
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<SonielStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    #region Extra methods
    Action GetCurrentAction()
    {
        if (!Context.HasRightArm && Context.Swords[1].pickMeUp)
        {
            return Action.RETRIEVE_RIGHT;
        }
        else
        {
            if (Context.HasLeftArm)
            {
                Context.Animator.SetBool(throwToIdleHash, true);
                return Action.THROW_LEFT;
            }
            else if (Context.Swords[0].pickMeUp)
            {
                Context.Animator.SetBool(getLeftHash, true);
                return Action.RETRIEVE_LEFT;
            }
        }

        return Action.NONE;
    }

    void Throw(int _id)
    {
        Vector3 direction = Context.Player.transform.position - Context.transform.position;
        direction.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation.x = 0;
        lookRotation.z = 0;

        Context.transform.rotation = Quaternion.Slerp(Context.transform.rotation, lookRotation, 5f * Time.deltaTime);

        if (attackDuration >= Context.Animator.GetCurrentAnimatorClipInfo(0).Length - 0.6f)
        {
            if (_id == 0) Context.HasLeftArm = false;
            else Context.HasRightArm = false;

            Context.Sounds.launchSword.Play(Context.transform.position, true);

            Context.Swords[_id].enabled = true;
            Context.Swords[_id].SetLeft(_id == 0);
            Context.Swords[_id].transform.parent = null;

            Context.Swords[_id].SetDirection(direction);

            if (Context.PhaseTwo && _id == 0)
            {
                Context.Animator.ResetTrigger(throwRightHash);
                Context.Animator.SetTrigger(throwRightHash);

                attackDuration = 0f;
                currentAction = Action.THROW_RIGHT;
            }
            else
            {
                currentAction = Action.NONE;
            }
        }
    }

    void Retrieve(int _id)
    {
        SonielProjectile sword = Context.Swords[_id];

        if (sword.transform.parent == null)
        {
            sword.GetBack();

            timeToRetrieve += Time.deltaTime;
            if (timeToRetrieve >= 2f)
            {
                sword.ForceBack();
            }

            Quaternion lookRotation = Quaternion.LookRotation(sword.rotationPoint - Context.transform.position);
            lookRotation.x = 0;
            lookRotation.z = 0;

            Context.transform.rotation = Quaternion.Slerp(Context.transform.rotation, lookRotation, 5f * Time.deltaTime);
        }
        else if (!retrieved)
        {
            if (_id == 0)
            {
                Context.HasLeftArm = true;
                Context.Animator.SetBool(getLeftHash, true);
            }
            else
            {
                Context.HasRightArm = true;
                Context.Animator.SetBool(getRightHash, true);
            }

            Context.Sounds.retrieveSword.Play(Context.transform.position, true);

            retrieved = true;
            attackDuration = 0f;
        }
        else
        {
            if (attackDuration >= Context.Animator.GetCurrentAnimatorClipInfo(0).Length - 0.8f)
            {
                currentAction = Action.NONE;
            }
        }
    }

    #endregion

}
