using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class BloodLover : ItemEffect , IPassiveItem 
{
    readonly float thresholdValue = 0.25f;
    readonly float coefValue = 0.5f;
    bool alreadyApplied = false;
    VisualEffect berserkerVFX;

    public void OnRetrieved() 
    {
        berserkerVFX = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_Berserk"), Utilities.Player.transform.parent.Find("UnmovableVFXs")).GetComponent<VisualEffect>();
        berserkerVFX.SetSkinnedMeshRenderer("New SkinnedMeshRenderer", Utilities.Player.GetComponentInChildren<SkinnedMeshRenderer>());
        //here i take .parent of vfxTarget because rendering is better like this
        berserkerVFX.GetComponent<VFXPropertyBinder>().GetPropertyBinders<VFXTransformBinderCustom>().ToArray()[0].Target = Utilities.Player.GetComponentInChildren<VFXTarget>().transform.parent;
        Utilities.Hero.Stats.onStatChange += Effect;
    }
 
    public void OnRemove()
    {
        GameObject.Destroy(berserkerVFX);
        Utilities.Hero.Stats.onStatChange -= Effect;
    } 
    
    private void Effect(Stat stat)
    {
        if (stat != Stat.HP)
            return;

        Hero hero = Utilities.Hero;
        if(hero.Stats.GetValue(Stat.HP)/ hero.Stats.GetMaxValue(Stat.HP) <= thresholdValue && !alreadyApplied)
        {
            hero.Stats.IncreaseCoeffValue(Stat.ATK, coefValue);
            alreadyApplied = true;
            berserkerVFX.Play();
        }
        else if (hero.Stats.GetValue(Stat.HP) / hero.Stats.GetMaxValue(Stat.HP) > thresholdValue)
        {
            berserkerVFX.Stop();
            alreadyApplied = false;
        }
    }
} 
