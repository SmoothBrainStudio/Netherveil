using StateMachine; // include all script about stateMachine
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PestAttackingState : BaseState<PestStateMachine>
{
    public PestAttackingState(PestStateMachine currentContext, StateFactory<PestStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    private enum State
    {
        Start,
        Charge,
        Dash,
        Recharge
    }

    private State curState = State.Start;
    private float elapsedTimeState = 0.0f;

    private float rechargeDuration = 0.5f;

    private float dashDistance = 0.0f;

    private Coroutine dashRoutine;

    bool attackEnded = false;

    Vector3 playerPos;

    // This method will be call every Update to check and change a state.
    protected override void CheckSwitchStates()
    {
        if (attackEnded)
        {
            if (!Context.Player)
            {
                SwitchState(Factory.GetState<PestWanderingState>());
            }
            else if (Vector3.Distance(Context.transform.position, Context.Player.transform.position) > Context.Stats.GetValue(Stat.ATK_RANGE))
            {
                SwitchState(Factory.GetState<PestTriggeredState>());
            }
        }
    }

    // This method will be call only one time before the update.
    protected override void EnterState()
    {
        curState = State.Start;
        elapsedTimeState = 0.0f;
        dashDistance = 0f;
        attackEnded = false;
        playerPos = Context.Player.position;
        dashRoutine = null;
    }

    // This method will be call only one time after the last update.
    protected override void ExitState()
    {
        if (dashRoutine != null)
        {
            Context.StopCoroutine(dashRoutine);
            dashRoutine = null;
        }

        Context.idleTimer = Context.MovementDelay / 2f;

        Context.Animator.ResetTrigger("Cancel");
        Context.Animator.SetTrigger("Cancel");
        Context.Animator.ResetTrigger("Cancel");

        Context.Animator.ResetTrigger(Context.ChargeInHash);
        Context.Animator.ResetTrigger(Context.ChargeOutHash);
    }

    // This method will be call every frame.
    protected override void UpdateState()
    {
        if (Context == null) return;

        if (Context.Player) playerPos = Context.Player.position;

        if (curState == State.Start)
        {
            attackEnded = false;
            Vector3 positionToLookAt = new Vector3(playerPos.x, Context.transform.position.y, playerPos.z);
            LookAt(positionToLookAt, 10f);

            elapsedTimeState = 0f;

            Context.Animator.ResetTrigger(Context.ChargeInHash);
            Context.Animator.SetTrigger(Context.ChargeInHash);

            curState = State.Charge;
        }
        else if (curState == State.Charge)
        {
            elapsedTimeState += Time.deltaTime;
            if (elapsedTimeState >= Context.AttackChargeDuration)
            {
                elapsedTimeState = 0.0f;

                dashDistance = Vector3.Distance(playerPos, Context.transform.position);
                dashDistance = Mathf.Clamp(dashDistance, 0f, Context.Stats.GetValue(Stat.ATK_RANGE));

                dashRoutine = Context.StartCoroutine(DashCoroutine(dashDistance, Context.DashSpeed));

                curState = State.Dash;
            }
            else if (elapsedTimeState <= Context.AttackChargeDuration - 0.2f)
            {
                Vector3 positionToLookAt = new Vector3(playerPos.x, Context.transform.position.y, playerPos.z);
                LookAt(positionToLookAt, 10f);
            }
        }
        else if (curState == State.Dash)
        {
            if (dashRoutine != null)
            {
                curState = State.Recharge;
            }
        }
        else if (curState == State.Recharge)
        {
            elapsedTimeState += Time.deltaTime;
            if (elapsedTimeState >= rechargeDuration)
            {
                elapsedTimeState = 0.0f;
                attackEnded = true;
                curState = State.Start;
            }
        }
    }

    private IEnumerator DashCoroutine(float distance, float speed)
    {
        if (distance <= 2f) { distance = 2f; }

        float timeElapsed = 0f;
        Vector3 startPosition = Context.transform.position;
        Vector3 dashTarget = Context.transform.position + Context.transform.forward * distance;

        float duration = distance / speed;
        bool isOnNavMesh = true;

        bool chargedOutAttack = false;

        while (timeElapsed < duration && isOnNavMesh)
        {
            yield return null;

            if (!Context.PlayerHit)
                AttackCollide(Context.AttackCollider, debugMode: false);

            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / duration);
            Vector3 warpPosition = Vector3.Lerp(startPosition, dashTarget, t);

            if (isOnNavMesh = NavMesh.SamplePosition(warpPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                if (!chargedOutAttack)
                {
                    chargedOutAttack = true;
                    Context.Animator.ResetTrigger(Context.ChargeOutHash);
                    Context.Animator.SetTrigger(Context.ChargeOutHash);
                }
                Context.Agent.Warp(hit.position);
            }
        }

        elapsedTimeState = 0f;
        dashRoutine = null;
        Context.PlayerHit = false;
    }

    // This method will be call on state changement.
    // No need to modify this method !
    protected override void SwitchState(BaseState<PestStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.CurrentState = newState;
    }

    #region Extra methods
    void LookAt(Vector3 _target, float _speed)
    {
        Quaternion lookRotation = Quaternion.LookRotation(_target - Context.transform.position);
        lookRotation.x = 0;
        lookRotation.z = 0;

        Context.transform.rotation = Quaternion.Slerp(Context.transform.rotation, lookRotation, _speed * Time.deltaTime);
    }

    public void AttackCollide(Collider _collider, bool debugMode = false)
    {
        if (debugMode)
        {
            _collider.gameObject.SetActive(false);
        }

        Collider[] tab = null;

        if (_collider is CapsuleCollider)
            tab = PhysicsExtensions.CapsuleOverlap(_collider as CapsuleCollider, LayerMask.GetMask("Entity"));
        else if (_collider is BoxCollider)
            tab = PhysicsExtensions.BoxOverlap(_collider as BoxCollider, LayerMask.GetMask("Entity"));
        else
            Debug.LogError("Type de collider non reconnu.");

        if (tab != null)
        {
            if (tab.Length > 0)
            {
                foreach (Collider col in tab)
                {
                    //if (col.gameObject.GetComponent<IDamageable>() != null && col.gameObject != Context.gameObject)
                    if (col.CompareTag("Player"))
                    {
                        Context.Attack(col.gameObject.GetComponent<IDamageable>());
                        Context.PlayerHit = true;
                    }
                }
            }
        }
    }
    #endregion
}
