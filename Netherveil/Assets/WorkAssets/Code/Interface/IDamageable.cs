public interface IDamageable
{
    void ApplyDamage(int _value, IAttacker attacker, bool hasAnimation = true);
    void Death();
}
