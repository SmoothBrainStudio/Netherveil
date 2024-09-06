using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[Serializable]
public abstract class Status
{
    public IAttacker launcher = null;
    public Entity target;
    private VisualEffect VFX;
    protected string vfxName;
    public Status(float _duration, float _chance)
    {
        this.duration = _duration;
        this.statusChance = _chance;
        this.isFinished = false;
    }
   
    public abstract Status DeepCopy();

    #region Properties

    // If an effect is played cyclically, at which frequency ( in seconds )

    // Chance to apply a status ( 0 -> 1 )
    public float statusChance = 0.3f;
    // Duration of one stack of the effect
    protected float duration = 1;
    protected bool isStackable = false;
    #endregion

    #region Time
    public bool isFinished = false;
    protected float currentTime = 0;

    public List<float> stopTimes = new();

    #endregion

    protected event Action OnAddStack;
    #region Abstract Effect Functions
    protected abstract void Effect();
    public abstract bool CanApplyEffect(Entity target);
    public virtual void ApplyEffect(Entity target)
    {
        if (this.GetType() == typeof(Electricity) || this.GetType() == typeof(Freeze)) return;
        AddStack(1);
        PlayStatus();
        CoroutineManager.Instance.StartCoroutine(ManageStack());
    }

    // Do something when status is removed from the target
    public abstract void OnFinished();
    protected abstract void PlayStatus();
    #endregion

    #region Stack
    protected int stack = 0;
    protected int maxStack = int.MaxValue;
    //public event Action onAddingStack;
    public int Stack { get => stack; }
    public virtual void AddStack(int nb)
    {
        if ((isStackable && stack < maxStack) || stack < 1)
            OnAddStack?.Invoke();
    }
    public void RemoveStack(int nb)
    {
        if (isStackable)
            stack -= nb;
    }
    private IEnumerator ManageStack()
    {
        while (!isFinished)
        {
            if (!isFinished && stopTimes.Count > 0)
            {
                currentTime += Time.deltaTime;
                if (currentTime >= stopTimes[0])
                {
                    stopTimes.RemoveAt(0);
                    stack--;
                    if (stack <= 0)
                    {
                        isFinished = true;
                        if(target != null)
                            OnFinished();
                    }
                }
            }
            yield return null;
        }
        yield break;
    }
    #endregion

    // Update and play effect on a target
    public abstract void DoEffect();

    protected void PlayVfx(string vfxName)
    {
        if (string.IsNullOrEmpty(vfxName) || target.Stats.GetValue(Stat.HP) <= 0) return;
        if (target.statusVfxs.FirstOrDefault(x => x.name.Contains(vfxName)) == null)
        {
            
            VisualEffect vfx = GameObject.Instantiate(GameResources.Get<GameObject>(vfxName)).GetComponent<VisualEffect>();
            target.statusVfxs.Add(vfx);
            if (VFX == null) VFX = vfx;
            vfx.gameObject.GetComponent<VFXStopper>().OnStop.AddListener(RemoveVFXFromEntity);
            vfx.gameObject.GetComponent<VFXStopper>().Duration = stopTimes[^1];
            vfx.SetSkinnedMeshRenderer("New SkinnedMeshRenderer", target.gameObject.GetComponentInChildren<SkinnedMeshRenderer>());
            vfx.GetComponent<VFXPropertyBinder>().GetPropertyBinders<VFXTransformBinderCustom>().ToArray()[0].Target = target.gameObject.GetComponentInChildren<VFXTarget>().transform;
            vfx.gameObject.GetComponent<VFXStopper>().PlayVFX();
        }
        else
        {
            VFXStopper vfxStopper = target.statusVfxs.FirstOrDefault(x => x.name.Contains(vfxName)).GetComponent<VFXStopper>();
            vfxStopper.StopAllCoroutines();
            vfxStopper.Duration = stopTimes[^1];
            vfxStopper.PlayVFX();
        }
    }

    private void RemoveVFXFromEntity()
    {
        target.statusVfxs.Remove(VFX);
    }
}