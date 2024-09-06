using PostProcessingEffects;
using UnityEngine;

public class Bleeding : OverTimeStatus
{
    readonly float coefValue = 0.02f;
    static Color bleedingColor = new(0.5f, 0.11f, 0.11f, 1f);

    public Bleeding(float _duration, float _chance) : base(_duration, _chance)
    {
        isStackable = true;
        maxStack = 3;
        frequency = 1.0f;
        vfxName = "VFX_Bleeding";
    }
    public override Status DeepCopy()
    {
        Bleeding bleeding = (Bleeding)this.MemberwiseClone();
        bleeding.stopTimes = new();
        return bleeding;
    }

    public override void OnFinished()
    {
    }

    protected override void Effect()
    {
        if (target != null)
        {
            int damages = target.IsInvincibleCount == 0 ? (int)(target.Stats.GetMaxValue(Stat.HP) * coefValue * Stack) : 0;

            FloatingTextGenerator.CreateEffectDamageText(damages, target.transform.position, bleedingColor);
            target.gameObject.GetComponent<IDamageable>().ApplyDamage(damages, null, false);

            if (Utilities.IsPlayer(target))
                PostProcessingEffectManager.current.Play(PostProcessingEffects.Effect.Bleeding, false);
        }
    }

    public override bool CanApplyEffect(Entity target)
    {
        return target.gameObject.TryGetComponent<IDamageable>(out _);
    }

    protected override void PlayStatus()
    {
    }
}
