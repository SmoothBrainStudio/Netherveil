using System.Linq;
using UnityEngine;

public class RuneOfSloth : ItemEffect, IActiveItem
{
    public float Cooldown { get; set; } = 30f;
    public bool TimeBased { get; set; } = true;
    private readonly float duration = 3f;
#pragma warning disable IDE0052 // Supprimer les membres privés non lus
    private readonly float displayValue;
#pragma warning restore IDE0052 // Supprimer les membres privés non lus

    public RuneOfSloth()
    {
        displayValue = Cooldown;
    }

    public void Activate()
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.RuneOfSlothSFX,Utilities.Player.transform.position);
        Utilities.Player.GetComponent<PlayerController>().PlayVFXAtPlayerPos(Utilities.Player.GetComponent<PlayerController>().RuneOfSlothVFX);
        float planeLength = 5f;
        float radius = (Utilities.Player.GetComponent<PlayerController>().RuneOfSlothVFX.GetFloat("Diameter") * planeLength) / 2f;

        Physics.OverlapSphere(Utilities.Player.transform.position, radius, LayerMask.GetMask("Entity"))
            .Select(entity => entity.GetComponent<Mobs>())
            .Where(entity => entity != null)
            .ToList()
            .ForEach(currentEntity =>
            {
                currentEntity.AddStatus(new Freeze(duration, 1));
            });
    }

    public void OnRetrieved()
    {
        
    }

    public void OnRemove()
    {
    }
} 
