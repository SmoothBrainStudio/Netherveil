using System.Collections.Generic;
using UnityEngine;

public class Outline : MonoBehaviour
{
    [SerializeField] private Renderer[] mRenderer;
    private Material mOutlineMaterial;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField, Range(0.01f, 0.5f)] private float outlineThickness = 0.02f;

    void Awake()
    {
        mOutlineMaterial = GameResources.Get<Material>("OutlineShaderMat");

        if (mRenderer == null)
            mRenderer = GetComponentsInChildren<MeshRenderer>();
        else if (mRenderer.Length == 0)
            mRenderer = GetComponentsInChildren<MeshRenderer>();

        mOutlineMaterial.SetColor("_Outline_Color", outlineColor);
        mOutlineMaterial.SetFloat("_Outline_thickness", outlineThickness);
    }

    public void EnableOutline()
    {
        foreach (Renderer renderer in mRenderer)
        {
            List<Material> materials = new List<Material>(renderer.materials)
            {
                mOutlineMaterial
            };
            renderer.SetMaterials(materials);
        }
    }

    public void DisableOutline()
    {
        foreach (Renderer renderer in mRenderer)
        {
            List<Material> materials = new List<Material>(renderer.materials);
            materials.RemoveAll(mat => mat.shader == mOutlineMaterial.shader);
            renderer.SetMaterials(materials);
        }
    }
}
