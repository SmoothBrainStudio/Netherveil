using System.Collections.Generic;
using UnityEngine;

public class GraftedProjectile : Projectile
{
    [HideInInspector]
    bool ignoreCollisions = false;
    Vector3 direction;
    GraftedStateMachine grafted;
    float tempSpeed = -1;
    public Sound explosionSound;

    [SerializeField] List<GameObject> projectileList = new List<GameObject>();

    public float Speed
    {
        get { return speed; }
    }

    public void SetCollisionImmune(bool _state) { ignoreCollisions = _state; }
    public bool GetCollisionImmune() { return ignoreCollisions; }

    public void SetTempSpeed(float _speed)
    {
        tempSpeed = _speed;
    }

    protected override void Awake()
    {
        damage = 5;
        GameObject go = Instantiate(projectileList[UnityEngine.Random.Range(0, projectileList.Count)], transform);
        go.GetComponent<Collider>().isTrigger = true;
    }

    public void Initialize(GraftedStateMachine _grafted)
    {
        grafted = _grafted;
    }

    public void SetDirection(Vector3 _direction)
    {
        direction = _direction;
    }

    protected override void Update()
    {
        float originalSpeed = speed;

        if (tempSpeed != -1)
        {
            speed = tempSpeed;
        }

        Move(direction);

        speed = originalSpeed;
        tempSpeed = -1;
    }

    public bool OnLauncher(Vector3 _launcher)
    {
        return Vector3.Distance(transform.position, _launcher) < 1f;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!ignoreCollisions)
        {
            if (((1 << other.gameObject.layer) & LayerMask.GetMask("Map")) != 0 && !other.isTrigger)
            {
                explosionSound.Play(transform.position);
                Destroy(gameObject);
                Destroy(Instantiate(GameResources.Get<GameObject>("VFX_Death"), transform.position, Quaternion.identity), 30f);
                return;
            }
        }

        IDamageable damageableObject = other.GetComponent<IDamageable>();
        if (damageableObject != null && other.CompareTag("Player"))
        {
            damageableObject.ApplyDamage(damage, grafted);
            Vector3 knockbackDirection = new Vector3(-direction.z, 0, direction.x);
            knockbackDirection.y = 0;
            knockbackDirection.Normalize();

            if (Vector3.Cross(transform.forward, other.transform.position - transform.position).y > 0)
            {
                knockbackDirection = -knockbackDirection;
            }

            grafted.ApplyKnockback(damageableObject, grafted, knockbackDirection);
            explosionSound.Play(transform.position);
            Destroy(gameObject);
            Destroy(Instantiate(GameResources.Get<GameObject>("VFX_Death"), transform.position, Quaternion.identity), 30f);
        }
    }
}