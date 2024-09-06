using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [SerializeField] private float offset = 0.4f;
    [SerializeField] private float randomOffset = 0.03f;
    [SerializeField] private float timeFactor = 2.25f;
    [SerializeField] private Light lightobject;
    private float startIntensity;

    private void Start()
    {
        startIntensity = lightobject.intensity;

        if (lightobject == null)
        {
            Debug.LogError("A FlickeringLight don't have light !");
        }
    }

    private void Update()
    {
        if (lightobject == null)
            return;

        lightobject.intensity = startIntensity + Mathf.Sin(Time.time * timeFactor * Time.timeScale) * offset + Random.Range(-randomOffset, randomOffset);
    }
}
