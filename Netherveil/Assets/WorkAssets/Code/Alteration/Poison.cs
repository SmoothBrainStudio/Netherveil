using PostProcessingEffects;
using UnityEngine;

public class Poison : OverTimeStatus
{
    private static readonly int baseStack = 3;
    static Color poisonColor = new Color(0.047f, 0.58f, 0.047f);
    public Poison(float _duration, float _chance) : base(_duration, _chance)
    {
        isStackable = true;
        frequency = _duration - 0.001f;
        vfxName = "VFX_Poison";
    }

    public override bool CanApplyEffect(Entity target)
    {
        return target.TryGetComponent<IDamageable>(out _);
    }

    public override Status DeepCopy()
    {
        Poison poison = (Poison)this.MemberwiseClone();
        poison.stopTimes = new();
        return poison;
    }

    public override void OnFinished()
    {
    }

    protected override void Effect()
    {
        if (target != null)
        {
            int damages = target.IsInvincibleCount == 0 ? Stack * 3 : 0;
            FloatingTextGenerator.CreateEffectDamageText(damages, target.transform.position, poisonColor);
            target.gameObject.GetComponent<IDamageable>().ApplyDamage(damages, launcher, false);

            if (Utilities.IsPlayer(target))
                PostProcessingEffectManager.current.Play(PostProcessingEffects.Effect.Poison);
        }
    }

    protected override void PlayStatus()
    {
        AddStack(baseStack - 1);
    }
}
