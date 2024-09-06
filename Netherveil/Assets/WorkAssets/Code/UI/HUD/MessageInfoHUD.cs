using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MessageInfoHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;

    Func<float, float> defaultEasing = e => e;
    private float baseDurationIn = 2.0f;
    private float baseDurationOut = 2.0f;
    private float baseDurationWait = 2.0f;
    private Coroutine displayRoutine;

    private void OnDisable()
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        textMesh.alpha = 0.0f;
    }

    public void Display(string message)
    {
        Display(message, baseDurationIn, baseDurationOut, baseDurationWait, defaultEasing);
    }

    public void Display(string message, float durationIn, float durationOut, float durationWait)
    {
        Display(message, durationIn, durationOut, durationWait, defaultEasing);
    }

    public void Display(string message, float durationIn, float durationOut, float durationWait, Func<float, float> easing)
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        textMesh.text = message;
        displayRoutine = StartCoroutine(DisplayRoutine(durationIn, durationOut, durationWait, easing));
    }

    private IEnumerator DisplayRoutine(float durationIn, float durationOut, float durationWait, Func<float, float> easing)
    {
        float elapsed = 0.0f;

        while (elapsed < durationIn)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, durationIn);
            float factor = elapsed / durationIn;
            float ease = easing(factor);

            textMesh.alpha = ease;

            yield return null;
        }

        textMesh.alpha = 1.0f;
        yield return new WaitForSeconds(durationWait);
        elapsed = 0.0f;

        while (elapsed < durationOut)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, durationOut);
            float factor = elapsed / durationOut;
            float ease = 1.0f - easing(factor);

            textMesh.alpha = ease;

            yield return null;
        }

        textMesh.alpha = 0.0f;
    }
}
