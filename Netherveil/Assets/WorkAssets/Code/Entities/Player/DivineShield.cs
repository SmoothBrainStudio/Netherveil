using FMOD.Studio;
using System.Collections;
using UnityEngine;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class DivineShield : ISpecialAbility
{
    public float Cooldown { get; set; } = 30f;
    public float CurrentEnergy { get; set; } = 0f;

    private float currentTime = 0f;
    private EventInstance loopSound;

    public DivineShield()
    {
        CurrentEnergy = Cooldown;
    }

    public void Activate()
    {
        ISpecialAbility.OnSpecialAbilityActivated?.Invoke();
        PlayerController playerController = Utilities.Player.GetComponent<PlayerController>();
        playerController.DivineShieldVFX.SetFloat("Duration", Utilities.PlayerController.DIVINE_SHIELD_DURATION);
        playerController.DivineShieldVFX.Play();
        playerController.SpecialAbilityCoroutine = playerController.StartCoroutine(DisableDivineShield());
        Utilities.Hero.IsInvincibleCount++;
        AudioManager.Instance.PlaySound(AudioManager.Instance.DivineShieldSFX);
        loopSound = AudioManager.Instance.PlaySound(AudioManager.Instance.DivineShieldLoopSFX);
    }

    IEnumerator DisableDivineShield()
    {
        while (currentTime < Utilities.PlayerController.DIVINE_SHIELD_DURATION)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }
        Utilities.Player.GetComponent<PlayerController>().SpecialAbilityCoroutine = null;
        Utilities.Player.GetComponent<PlayerController>().DivineShieldVFX.Stop();
        AudioManager.Instance.StopSound(loopSound, STOP_MODE.ALLOWFADEOUT);
        currentTime = 0f;
        Utilities.Hero.IsInvincibleCount--;
        yield break;
    }
}
