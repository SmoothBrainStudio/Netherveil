using System.Collections;
using UnityEngine;

public static class UITween
{
    public static IEnumerator UpScaleCoroutine(this RectTransform t, float duration, float maxScale = 1.0f, float minScale = 0.0f)
    {
        float elapsed = 0.0f;
        float startScale = t.localScale.x;
        duration -= (startScale - minScale) / (maxScale - minScale) * duration;

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;
            float lerp = Mathf.Lerp(startScale, maxScale, factor);

            t.localScale = Vector3.one * lerp;

            yield return null;
        }
    }

    public static IEnumerator DownScaleCoroutine(this RectTransform t, float duration, float maxScale = 1.0f, float minScale = 0.0f)
    {
        float elapsed = 0.0f;
        float startScale = t.localScale.x;
        duration -= (startScale - maxScale) / (minScale - maxScale) * duration;

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;
            float lerp = Mathf.Lerp(startScale, minScale, factor);

            t.localScale = Vector3.one * lerp;

            yield return null;
        }
    }

    public static IEnumerator TranslateFromTo(this RectTransform t, float duration, Vector3 from, Vector3 to)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;
            Vector3 lerp = Vector3.Lerp(from, to, factor);

            t.position = lerp;

            yield return null;
        }
    }

    public static IEnumerator TranslateTo(this RectTransform t, float duration, Vector3 to)
    {
        float elapsed = 0.0f;
        Vector3 from = t.position;

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;
            Vector3 lerp = Vector3.Lerp(from, to, factor);

            t.position = lerp;

            yield return null;
        }
    }

    public static IEnumerator TranslateX(this RectTransform t, float duration, float toX)
    {
        float elapsed = 0.0f;
        Vector3 from = t.anchoredPosition;
        Vector3 to = t.anchoredPosition + new Vector2(toX, 0.0f);

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;
            Vector3 lerp = Vector3.Lerp(from, to, factor);

            t.anchoredPosition = lerp;

            yield return null;
        }
    }
}
