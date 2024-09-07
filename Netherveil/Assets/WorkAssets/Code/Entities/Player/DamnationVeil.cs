using System.Linq;
using UnityEngine;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class DamnationVeil : ISpecialAbility
{
    public float Cooldown { get; set; } = 40f;
    public float CurrentEnergy { get; set; } = 0;
    private readonly float radius = 10f;

    public DamnationVeil()
    {
        CurrentEnergy = Cooldown;
        float planeLength = 5f;
        radius = (Utilities.Player.GetComponent<PlayerController>().DamnationVeilVFX.GetFloat("Diameter") * planeLength) / 2f;
    }
    public void Activate()
    {
        PlayerController playerController = Utilities.Player.GetComponent<PlayerController>();
        playerController.PlayVFXAtPlayerPos(playerController.DamnationVeilVFX);
        ISpecialAbility.OnSpecialAbilityActivated?.Invoke();
        AudioManager.Instance.PlaySound(AudioManager.Instance.DamnationVeilSFX, Utilities.Player.transform.position);

        Physics.OverlapSphere(Utilities.Player.transform.position, radius, LayerMask.GetMask("Entity"))
            .Select(entity => entity.GetComponent<Mobs>())
            .Where(entity => entity != null)
            .ToList()
            .ForEach(currentEntity =>
            {
                currentEntity.AddStatus(new Damnation(5f, 1));
            });
    }
}
