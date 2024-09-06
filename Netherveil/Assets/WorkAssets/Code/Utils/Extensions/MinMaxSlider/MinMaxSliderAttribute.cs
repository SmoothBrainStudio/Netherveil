using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class MinMaxSliderAttribute : PropertyAttribute
{
    public float min;
    public float max;

    public MinMaxSliderAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}