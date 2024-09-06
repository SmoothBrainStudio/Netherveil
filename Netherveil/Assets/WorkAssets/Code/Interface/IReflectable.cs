using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReflectable
{
    Vector3 Direction { get; set; }
    public bool IsReflected { get; set; }
    public bool CanBeReflected { get; set; }
    public void Reflect()
    {
        Direction = -Direction;
        IsReflected = true;
    }

    public void Reflect(Vector3 direction)
    {
        if (!CanBeReflected)
            return;

        AudioManager.Instance.PlaySound(AudioManager.Instance.ReflectSFX);
        Direction = direction;
        IsReflected = true;
    }
}
