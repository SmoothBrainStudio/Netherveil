using PostProcessingEffects;
using UnityEngine;
using UnityEngine.VFX;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class EzrealAttack : Projectile
{
    [SerializeField] VisualEffect effect;
    float baseOffsetZ;

    protected override void Awake()
    {
        base.Awake();
        damage = (int)((Utilities.Hero.Stats.GetValueWithoutCoeff(Stat.ATK) + Utilities.PlayerController.FINISHER_DAMAGES) * Utilities.Hero.Stats.GetCoeff(Stat.ATK));
        AudioManager.Instance.PlaySound(AudioManager.Instance.EzrealUltSFX);
        baseOffsetZ = effect.transform.localPosition.z;
    }

    protected override void Update()
    {
        Move(transform.forward);
        effect.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - baseOffsetZ);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Map") && !other.isTrigger)
        {
            return;
        }

        IDamageable damageableObject = other.GetComponent<IDamageable>();
        if (damageableObject != null && other.gameObject.CompareTag("Enemy"))
        {
            damageableObject.ApplyDamage(damage, null);
        }
    }

    private void OnDestroy()
    {
        Destroy(transform.parent.gameObject);
    }
}
