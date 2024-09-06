using FMODUnity;
using UnityEngine;

public class ProjectileLaunchTrap : MonoBehaviour , IActivableTrap
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform launchTransform;
    // FMOD
    [SerializeField] private EventReference throwProjectilSFX;
    private FMOD.Studio.EventInstance throwProjectilEvent;

    private void Awake()
    {
        throwProjectilEvent = RuntimeManager.CreateInstance(throwProjectilSFX);
    }

    public void Active()
    {
        AudioManager.Instance.StopSound(throwProjectilEvent, FMOD.Studio.STOP_MODE.IMMEDIATE);
        AudioManager.Instance.PlaySound(throwProjectilSFX);
        Instantiate(projectilePrefab, launchTransform.position, transform.rotation);
    }
}
