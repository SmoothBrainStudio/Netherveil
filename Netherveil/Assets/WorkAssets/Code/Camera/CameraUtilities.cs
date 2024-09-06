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
    static public bool toggleScreenShake = true;

    [HideInInspector] public float defaultFOV;

    private void Start()
    {
        defaultFOV = virtualCamera.m_Lens.FieldOfView;
        shakeTimer = 0f;
        shakeTotalTime = 0f;
        startingIntensity = 0f;
    }

    public void ChangeFov(float _reachedFOV, float _duration, Func<float, float> easingFunction)
    {
        StartCoroutine(ChangeFovCoroutine(_reachedFOV, _duration, easingFunction));
    }

    private IEnumerator ChangeFovCoroutine(float _reachedFOV, float _duration, Func<float, float> easingFunction)
    {
        float elapsedTime = 0f;
        float initialFOV = virtualCamera.m_Lens.FieldOfView;

        while (elapsedTime < _duration)
        {
            float t = elapsedTime / _duration;
            float currentFOV = Mathf.Lerp(initialFOV, _reachedFOV, easingFunction(t));
            virtualCamera.m_Lens.FieldOfView = currentFOV;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        virtualCamera.m_Lens.FieldOfView = _reachedFOV;
    }

    public void ShakeCamera(float _intensity, float _time, Func<float, float> easingFunction)
    {
        if (toggleScreenShake)
        {
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = _intensity;
            startingIntensity = _intensity;
            shakeTotalTime = _time;
            shakeTimer = _time;
            StartCoroutine(ShakeCameraCoroutine(easingFunction));
        }
    }

    private IEnumerator ShakeCameraCoroutine(Func<float, float> easingFunction)
    {
        while (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            float shakeProgression = 1 - (shakeTimer / shakeTotalTime);
            cinemachineBasicMultiChannelPerlin.m_FrequencyGain = Mathf.Lerp(startingIntensity, 0f, easingFunction(shakeProgression));
            yield return null;
        }
    }
}
