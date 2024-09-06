// ---[ STATE ] ---
// replace "FinalBossCircularDashAttack_STATEMACHINE" by your state machine class name.
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

public class ErecrosPrisonAttack : BaseState<ErecrosStateMachine>
{
    public ErecrosPrisonAttack(ErecrosStateMachine currentContext, StateFactory<ErecrosStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    bool attackEnded = false;

    Vector3 prisonCenter;
    float prisonRadius;
    float dashDistance;

    float delayBeforeDash = 0.3f;
    bool waiting = false;

    int randomClone = 0;

    List<Transform> clones = new();

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
        prisonCenter = Context.Player.transform.position;

        Context.PrisonVFX.transform.position = prisonCenter;
        Context.PrisonVFX.Play();

        prisonRadius = Context.PrisonVFX.GetFloat("Radius");

        Context.Agent.isStopped = true;

        int clonesAmount = 8;

        Context.Sounds.prison.Play(Context.transform.position);

        for (int i = 0; i < clonesAmount; i++)
        {
            Vector3 spawnVector = (Context.transform.position - prisonCenter).normalized * (prisonRadius + 2f);
            spawnVector = Quaternion.AngleAxis(360f / clonesAmount * i, Vector3.up) * spawnVector;
            spawnVector.y = 0f;

            GameObject clone = Object.Instantiate(Context.ClonePrefab, prisonCenter + spawnVector, Quaternion.identity);
            clone.SetActive(false);
            clone.transform.GetChild(0).GetChild(0).transform.position -= new Vector3(0, 2, 0);

            FacePlayer(clone.transform);
            clones.Add(clone.transform);

            Context.Clones.Add(clone);
        }

        Context.Vignette.active = true;

        Context.Animator.ResetTrigger("Prison");
        Context.Animator.SetTrigger("Prison");
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.Agent.isStopped = false;

        foreach (Transform clone in clones)
        {
            Context.Clones.Remove(clone.gameObject);
            Object.Destroy(clone.gameObject);
        }

        Context.Animator.ResetTrigger("PrisonEnded");
        Context.Animator.SetTrigger("PrisonEnded");

        Context.Vignette.active = false;

        //Context.PrisonVFX.Reinit();
        //Context.PrisonVFX.Stop();

        Context.AttackCooldown = 2f + Random.Range(-0.25f, 0.25f);
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {
        Vector2 sex = Camera.main.WorldToViewportPoint(prisonCenter);
        Context.Vignette.center.Override(sex + new Vector2(0, 0.05f));

        Vector3 prisonCenterToPlayer = Context.Player.transform.position - prisonCenter;
        if (prisonCenterToPlayer.sqrMagnitude > (prisonRadius - 0.5f) * (prisonRadius - 0.5f))
        {
            Context.Player.transform.position = prisonCenter + prisonCenterToPlayer.normalized * (prisonRadius - 0.5f);
        }

        if (clones.Count > 0)
        {
            ErecrosCloneBehaviour cloneBehaviour = clones[randomClone].GetComponentInChildren<ErecrosCloneBehaviour>();

            if (dashDistance == 0f)
            {
                clones[randomClone].gameObject.SetActive(true);
                FacePlayer(clones[randomClone].transform);
                Context.Sounds.dash.Play(clones[randomClone].transform.position);
            }

            if (delayBeforeDash > 0f)
            {
                FacePlayer(clones[randomClone].transform);
            }

            if (!waiting)
            {
                if (delayBeforeDash > 0f)
                {
                    clones[randomClone].position += (prisonCenter - clones[randomClone].transform.position).normalized * 20f * Time.deltaTime;
                }
                else
                {
                    clones[randomClone].position += clones[randomClone].forward * 20f * Time.deltaTime;
                }

                dashDistance += 20f * Time.deltaTime;
            }

            if (dashDistance >= 3f)
            {
                if (delayBeforeDash > 0f)
                {
                    delayBeforeDash -= Time.deltaTime;
                    waiting = true;
                    return;
                }

                cloneBehaviour.animator.ResetTrigger("Dash");
                cloneBehaviour.animator.SetTrigger("Dash");

                waiting = false;

                if (!Context.PlayerHit)
                {
                    if (cloneBehaviour.AttackCollide(Context, Context.DebugMode))
                    {
                        Context.PlayerHit = true;
                    }
                }

                if (dashDistance > prisonRadius * 2f + 2f)
                {
                    Context.Clones.Remove(clones[randomClone].gameObject);
                    Object.Destroy(clones[randomClone].gameObject);
                    clones.RemoveAt(randomClone);
                    dashDistance = 0f;

                    delayBeforeDash = 0.5f;

                    randomClone = Random.Range(0, clones.Count);
                    Context.PlayerHit = false;
                    return;
                }
            }
        }
        else
        {
            attackEnded = true;
        }
    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<ErecrosStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    void FacePlayer(Transform _sender)
    {
        Vector3 cloneToPlayer = Context.Player.transform.position - _sender.position;
        cloneToPlayer.y = 0f;

        Quaternion lookRotation = Quaternion.LookRotation(cloneToPlayer);
        lookRotation.x = 0;
        lookRotation.z = 0;

        _sender.transform.rotation = lookRotation;
    }
}
