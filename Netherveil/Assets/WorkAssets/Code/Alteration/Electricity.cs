using PostProcessingEffects;
using System.Collections;
using UnityEngine;

public class Electricity : OverTimeStatus
{
    const float stunTime = 0.5f;
    private bool isStunCoroutineOn = false;
    public Electricity(float duration, float chance) : base(duration, chance)
    {
        isStackable = false;
        frequency = 1.0f;
        vfxName = "VFX_Electricity";
        
    }
    public override bool CanApplyEffect(Entity target)
    {
        return true;
    }

    public override Status DeepCopy()
    {
        Electricity electricity = (Electricity)MemberwiseClone();
        electricity.stopTimes = new();
        return electricity;
    }

    public override void OnFinished()
    {
        if(target as Mobs != null)
        {
            (target as Mobs).Agent.isStopped = false;
        }
    }

    protected override void Effect()
    {
        if (target != null && !isStunCoroutineOn)
        {
            CoroutineManager.Instance.StartCoroutine(Stun());

            if (Utilities.IsPlayer(target))
                PostProcessingEffectManager.current.Play(PostProcessingEffects.Effect.Electricity);
        }
    }

    private IEnumerator Stun()
    {
        isStunCoroutineOn = true;
        target.isFreeze += 1;
        target.GetComponentInChildren<Animator>().speed = 0;
        if (target as Mobs != null)
        {
            (target as Mobs).Agent.isStopped = true;
        }
        yield return new WaitForSeconds(stunTime);
        target.isFreeze -= 1;
        if (target as Mobs != null)
        {
            (target as Mobs).Agent.isStopped = false;
        }
        target.GetComponentInChildren<Animator>().speed = 1;
        isStunCoroutineOn = false;
    }

    protected override void PlayStatus()
    {
    }
}
