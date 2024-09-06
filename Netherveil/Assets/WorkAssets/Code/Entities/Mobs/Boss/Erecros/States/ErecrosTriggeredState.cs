// ---[ STATE ] ---
// replace "FinalBossTriggeredState_STATEMACHINE" by your state machine class name.
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

public class ErecrosTriggeredState : BaseState<ErecrosStateMachine>
{
    public ErecrosTriggeredState(ErecrosStateMachine currentContext, StateFactory<ErecrosStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (Context.DebugMode)
        {
            UseDebugKeys();
        }
        else
        {
            if (Context.AttackCooldown <= 0f && Context.CurrentPart != 3)
            {
                if (Vector3.Distance(Context.Player.transform.position, Context.transform.position) <= Context.Stats.GetValue(Stat.ATK_RANGE))
                {
                    List<Type> availableAttacks = GetAvailableAttacks();

                    Context.LastAttack = availableAttacks[UnityEngine.Random.Range(0, availableAttacks.Count)];
                    SwitchState(Factory.GetState(Context.LastAttack));
                }
            }
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
        Context.AttackCooldown -= Time.deltaTime;

        Context.MoveTo(Context.Player.transform.position);

        if (Context.Agent.remainingDistance <= Context.Agent.stoppingDistance)
        {
            Context.Animator.SetBool("Walk", false);
            Context.Sounds.walk.Stop();
        }
        else
        {
            Context.Animator.SetBool("Walk", true);
            Context.Sounds.walk.Play(Context.transform.position);
        }

    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<ErecrosStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    List<Type> GetAvailableAttacks()
    {
        float distanceToPlayer = (Context.transform.position - Context.Player.transform.position).magnitude;

        List<Type> availableAttacks = new()
        {
            typeof(ErecrosTriangleDashAttack),
            typeof(ErecrosTeleportAttack)
        };

        if (distanceToPlayer >= 3f && UnityEngine.Random.Range(0, 10) < 3 && !Context.HasDoneSummoningPhase)
        {
            availableAttacks.Add(typeof(ErecrosSummoningAttack));
        }

        if (Context.CurrentPart == 1)
        {
            if (Context.CurrentPhase == 2)
            {
                if (distanceToPlayer > Context.PrisonVFX.GetFloat("Radius"))
                {
                    availableAttacks.Add(typeof(ErecrosPrisonAttack));
                }
            }
        }
        else if (Context.CurrentPart == 2)
        {
            availableAttacks.Add(typeof(ErecrosShockwaveAttack));

            if (Context.CurrentPhase == 2)
            {
                availableAttacks.Add(typeof(ErecrosWeaponThrowAttack));
                availableAttacks.Add(typeof(ErecrosWeaponThrowAttack));
                availableAttacks.Add(typeof(ErecrosWeaponThrowAttack));
            }
        }

        if (availableAttacks.Contains(Context.LastAttack))
        {
            availableAttacks.Remove(Context.LastAttack);
        }

        return availableAttacks;
    }

    #region Extra methods
    void UseDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchState(Factory.GetState<ErecrosTriangleDashAttack>());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchState(Factory.GetState<ErecrosSummoningAttack>());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchState(Factory.GetState<ErecrosTeleportAttack>());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchState(Factory.GetState<ErecrosPrisonAttack>());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SwitchState(Factory.GetState<ErecrosShockwaveAttack>());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SwitchState(Factory.GetState<ErecrosWeaponThrowAttack>());
        }
    }
    #endregion
}
