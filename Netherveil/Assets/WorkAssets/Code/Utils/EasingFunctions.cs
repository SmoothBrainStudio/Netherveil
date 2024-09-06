using UnityEngine;

public static class EasingFunctions
{
    public enum EaseName
    {
        EaseInBack,
        EaseInBounce,
        EaseInCirc,
        EaseInCubic,
        EaseInElastic,
        EaseInExpo,
        EaseInOutBack,
        EaseInOutBounce,
        EaseInOutCirc,
        EaseInOutCubic,
        EaseInOutElastic,
        EaseInOutExpo,
        EaseInOutQuad,
        EaseInOutQuart,
        EaseInOutQuint,
        EaseInOutSin,
        EaseInQuad,
        EaseInQuint,
        EaseInSin,
        EaseOutBack,
        EaseOutBounce,
        EaseOutCirc,
        EaseOutCubic,
        EaseOutElastic,
        EaseOutExpo,
        EaseOutQuad,
        EaseOutQuart,
        EaseOutQuint,
        EaseOutSin
    }

    public static float EaseInQuad(float t)
    {
        return t * t;
    }

    public static float EaseOutQuad(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }

    public static float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    }

    public static float EaseInCubic(float t)
    {
        return t * t * t;
    }

    public static float EaseOutCubic(float t)
    {
        float f = (t - 1);
        return f * f * f + 1;
    }

    public static float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
    }

    public static float EaseInQuart(float t)
    {
        return t * t * t * t;
    }

    public static float EaseOutQuart(float t)
    {
        float f = (t - 1);
        return f * f * f * (1 - t) + 1;
    }

    public static float EaseInOutQuart(float t)
    {
        return t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
    }

    public static float EaseInQuint(float t)
    {
        return t * t * t * t * t;
    }

    public static float EaseOutQuint(float t)
    {
        float f = (t - 1);
        return f * f * f * f * f + 1;
    }

    public static float EaseInOutQuint(float t)
    {
        return t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
    }

    public static float EaseInSin(float t)
    {
        return 1 - Mathf.Cos(t * Mathf.PI / 2);
    }

    public static float EaseOutSin(float t)
    {
        return Mathf.Sin(t * Mathf.PI / 2);
    }

    public static float EaseInOutSin(float t)
    {
        return (1 - Mathf.Cos(Mathf.PI * t)) / 2;
    }

    public static float EaseInExpo(float t)
    {
        return Mathf.Pow(2, 10 * (t - 1));
    }

    public static float EaseOutExpo(float t)
    {
        return 1 - Mathf.Pow(2, -10 * t);
    }

    public static float EaseInOutExpo(float t)
    {
        t /= 0.5f;
        if (t < 1) return 0.5f * Mathf.Pow(2, 10 * (t - 1));
        t--;
        return 0.5f * (-Mathf.Pow(2, -10 * t) + 2);
    }

    public static float EaseInCirc(float t)
    {
        return 1 - Mathf.Sqrt(Mathf.Max(0, 1 - t * t));
    }

    public static float EaseOutCirc(float t)
    {
        return Mathf.Sqrt(Mathf.Max(0, 1 - (t - 1) * (t - 1)));
    }

    public static float EaseInOutCirc(float t)
    {
        if (t < 0.25f)
            return (1 - Mathf.Sqrt(1 - 4 * t * t)) / 2;
        else if (t < 0.75f)
            return (Mathf.Sqrt(1 - 4 * (t - 0.5f) * (t - 0.5f)) + 1) / 2;
        else
            return (Mathf.Sqrt(1 - 4 * (t - 1) * (t - 1)) + 1) / 2;
    }

    public static float EaseInElastic(float t)
    {
        float c4 = (2 * Mathf.PI) / 3;

        return t == 0 ? 0 : t == 1 ? 1 : -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * c4);
    }

    public static float EaseOutElastic(float t)
    {
        float c4 = (2 * Mathf.PI) / 3;

        return t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c4) + 1;
    }

    public static float EaseInOutElastic(float t)
    {
        float c5 = (2 * Mathf.PI) / 4.5f;

        return t == 0 ? 0 : t == 1 ? 1 : t < 0.5f ?
            -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2 :
            (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2 + 1;
    }

    public static float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return c3 * t * t * t - c1 * t * t;
    }

    public static float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    public static float EaseInOutBack(float t)
    {
        float c1 = 1.70158f;
        float c2 = c1 * 1.525f;

        return t < 0.5f ?
            (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2 :
            (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
    }

    public static float EaseInBounce(float t)
    {
        return 1 - EaseOutBounce(1 - t);
    }

    public static float EaseOutBounce(float t)
    {
        if (t < 1 / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2 / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }

    public static float EaseInOutBounce(float t)
    {
        return t < 0.5f ? (1 - EaseOutBounce(1 - 2 * t)) / 2 : (1 + EaseOutBounce(2 * t - 1)) / 2;
    }
}