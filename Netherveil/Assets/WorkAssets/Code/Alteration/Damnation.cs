using PostProcessingEffects;

public class Damnation : ConstantStatus
{
    public Damnation(float _duration, float _chance) : base(_duration, _chance)
    {
        isStackable = false;
        vfxName = "VFX_DamnationDot";
    }

    public override bool CanApplyEffect(Entity target)
    {
        return target.TryGetComponent<Mobs>(out _);
    }

    public override Status DeepCopy()
    {
        Damnation damnation = (Damnation)this.MemberwiseClone();
        damnation.stopTimes = new();
        return damnation;
    }

    public override void OnFinished()
    {
        target.GetComponent<Mobs>().DamageTakenMultiplicator -= 1.0f;

        if (Utilities.IsPlayer(target))
            PostProcessingEffectManager.current.Stop(PostProcessingEffects.Effect.Damnation);
    }

    protected override void Effect()
    {
        target.GetComponent<Mobs>().DamageTakenMultiplicator += 1.0f;
    }

    protected override void PlayStatus()
    {
        PlayPostProcessing();
    }


    private void PlayPostProcessing()
    {
        if (Utilities.IsPlayer(target))
            PostProcessingEffectManager.current.Play(PostProcessingEffects.Effect.Damnation);
    }
}
