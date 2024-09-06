using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class VFXStopper : MonoBehaviour
{
    [SerializeField] VisualEffect effect;
    public float Duration { get; set; } = 0f;
    [SerializeField] bool destroyOnStop = false;
    [SerializeField] float destroyDurationDelay = 0f;
    public UnityEvent OnStop;


    private void Awake()
    {
        if(effect.HasFloat("Duration"))
        {
            Duration = effect.GetFloat("Duration");
        }
    }

    public void PlayVFX()
    {
        if(gameObject.activeInHierarchy)
        {
            effect.Play();
            StartCoroutine(StopVFXCoroutine());
        }
    }

    IEnumerator StopVFXCoroutine()
    {
        yield return new WaitForSeconds(Duration);
        effect.Stop();
        OnStop?.Invoke();
        if (destroyOnStop)
        {
            yield return new WaitForSeconds(destroyDurationDelay);
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
