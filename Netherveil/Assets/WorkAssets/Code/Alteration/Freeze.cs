using PostProcessingEffects;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Freeze : ConstantStatus
{
    Material freezeMat = null;
    public Freeze(float _duration, float _chance) : base(_duration, _chance)
    {
        isStackable = false;
        vfxName = "VFX_Frozen";
    }

    public override Status DeepCopy()
    {
        Freeze freeze = (Freeze)MemberwiseClone();
        return freeze;
    }

    protected override void Effect()
    {
        if (target != null)
        {
           // target.Stats.SetValue(Stat.SPEED, 0);
            target.isFreeze += 1;
            if (target as Mobs)
            {
                (target as Mobs).Agent.isStopped = true;
            }
        }
    }

    public override void OnFinished()
    {
        if(target == null)
            return;

        target.isFreeze -= 1;
        if(target as Mobs)
        {
            (target as Mobs).Agent.isStopped = false;
        }
        
        target.GetComponentInChildren<Animator>().speed = 1;
        Renderer[] renderers = target.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (Renderer renderer in renderers)
        {
            List<Material> materials = new List<Material>(renderer.materials);
            materials.RemoveAll(mat => mat.shader == freezeMat.shader);
            renderer.SetMaterials(materials);
        }

        if (Utilities.IsPlayer(target))
            PostProcessingEffectManager.current.Stop(PostProcessingEffects.Effect.Freeze);
    }

    public override bool CanApplyEffect(Entity target)
    {
        return target.Stats.HasStat(Stat.SPEED);
    }

    protected override void PlayStatus()
    {
        PlayVFX();
        PlayPostProcessing();
        target.GetComponentInChildren<Animator>().speed = 0;
        target.OnFreeze?.Invoke();
    }
    
    private void PlayVFX()
    {
        freezeMat = GameResources.Get<Material>("OutlineShaderMat");
        Renderer[] renderers = target.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach(Renderer renderer in renderers)
        {
            List<Material> materials = new List<Material>(renderer.materials)
            {
                freezeMat
            };
            renderer.SetMaterials(materials);
        }
    }

    private void PlayPostProcessing()
    {
        if (Utilities.IsPlayer(target))
            PostProcessingEffectManager.current.Play(PostProcessingEffects.Effect.Freeze);
    }
}
