using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class RenderVFX : MonoBehaviour
{
    VisualEffect[] VFXs;
    void Start()
    {
        var VFXFounded = FindObjectsOfType<VisualEffect>();
        VFXs = VFXFounded;
        foreach (VisualEffect effect in VFXs)
        {
            Debug.Log(effect.name);
        }
    }

    IEnumerator PlayAllVFX()
    {
        for (int i = 0; i < VFXs.Length; i++)
        {
            while (VFXs[i].aliveParticleCount <= 0)
            {
                VFXs[i].Play();
                yield return new WaitForSeconds(3);
                VFXs[i].Stop();
                yield return new WaitForSeconds(2);
                i++;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(PlayAllVFX());
        }
    }
}
