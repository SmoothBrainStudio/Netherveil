using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class Knockback : MonoBehaviour
{
    private Hero hero;
    private Animator animator;
    private Coroutine knockbackRoutine;
    private bool isKnockback = false;
    public Vector3 startKnockback;
    public Vector3 endKnockback;

    [SerializeField] private float distanceFactor = 1f;

    public bool IsKnockback => isKnockback;
    public Vector3 StartKnockback => startKnockback;
    public Vector3 EndKnockback => endKnockback;

    /// <summary>
    /// int _value, bool isCrit = false, bool notEffectDamages = true
    /// </summary>
    public Action<int, IAttacker, bool> onObstacleCollide;
    [SerializeField] private int damageTakeOnObstacleCollide = 10;

    //[SerializeField, Range(0.001f, 0.1f)] private float StillThreshold = 0.05f; // Commenter par Dorian -> WARNING

    private void Start()
    {
        hero = GetComponent<Hero>();
        animator = GetComponentInChildren<Animator>();
    }

    public void GetKnockback(IAttacker attacker, Vector3 direction, float distance, float speed)
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        CharacterController characterController = GetComponent<CharacterController>();

        if (knockbackRoutine != null || !GetComponent<Entity>().IsKnockbackable)
            return;

        if (agent != null)
        {
            knockbackRoutine = StartCoroutine(ApplyKnockback(agent, attacker, direction, distance, speed));
        }
        else if (characterController != null && hero.State != (int)Entity.EntityState.DEAD)
        {
            animator.SetBool(Utilities.Player.GetComponent<PlayerController>().IsKnockbackHash, true);
            hero.State = (int)Hero.PlayerState.KNOCKBACK;
            knockbackRoutine = StartCoroutine(ApplyKnockback(characterController, attacker, direction, distance, speed));
        }
    }

    private IEnumerator ApplyKnockback(NavMeshAgent agent, IAttacker attacker, Vector3 direction, float distance, float speed)
    {
        float elapsed = 0f;
        bool canWarp = true;
        isKnockback = true;
        VFXStopper vfx = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_WallKB"), agent.transform.position, Quaternion.identity).GetComponent<VFXStopper>();
        vfx.Duration = 1f;
        vfx.PlayVFX();

        startKnockback = transform.position;
        endKnockback = transform.position + direction * distance * distanceFactor;

        float duration = distance * distanceFactor / speed;

        while (elapsed < duration && canWarp)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;
            Vector3 lerp = Vector3.Lerp(startKnockback, endKnockback, factor);

            canWarp = WarpPosition(agent, lerp, attacker);

            yield return null;
        }

        isKnockback = false;
        knockbackRoutine = null;
    }

    protected IEnumerator ApplyKnockback(CharacterController controller, IAttacker attacker, Vector3 direction, float distance, float speed)
    {
        if (controller == null)
        {
            knockbackRoutine = null;
            yield break;
        }


        Bounds bounds = controller.bounds;
        controller.enabled = false;

        float elapsed = 0f;
        startKnockback = transform.position;
        endKnockback = transform.position + direction * distance * distanceFactor;

        float duration = distance * distanceFactor / speed;
        bool hitObstacle = false;
        isKnockback = true;

        VFXStopper vfx = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_WallKB"), controller.transform.position, Quaternion.identity).GetComponent<VFXStopper>();
        vfx.Duration = 1f;
        vfx.PlayVFX();

        while (elapsed < duration && !hitObstacle)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;

            Vector3 lastPos = transform.position;
            Vector3 nextPos = Vector3.Lerp(startKnockback, endKnockback, factor);

            Collider[] collide = Physics.OverlapSphere(lastPos + Vector3.up, 0.1f, ~LayerMask.GetMask("Entity"), QueryTriggerInteraction.Ignore)
                                        .ToArray();

            hitObstacle = collide.Any();

            if (hitObstacle)
            {
                onObstacleCollide?.Invoke(damageTakeOnObstacleCollide, attacker, true);
                transform.position = nextPos - (bounds.size.x * direction);
            }
            else
            {
                transform.position = nextPos;
            }

            yield return null;
        }

        if (controller != null)
        {
            controller.enabled = true;
            hero.State = (int)Entity.EntityState.MOVE;
            animator.SetBool(Utilities.Player.GetComponent<PlayerController>().IsKnockbackHash, false);
        }
        knockbackRoutine = null;
        isKnockback = false;
    }

    private bool WarpPosition(NavMeshAgent agent, Vector3 position, IAttacker attacker)
    {
        bool canWarp = NavMesh.SamplePosition(position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas);
        if (canWarp)
            agent.Warp(hit.position);
        else
            onObstacleCollide?.Invoke(damageTakeOnObstacleCollide, attacker, true);

        return canWarp;
    }
}

