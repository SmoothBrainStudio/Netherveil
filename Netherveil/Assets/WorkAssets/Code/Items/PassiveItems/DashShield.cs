using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class DashShield : ItemEffect , IPassiveItem 
{
    private bool hasShield = false;
    public void OnRetrieved() 
    {
        Utilities.PlayerInput.OnStartDash += ApplyShield;
        Utilities.PlayerInput.OnEndDash += RemoveShield;
        Utilities.PlayerInput.OnEndDashAttack += RemoveShield;
        hasShield = false;
    } 
 
    public void OnRemove() 
    {
        Utilities.PlayerInput.OnStartDash -= ApplyShield;
        Utilities.PlayerInput.OnEndDash -= RemoveShield;
        Utilities.PlayerInput.OnEndDashAttack -= RemoveShield;
        hasShield = false;
    } 
 
    private void ApplyShield()
    {
        Utilities.Hero.IsInvincibleCount++;
        Utilities.PlayerController.DashShieldVFX.Reinit();
        Utilities.PlayerController.DashShieldVFX.Play();
        AudioManager.Instance.PlaySound(AudioManager.Instance.DashShieldSFX);
        hasShield = true;
        CoroutineManager.Instance.StartCoroutine(Shield());
    }

    private void RemoveShield(Vector3 playerPos)
    {
        Utilities.Hero.IsInvincibleCount--;
        Utilities.PlayerController.DashShieldVFX.Stop();
    }

    private IEnumerator Shield()
    {
        while (hasShield)
        {
            float playerheight = Utilities.CharacterController.height;
            float playerRadius = Utilities.CharacterController.radius;
            Vector3 basePlayer = Utilities.Player.transform.position;
            Vector3 finalPos = basePlayer;
            finalPos.y += playerheight;
            List<IReflectable> reflectables = Physics.OverlapCapsule(basePlayer, finalPos, playerRadius).Where(x => x.TryGetComponent<IReflectable>(out var reflectable) && !reflectable.IsReflected).Select(x => x.GetComponent<IReflectable>()).ToList();
            if (reflectables.Count > 0)
            {
                foreach(IReflectable reflectable in reflectables)
                {
                    reflectable.Reflect();
                }
            }
            yield return null;
        }
    }
} 
