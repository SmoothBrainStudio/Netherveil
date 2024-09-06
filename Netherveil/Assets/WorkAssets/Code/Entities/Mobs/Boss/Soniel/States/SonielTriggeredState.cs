// ---[ STATE ] ---
// replace "SonielTriggeredState_STATEMACHINE" by your state machine class name.
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
using System;
using System.Collections.Generic;
using UnityEngine;

public class SonielTriggeredState : BaseState<SonielStateMachine>
{
    public SonielTriggeredState(SonielStateMachine currentContext, StateFactory<SonielStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    Type lastAttack;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (Context.AttackCooldown <= 0f)
        {
            List<Type> availableAttacks = GetAvailableAttacks();
            if (availableAttacks.Count <= 0)
            {
                Context.Swords[1].pickMeUp = true;
                availableAttacks = GetAvailableAttacks();
                if (availableAttacks.Count <= 0) throw new Exception("Non tu tbranles là chef");
            }

            lastAttack = availableAttacks[UnityEngine.Random.Range(0, availableAttacks.Count)];
            SwitchState(Factory.GetState(lastAttack));
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        Context.Animator.SetBool("Walk", true);
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Animator.SetBool("Walk", false);
        Context.Sounds.walk.Stop();
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        Context.MoveTo(Context.Player.transform.position - (Context.Player.transform.position - Context.transform.position).normalized * 2f);

        if (Context.Agent.remainingDistance > Context.Agent.stoppingDistance)
        {
            Context.Animator.SetBool("Walk", true);
            Context.Sounds.walk.Play(Context.transform.position, false);
        }
        else
        {
            Context.Animator.SetBool("Walk", false);
            Context.Sounds.walk.Stop();
        }


        Context.AttackCooldown -= Time.deltaTime;
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<SonielStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    #region Extra methods
    List<Type> GetAvailableAttacks()
    {
        List<Type> availableAttacks = new();

        if (Context.Swords[0].pickMeUp || Context.Swords[1].pickMeUp)
        {
            availableAttacks.Clear();
            availableAttacks.Add(typeof(SonielSpinningSwords));
            return availableAttacks;
        }

        float distanceToPlayer = Vector3.Distance(Context.transform.position, Context.Player.transform.position);

        if (Context.HasRightArm)
        {
            if (distanceToPlayer <= 10f)
            {
                availableAttacks.Add(typeof(SonielCircularHit));
            }

            if (Context.HasLeftArm && distanceToPlayer > Context.Agent.stoppingDistance + 1f)
            {
                availableAttacks.Add(typeof(SonielBerserk));
            }
        }
        if (distanceToPlayer >= 4f)
        {
            availableAttacks.Add(typeof(SonielSpinningSwords));
        }


        if (availableAttacks.Count <= 0 && Context.HasRightArm)
        {
            availableAttacks.Add(typeof(SonielCircularHit));
        }

        if (availableAttacks.Count > 1)
        {
            if (availableAttacks.Contains(lastAttack))
            {
                if (UnityEngine.Random.Range(0, 10) < 5) // 50% de chance d'enlever l'attaque qu'il a déjà faite de la liste des attaques dispo
                {
                    availableAttacks.Remove(lastAttack);
                }
            }
        }

        return availableAttacks;
    }

    #endregion
}
