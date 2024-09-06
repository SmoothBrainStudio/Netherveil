using System;
using System.Collections;
using UnityEngine;

public interface IActiveItem : IItem
{
    public static event Action<ItemEffect> OnActiveItemCooldownStartedTimeBased;
    public static event Action<ItemEffect> OnActiveItemCooldownUpdatedRoomBased;
    public float Cooldown { get; set; }
    public bool TimeBased { get; set; }

    void Activate();

    sealed IEnumerator WaitToUse()
    {
        ItemEffect effect = this as ItemEffect;

        if (!TimeBased)
        {
            OnActiveItemCooldownUpdatedRoomBased?.Invoke(effect);
            yield break;
        }
        
        OnActiveItemCooldownStartedTimeBased?.Invoke(effect);
        while (effect.CurrentEnergy < Cooldown)
        {
            effect.CurrentEnergy += Time.deltaTime;
            yield return null;
        }
    }

    void WaitToUseRoom()
    {
        ItemEffect effect = this as ItemEffect;
        effect.CurrentEnergy += 1;
        if(effect.CurrentEnergy > Cooldown)
        {
            effect.CurrentEnergy = Cooldown;
        }
        OnActiveItemCooldownUpdatedRoomBased?.Invoke(effect);
    }
}
