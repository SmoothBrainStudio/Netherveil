using UnityEditor;
using UnityEngine;

public static class ConvertUrpToToon
{
    public static Shader toonShader;

    [MenuItem("Tools/Convert Materials to ToonShader")]
    public static void ConvertMaterialsToToonShader()
    {
        string[] allMaterialGUIDs = AssetDatabase.FindAssets("t:Material");
        Shader toonShader = Shader.Find("Toon");

        foreach (string materialGUID in allMaterialGUIDs)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(materialGUID);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material != null)
            {
                if (!material.shader.name.Contains("Toon"))
                {
                    if (toonShader != null)
                    {
                        Material memMat = material;


                        material.shader = toonShader;

                        if (memMat.HasFloat("_Blend")) material.SetInt("_Blend", memMat.GetFloat("_Blend") > 0 ? 1 : 0);

                        material.SetColor("_1st_ShadeColor", new Color(0.67f, 0.67f, 0.67f, 1.0f));
                        material.SetColor("_2st_ShadeColor", new Color(0.33f, 0.33f, 0.33f, 1.0f));

                        material.SetFloat("_BaseColor_Step", 0.25f);
                        material.SetFloat("_BaseShade_Feather", 0.03f);
                        material.SetFloat("_ShadeColor_Step", 0.25f);
                        material.SetFloat("_1st2nd_Shades_Feather", 0.03f);

                        if (memMat.HasTexture("_BumpMap")) material.SetTexture("_NormalMap", material.GetTexture("_BumpMap"));
                        if (memMat.HasFloat("_BumpScale")) material.SetFloat("_NormalScale", material.GetFloat("_BumpScale"));

                        material.SetColor("_HighColor", new Color(0.64f, 0.62f, 0.67f, 1.0f));
                        material.SetFloat("_HighColor_Power", 0.4f);

                        material.SetFloat("_RimLight", 1);
                        material.SetColor("_RimLightColor", new Color(0.64f, 0.62f, 0.67f, 1.0f));
                        material.SetFloat("_RimLight_Power", 1.0f);
                        material.SetFloat("_RimLight_InsideMask", 0.65f);
                        material.SetFloat("_LightDirection_MaskOn", 1);
                        material.SetFloat("_Tweak_LightDirection_MaskLevel", 0.5f);
                        material.SetFloat("_Tweak_RimLightMaskLevel", -0.25f);

                        material.SetFloat("_MatCap", 1);
                        if (memMat.HasTexture("_MetallicGlossMap")) material.SetTexture("_MatCap_Sampler", material.GetTexture("_MetallicGlossMap"));
                        material.SetFloat("_Is_NormalMapForMatCap", 1);
                        if (memMat.HasTexture("_NormalMapForMatCap")) material.SetTexture("_MatCap_Sampler", material.GetTexture("_NormalMapForMatCap"));

                        if (memMat.HasTexture("_EmissionMap")) material.SetTexture("_Emissive_Tex", material.GetTexture("_EmissionMap"));

                        material.SetFloat("_Outline_Width", 5.0f);

                        EditorUtility.SetDirty(material);
                    }
                    else
                    {
                        Debug.LogError("ToonShader not found. Make sure the ToonShader is in your project.");
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
