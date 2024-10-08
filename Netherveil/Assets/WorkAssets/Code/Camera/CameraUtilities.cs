using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

public class CameraUtilities : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private float shakeTimer;
    private float shakeTotalTime;
    private float startingIntensity;
    public static bool toggleScreenShake = true;

    [HideInInspector] public float defaultFOV;

    private void Start()
    {
        // Set the default field of view and initialize shake values
        defaultFOV = virtualCamera.m_Lens.FieldOfView;
        shakeTimer = 0f;
        shakeTotalTime = 0f;
        startingIntensity = 0f;
    }

    // Smoothly changes the camera's field of view (FOV) over a specified duration using an easing function.
    public void ChangeFov(float _reachedFOV, float _duration, Func<float, float> easingFunction)
    {
        StartCoroutine(ChangeFovCoroutine(_reachedFOV, _duration, easingFunction));
    }

    // Coroutine to animate the FOV change over time
    private IEnumerator ChangeFovCoroutine(float _reachedFOV, float _duration, Func<float, float> easingFunction)
    {
        float elapsedTime = 0f;
        float initialFOV = virtualCamera.m_Lens.FieldOfView;

        // Gradually interpolate FOV using the provided easing function
        while (elapsedTime < _duration)
        {
            float t = elapsedTime / _duration;
            float currentFOV = Mathf.Lerp(initialFOV, _reachedFOV, easingFunction(t));
            virtualCamera.m_Lens.FieldOfView = currentFOV;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final FOV is set to the target value
        virtualCamera.m_Lens.FieldOfView = _reachedFOV;
    }

    // Initiates a camera shake effect with a specified intensity and duration.
    public void ShakeCamera(float _intensity, float _time, Func<float, float> easingFunction)
    {
        if (toggleScreenShake)
        {
            // Access Cinemachine noise component to control the shake effect
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = _intensity;

            // Store initial shake values
            startingIntensity = _intensity;
            shakeTotalTime = _time;
            shakeTimer = _time;

            // Start shake coroutine to reduce intensity over time
            StartCoroutine(ShakeCameraCoroutine(easingFunction));
        }
    }

    // Coroutine that manages the camera shake over time, gradually reducing the intensity
    private IEnumerator ShakeCameraCoroutine(Func<float, float> easingFunction)
    {
        while (shakeTimer > 0f)
        {
            // Decrease shake timer
            shakeTimer -= Time.deltaTime;

            // Access Cinemachine noise settings to adjust shake frequency
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            // Calculate the progression of the shake effect and apply easing
            float shakeProgression = 1 - (shakeTimer / shakeTotalTime);
            cinemachineBasicMultiChannelPerlin.m_FrequencyGain = Mathf.Lerp(startingIntensity, 0f, easingFunction(shakeProgression));

            yield return null;
        }
    }
}