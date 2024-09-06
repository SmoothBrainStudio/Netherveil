// ---[ STATE ] ---
// replace "FinalBossTeleportatAttack_STATEMACHINE" by your state machine class name.
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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ErecrosTeleportAttack : BaseState<ErecrosStateMachine>
{
    public ErecrosTeleportAttack(ErecrosStateMachine currentContext, StateFactory<ErecrosStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool attackEnded;
    List<Vector3> teleportPos = new();

    float teleportCooldown = 0f;
    float teleportAnimDelay = 0f;

    List<GameObject> clones = new();

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
        NavMeshHit hit;

        if (Context.CurrentPhase > 1 || Context.CurrentPart > 1)
        {
            Vector3 newPos = Context.Player.transform.position + (Context.Player.transform.position - Context.transform.position).normalized * 6f;
            newPos.y = Context.Player.transform.position.y;

            if (NavMesh.SamplePosition(newPos, out hit, 0.1f, NavMesh.AllAreas))
            {
                teleportPos.Add(newPos);
            }
        }

        Vector3 newPos2 = Context.transform.position + (Context.RoomCenter.position - Context.transform.position).normalized * Random.Range(8f, 12f);
        newPos2.y = Context.transform.position.y;

        teleportPos.Add(newPos2);
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Agent.isStopped = false;
        Context.AttackCooldown = 1.5f + Random.Range(-0.25f, 0.25f);

        foreach (GameObject clone in clones)
        {
            Context.Clones.Remove(clone);
            Object.Destroy(clone);
        }
        clones.Clear();
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        teleportCooldown -= Time.deltaTime;

        if (teleportCooldown <= 0)
        {
            Teleport();
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<ErecrosStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    #region Extra methods

    void Teleport()
    {
        if (teleportPos.Count <= 0)
        {
            attackEnded = true;
            return;
        }

        if (teleportAnimDelay == 0)
        {
            Context.Animator.ResetTrigger("Teleport");
            Context.Animator.SetTrigger("Teleport");

            Context.TeleportVFX.Play();
        }

        teleportAnimDelay += Time.deltaTime;

        if (teleportAnimDelay >= 0.9f)
        {
            teleportAnimDelay = 0;

            Context.Sounds.clone.Play(Context.transform.position, true);

            GameObject clone = Object.Instantiate(Context.ClonePrefab, Context.transform.position, Context.transform.rotation);
            clone.GetComponentInChildren<ErecrosCloneBehaviour>().Explode(Context);
            Context.transform.position = teleportPos[0];

            clones.Add(clone);
            Context.Clones.Add(clone);

            Vector3 bossToPlayer = Context.Player.transform.position - Context.transform.position;
            bossToPlayer.y = 0f;

            Context.transform.LookAt(Context.transform.position + bossToPlayer);

            teleportPos.RemoveAt(0);

            Context.Sounds.teleport.Play(Context.transform.position, true);

            teleportCooldown = teleportPos.Count > 0 ? 0.5f : 0;
        }
    }

    #endregion
}
