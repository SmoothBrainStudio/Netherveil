public class KnockbackPiston : ItemEffect , IPassiveItem 
{ 
    public void OnRetrieved() 
    {
        Utilities.Hero.OnBasicAttack += Utilities.Hero.ApplyKnockback;
        Utilities.Hero.OnDashAttack += Utilities.Hero.ApplyKnockback;
    } 
 
    public void OnRemove() 
    {
        Utilities.Hero.OnBasicAttack -= Utilities.Hero.ApplyKnockback;
        Utilities.Hero.OnDashAttack -= Utilities.Hero.ApplyKnockback;
    } 
 
} 
