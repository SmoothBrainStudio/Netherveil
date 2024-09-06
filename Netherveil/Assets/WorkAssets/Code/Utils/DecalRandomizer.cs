using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class DecalRandomizer : MonoBehaviour
{
    [SerializeField] private Material[] materials;

    void Awake()
    {
        DecalProjector decalProjector = GetComponent<DecalProjector>();
        decalProjector.material = materials[UnityEngine.Random.Range(0, materials.Length)];
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, UnityEngine.Random.Range(0f, 360f), transform.eulerAngles.z);
    }
}
