using System;
using UnityEngine;

public abstract class Projectile : MonoBehaviour, IProjectile
{
    public DamageState elementalDamage;
    [SerializeField] protected int damage = 5;
    [SerializeField] protected float speed = 30f;
    [SerializeField] protected float lifeTime = 5f;
    public static event Action<IDamageable, IAttacker> OnProjectileHit;

    public enum DamageState
    {
        NORMAL,
        FIRE,
        ICE,
        ELECTRICITY
    }

    protected virtual void Awake()
    {
        Destroy(gameObject, lifeTime);
    }

    public virtual void Move(Vector3 _direction)
    {
        _direction.Normalize();
        transform.Translate(_direction * speed * Time.deltaTime, Space.World);
        transform.rotation = Quaternion.LookRotation(_direction);
    }

    protected abstract void Update();

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Map") && !other.isTrigger)
        {
            Destroy(gameObject);
            return;
        }

        IDamageable damageableObject = other.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            OnProjectileHit?.Invoke(damageableObject, null);
            damageableObject.ApplyDamage(damage, null);

            if (elementalDamage != DamageState.NORMAL && TryGetComponent<Entity>(out var entity))
            {
                switch (elementalDamage)
                {
                    case DamageState.FIRE:
                        entity.AddStatus(new Fire(2, 1.0f));
                        break;
                    case DamageState.ICE:
                        entity.AddStatus(new Freeze(2, 1.0f));
                        break;
                    case DamageState.ELECTRICITY:
                        entity.AddStatus(new Electricity(2, 1.0f));
                        break;
                }
                // apply elementalState
            }

            Destroy(gameObject);
        }
    }
}
