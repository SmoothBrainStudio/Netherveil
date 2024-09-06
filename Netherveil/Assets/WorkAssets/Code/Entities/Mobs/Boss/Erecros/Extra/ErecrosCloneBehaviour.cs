using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class ErecrosCloneBehaviour : MonoBehaviour
{
    [SerializeField] VisualEffect VFXBomb;
    IAttacker attacker;

    CameraUtilities cameraUtilities;

    Coroutine explosionCoroutine = null;

    public Animator animator;

    [SerializeField] Collider attackHitbox;
    [SerializeField] Sound explosionSound;

    private void Awake()
    {
        cameraUtilities = Camera.main.GetComponent<CameraUtilities>();
    }

    public void Explode(IAttacker _attacker)
    {
        explosionCoroutine = StartCoroutine(ExplosionCoroutine());
    }

    void OnDestroy()
    {
        if (explosionCoroutine != null)
        {
            StopCoroutine(explosionCoroutine);
        }
    }

    IEnumerator ExplosionCoroutine()
    {
        VFXBomb.Play();

        Hero player = Utilities.Hero;
        float timer = 0f;
        float timeToExplode = VFXBomb.GetFloat("TimeToExplode");
        float explosionDuration = VFXBomb.GetFloat("ExplosionTime");

        Object.Destroy(transform.parent.gameObject, timeToExplode + explosionDuration);

        Vector3 direction = default;

        while (timer < timeToExplode)
        {
            timer += Time.deltaTime;

            if (timer >= 0.8)
            {
                transform.position += direction.normalized * 15f * Time.deltaTime;

                animator.ResetTrigger("Dash");
                animator.SetTrigger("Dash");
            }
            else
            {
                LookAtPlayer(player.transform);
                direction = player.transform.position - transform.position;
            }

            yield return null;
        }

        bool playerHit = false;

        GetComponentInChildren<Renderer>().gameObject.SetActive(false);

        explosionSound.Play(transform.position);

        DeviceManager.Instance.ApplyVibrations(0.8f, 0.8f, 0.5f);
        cameraUtilities.ShakeCamera(0.5f, 0.5f, EasingFunctions.EaseInQuint);

        Vector3 clonePos = transform.position;
        clonePos.y = player.transform.position.y;

        timer = 0f;
        do
        {
            timer += Time.deltaTime;

            if (Vector3.Distance(player.transform.position, clonePos) <= VFXBomb.GetFloat("ExplosionRadius") / 2f)
            {
                player.ApplyDamage(10, attacker);
                playerHit = true;
            }

            yield return null;
        } while (timer < explosionDuration && !playerHit);

        yield return null;
    }

    public void LookAtPlayer(Transform _player)
    {
        Vector3 mobToPlayer = _player.position - transform.position;
        mobToPlayer.y = 0f;

        Quaternion lookRotation = Quaternion.LookRotation(mobToPlayer);
        lookRotation.x = 0;
        lookRotation.z = 0;

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
    }

    public bool AttackCollide(IAttacker _attacker, bool debugMode = true)
    {
        if (debugMode)
        {
            attackHitbox.gameObject.SetActive(true);
        }

        Vector3 rayOffset = Vector3.up / 2;

        Collider[] tab = PhysicsExtensions.CheckAttackCollideRayCheck(attackHitbox, transform.position + rayOffset, "Player", LayerMask.GetMask("Map"));
        if (tab.Length > 0)
        {
            foreach (Collider col in tab)
            {
                if (col.gameObject.GetComponent<Hero>() != null)
                {
                    IDamageable damageable = col.gameObject.GetComponent<IDamageable>();
                    _attacker.Attack(damageable);

                    if (debugMode)
                    {
                        attackHitbox.gameObject.SetActive(false);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public void DisableDebugCollider()
    {
        attackHitbox.gameObject.SetActive(false);
    }
}
