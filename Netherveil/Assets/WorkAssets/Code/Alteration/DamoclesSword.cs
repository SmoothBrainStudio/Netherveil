using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class DamoclesSword : ConstantStatus
{
    VisualEffect vfx;
    readonly int damages;
    //used to make the vfx a little bit longer than the effect so that the OnFinished is perfectly timed on the sword drop
    readonly float VFX_DURATION_OFFSET = 0.2f;
    readonly float SWORD_BASE_OFFSET = 0.5f;
    readonly float SWORD_BEFORE_DROP_OFFSET = 0.75f;

    public DamoclesSword(float _duration, float _chance) : base(_duration, _chance)
    {
        isStackable = false;
        damages = Utilities.PlayerController.DAMOCLES_SWORD_DAMAGES;
    }

    public override Status DeepCopy()
    {
        DamoclesSword damoclesSword = (DamoclesSword)MemberwiseClone();
        return damoclesSword;
    }

    protected override void Effect()
    {
    }

    public override void OnFinished()
    {
        if (target == null)
            return;

        AudioManager.Instance.PlaySound(AudioManager.Instance.AresBladeSFX, target.transform.position);

        Physics.OverlapSphere(target.transform.position, vfx.GetAnimationCurve("SizeSlash").keys.Last().value / 2f, LayerMask.GetMask("Entity"))
        .Where(entity => entity.gameObject != (launcher as MonoBehaviour).gameObject)
        .Select(entity => entity.GetComponent<IDamageable>())
        .Where(entity => entity != null)
        .ToList()
        .ForEach(currentDamageable =>
        {
            if((currentDamageable as MonoBehaviour).TryGetComponent(out Entity entity) && entity.IsInvincibleCount == 0)
            {
                FloatingTextGenerator.CreateEffectDamageText(damages, (currentDamageable as MonoBehaviour).transform.position, Hero.corruptionColor);
                currentDamageable.ApplyDamage(damages, launcher, false);
            }
        });

        GameObject.Destroy(vfx.gameObject, 0.3f);
    }

    public override bool CanApplyEffect(Entity target)
    {
        return true;
    }

    protected override void PlayStatus()
    {
        vfx = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_CorruptedSword"), target.transform).GetComponent<VisualEffect>();
        vfx.SetFloat("Duration", duration + VFX_DURATION_OFFSET);

        AnimationCurve curve = vfx.GetAnimationCurve("SwordOffset");
        Keyframe[] keyframes = curve.keys;
        keyframes[0].value = target.GetComponent<CapsuleCollider>().height + SWORD_BASE_OFFSET;
        keyframes[1].value = keyframes[0].value;
        keyframes[2].value = keyframes[1].value + SWORD_BEFORE_DROP_OFFSET;

        for (int i = 0; i < keyframes.Length; i++)
        {
            curve.MoveKey(i, keyframes[i]);
        }

        vfx.SetAnimationCurve("SwordOffset", curve);
        vfx.Play();
    }
}
