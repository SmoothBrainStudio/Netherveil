//Copyright 2024 Property of Olivier Maurin.All rights reserved.
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
