using StateMachine; // include all scripts about StateMachines
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZiggoSpitAttack : BaseState<ZiggoStateMachine>
{
    public ZiggoSpitAttack(ZiggoStateMachine currentContext, StateFactory<ZiggoStateMachine> currentFactory)
        : base(currentContext, currentFactory) { }

    private bool attackEnded;
    private float puddleDuration = 1.5f;

    // This method will be called every Update to check whether or not to switch states.
    protected override void CheckSwitchStates()
    {
        if (attackEnded)
        {
            SwitchState(Factory.GetState<ZiggoTriggeredState>());
        }
    }

    // This method will be called only once before the update.
    protected override void EnterState()
    {
        attackEnded = false;
        Context.SpitAttackCoroutine = Context.StartCoroutine(SpitAttack());
        Context.Agent.isStopped = true;

        Context.Sounds.moveSound.Stop();
        Context.Sounds.spitSound.Play(Context.transform.position);
    }

    // This method will be called only once after the last update.
    protected override void ExitState()
    {
        Context.SpitCooldown = 2f;
        Context.Agent.isStopped = false;
    }

    // This method will be called every frame.
    protected override void UpdateState()
    {

    }

    // This method will be called on state switch.
    // No need to modify this method !
    protected override void SwitchState(BaseState<ZiggoStateMachine> newState)
    {
        base.SwitchState(newState);
        Context.currentState = newState;
    }

    private IEnumerator SpitAttack()
    {
        Context.Projectile.SetActive(true);

        ZiggoProjectile projectile = Context.Projectile.GetComponent<ZiggoProjectile>();
        Transform originalParent = projectile.transform.parent;

        Context.Animator.ResetTrigger("Spit");
        Context.Animator.SetTrigger("Spit");

        float timeToThrow = 1f;
        float maxHeight = 2f;

        Vector2 pointToReach2D = MathsExtension.GetRandomPointOnCircle(new Vector2(Context.Player.transform.position.x, Context.Player.transform.position.z), 1f);
        Vector3 pointToReach3D = new(pointToReach2D.x, Context.Player.transform.position.y, pointToReach2D.y);

        if (NavMesh.SamplePosition(pointToReach3D, out var hit, 3, -1))
        {
            pointToReach3D = hit.position;
        }

        projectile.transform.rotation = Quaternion.identity;
        projectile.transform.parent = null;

        Vector3 throwPos = Context.transform.position + (pointToReach3D - Context.transform.position).normalized * Mathf.Min((pointToReach3D - Context.transform.position).magnitude, Context.Stats.GetValue(Stat.ATK_RANGE));
        projectile.ThrowToPos(throwPos, timeToThrow, maxHeight);

        yield return new WaitForSeconds(timeToThrow);

        projectile.PoisonBallVFX.gameObject.SetActive(false);
        projectile.PoisonPuddleVFX.transform.parent = null;
        projectile.PoisonPuddleVFX.transform.position = new Vector3(projectile.transform.position.x, Utilities.Player.transform.position.y, projectile.transform.position.z);
        projectile.PoisonPuddleVFX.transform.localScale = Vector3.one;
        projectile.PoisonPuddleVFX.transform.rotation = Quaternion.identity;

        projectile.PoisonPuddleVFX.SetFloat("Duration", puddleDuration);
        //this is because vfx is based on plane size so 1 plane size equals 5 unity units
        float planeLength = 5f;
        projectile.PoisonPuddleVFX.SetFloat("Diameter", projectile.FlaqueRadius * 2f / planeLength);
        projectile.PoisonPuddleVFX.Play();

        // Splatter
        float maxDiameter = 20f;
        float maxThickness = 0.2f;
        float coeff = 20 / (1 - maxThickness);
        float speed = 3f;

        projectile.ApplyPoison();
        Context.Sounds.splatterSound.Play(projectile.transform.position);

        do
        {
            yield return null;
            Vector3 scale = projectile.transform.localScale;
            scale.x = scale.x >= maxDiameter ? maxDiameter : scale.x + Time.deltaTime * coeff * speed;
            scale.z = scale.z >= maxDiameter ? maxDiameter : scale.z + Time.deltaTime * coeff * speed;
            scale.y = scale.y <= maxThickness ? maxThickness : scale.y - Time.deltaTime * speed;

            projectile.transform.localScale = scale;

        } while (projectile.transform.localScale.x != maxDiameter || projectile.transform.localScale.y != maxThickness);

        attackEnded = true;
        yield return new WaitForSeconds(puddleDuration);

        projectile.PoisonPuddleVFX.Stop();
        projectile.PoisonPuddleVFX.transform.parent = projectile.transform;
        projectile.PoisonBallVFX.gameObject.SetActive(true);

        //if (Context && Context.Stats.GetValue(Stat.HP) > 0)
        //{
        projectile.transform.parent = originalParent;
        projectile.transform.localPosition = Vector3.zero;
        projectile.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        projectile.gameObject.SetActive(false);
        //}
        //else
        //{
        //    Object.Destroy(projectile.gameObject);
        //}
    }
}

