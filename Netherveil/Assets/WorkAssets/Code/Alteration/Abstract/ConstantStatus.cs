public abstract class ConstantStatus : Status
{
    protected ConstantStatus(float _duration, float _chance) : base(_duration, _chance)
    {
        OnAddStack += DoEffect;
    }
    ~ConstantStatus()
    {
        OnAddStack -= DoEffect;
    }

    public sealed override void ApplyEffect(Entity target)
    {
        base.ApplyEffect(target);
    }

    public override void DoEffect()
    {
        Effect();
    }

    public sealed override void AddStack(int nb)
    {
        base.AddStack(nb);
        if ((isStackable && stack < maxStack) || stack < 1)
        {
            nb = isStackable ? nb : 1;
            stack += nb;
            for (int i = 0; i < nb; i++)
            {
                stopTimes.Add(duration + currentTime);
            }
            PlayVfx(vfxName);
        }
        
    }
}
