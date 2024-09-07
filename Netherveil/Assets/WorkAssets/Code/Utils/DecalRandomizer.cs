using UnityEngine;
using UnityEngine.Rendering.Universal;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
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
