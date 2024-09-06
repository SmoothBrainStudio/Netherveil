using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;

[Serializable]
public class Sound
{
    [SerializeField] private EventReference reference;
    private EventInstance instance;

    public PLAYBACK_STATE GetState()
    {
        if (!instance.isValid())
        {
            return PLAYBACK_STATE.STOPPED;
        }

        instance.getPlaybackState(out PLAYBACK_STATE state);
        return state;
    }

    private bool CreateInstance(bool restart = false)
    {
        if (IsNull)
        {
            Debug.LogError("Sound doesn't have a valid Guid " + reference.Guid);
            return false;
        }
        else if (!instance.isValid())
        {
            instance = RuntimeManager.CreateInstance(reference);
            return true;
        }

        if (GetState() == PLAYBACK_STATE.STOPPED || restart)
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance = RuntimeManager.CreateInstance(reference);
            return true;
        }

        return false;
    }

    public void Play(bool restart = true)
    {
        if (!CreateInstance(restart))
        {
            return;
        }

        instance.start();
        AudioManager.Instance.AddSound(ref instance);
    }

    public void Play(Vector3 worldPosition, bool restart = false)
    {
        if (!CreateInstance(restart))
        {
            return;
        }

        instance.start();
        instance.set3DAttributes(worldPosition.To3DAttributes());
        AudioManager.Instance.AddSound(ref instance);
    }

    public void Stop(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.Immediate)
    {
        instance.stop(stopMode);
        AudioManager.Instance.RemoveSound(ref instance);
    }

    public bool IsNull
    {
        get
        {
            return reference.IsNull;
        }
    }
}