using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HitMaterialApply : MonoBehaviour
{
    [SerializeField] private Renderer[] mRenderer;
    private Material mMaterial;
    private Coroutine routine;
    Func<float, float> defaultEasing = e => e;

    private bool isEnable = false;
    public bool IsEnable => isEnable;

    void Awake()
    {
        mMaterial = GameResources.Get<Material>("MAT_Entity_Hit");

        if (mRenderer == null)
            mRenderer = GetComponentsInChildren<MeshRenderer>();
        else if (mRenderer.Length == 0)
            mRenderer = GetComponentsInChildren<MeshRenderer>();
    }

    public void SetAlpha(float alpha)
    {
        if (!IsEnable)
            return;

        foreach (Renderer renderer in mRenderer)
        {
            List<Material> materials = new List<Material>(renderer.materials).Where(mat => mat.shader == mMaterial.shader).ToList();

            materials.ForEach(x =>
            {
                x.SetFloat("_alpha", alpha);
            });
        }
    }

    public void SetAlpha(float from, float to, float duration)
    {
        SetAlpha(from, to, duration, defaultEasing, null);
    }

    public void SetAlpha(float from, float to, float duration, Func<float, float> easingFunction)
    {
        SetAlpha(from, to, duration, easingFunction, null);
    }

    public void SetAlpha(float from, float to, float duration, Action onFinish)
    {
        SetAlpha(from, to, duration, defaultEasing, onFinish);
    }

    public void SetAlpha(float from, float to, float duration, Func<float, float> easingFunction, Action onFinish)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(SetAlphaRoutine(from, to, duration, easingFunction, onFinish));
    }

    public void EnableMat()
    {
        if (IsEnable)
            return;

        isEnable = true;

        foreach (Renderer renderer in mRenderer)
        {
            List<Material> materials = new List<Material>(renderer.materials)
            {
                mMaterial
            };
            renderer.SetMaterials(materials);
        }
    }

    public void DisableMat()
    {
        if (!IsEnable)
            return;

        isEnable = false;

        foreach (Renderer renderer in mRenderer)
        {
            List<Material> materials = new List<Material>(renderer.materials);
            materials.RemoveAll(mat => mat.shader == mMaterial.shader);
            renderer.SetMaterials(materials);
        }
    }

    private IEnumerator SetAlphaRoutine(float from, float to, float duration, Func<float, float> easingFunction, Action onFinish)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            float factor = elapsed / duration;
            float ease = easingFunction(factor);
            float result = Mathf.Lerp(from, to, ease);

            SetAlpha(result);

            yield return null;
        }

        SetAlpha(to);
        onFinish?.Invoke();
    }
}
