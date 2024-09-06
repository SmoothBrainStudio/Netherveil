using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour, IReflectable
{
    // Start is called before the first frame update
    private Vector3 direction = Vector3.zero;

    public List<Status> statusToApply = new List<Status>();
    public List<Status> StatusToApply => statusToApply;

    public Vector3 Direction { get => direction; set => direction = value; }
    public bool IsReflected { get; set; }
    public bool CanBeReflected { get; set; } = false;

    public float FireballSpeed = 2f;


    float radius = 0f;

    public Mobs launcher = null;

    void Start()
    {
        radius = GetComponent<CapsuleCollider>().radius;
        Vector3 dir = direction;
        dir.y = 0;
        direction = dir;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Direction.normalized * Time.deltaTime * FireballSpeed;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, radius);
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (((1 << hit.collider.gameObject.layer) & LayerMask.GetMask("Map")) != 0)
                {
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsReflected)
        {
            Hero hero = other.GetComponentInParent<Hero>();
            if (hero)
            {
                Attack(hero);
                hero.AddStatus(new Fire(3f, 1f), launcher as IAttacker);
                Destroy(gameObject);
            }
        }
        else if (IsReflected)
        {
            Mobs mobs = other.GetComponentInParent<Mobs>();
            if (mobs)
            {
                Attack(mobs as IDamageable);
                mobs.AddStatus(new Fire(3f, 1f), launcher as IAttacker);
                Destroy(gameObject);
            }
        }
    }

    public void Attack(IDamageable damageable, int additionalDamages = 0)
    {
        damageable.ApplyDamage(10, launcher as IAttacker);
    }
}
