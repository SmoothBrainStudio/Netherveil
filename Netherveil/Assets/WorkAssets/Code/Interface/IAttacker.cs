using System.Collections.Generic;

public interface IAttacker 
{
    public List<Status> StatusToApply { get; }
    public delegate void AttackDelegate();
    public AttackDelegate OnAttack
    {
        get;
        set;
    }

    public delegate void HitDelegate(IDamageable damageable, IAttacker attacker);
    public HitDelegate OnAttackHit
    {
        get;
        set;
    }

    public void Attack(IDamageable damageable, int additionalDamages = 0);

    public void ApplyStatus(IDamageable damageable, IAttacker attacker)
    {
        Entity entity = damageable as Entity;
        Mobs mobs = damageable as Mobs;
        if (entity == null || (mobs != null && mobs.IsSpawning) || entity.IsInvincibleCount > 0) return;
        foreach (var status in StatusToApply)
        {
            Status newStatus = status.DeepCopy();
            entity.AddStatus(newStatus);
        }
    }
}
