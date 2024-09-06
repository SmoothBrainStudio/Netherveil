// ---[ STATE ] ---
// replace "GraftedTriggeredState_STATEMACHINE" by your state machine class name.
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
using System;
using System.Collections.Generic;
using UnityEngine; // include all scripts about StateMachines

public class GraftedTriggeredState : BaseState<GraftedStateMachine>
{
    public GraftedTriggeredState(GraftedStateMachine currentContext, StateFactory<GraftedStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    Type lastAttack;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        //UseDebugKeys();

        if (Context.Cooldown <= 0f)
        {
            List<Type> availableAttacks = GetAvailableAttacks();

            lastAttack = availableAttacks[UnityEngine.Random.Range(0, availableAttacks.Count)];
            SwitchState(Factory.GetState(lastAttack));
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {

    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {

    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        Context.MoveTo(Context.Player.transform.position - (Context.Player.transform.position - Context.transform.position).normalized * 2f);
        Context.Sounds.walkingSound.Play(Context.transform.position);

        Context.Cooldown -= Time.deltaTime;

    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<GraftedStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    #region Extra methods

    List<Type> GetAvailableAttacks()
    {
        bool isNearPlayer = Vector3.Distance(Context.transform.position, Context.Player.transform.position) <= Context.Stats.GetValue(Stat.ATK_RANGE);

        List<Type> availableAttacks = new()
        {
            typeof(GraftedTripleThrustAttack),
            typeof(GraftedDashAttack),
            typeof(GraftedThrowProjectileAttack)
        };


        if (availableAttacks.Contains(typeof(GraftedThrowProjectileAttack)))
        {
            if (isNearPlayer)
            {
                if (UnityEngine.Random.Range(0, 10) < 5) availableAttacks.Remove(typeof(GraftedThrowProjectileAttack));
            }
        }

        if (availableAttacks.Contains(lastAttack))
        {
            if (UnityEngine.Random.Range(0, 10) < 5) availableAttacks.Remove(lastAttack);
        }

        return availableAttacks;
    }

    // DEBUG
    void UseDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchState(Factory.GetState<GraftedThrowProjectileAttack>());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchState(Factory.GetState<GraftedTripleThrustAttack>());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchState(Factory.GetState<GraftedDashAttack>());
        }
    }

    #endregion
}
