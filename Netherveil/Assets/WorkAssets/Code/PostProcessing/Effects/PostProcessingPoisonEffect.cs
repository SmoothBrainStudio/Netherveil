using PostProcessingEffects.Effects;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcessingEffects
{
    [System.Serializable]
    public class PostProcessingPoisonEffect : PostProcessingAbstractEffect
    {
        [SerializeField] private VolumeProfile profile;
        [SerializeField] private float durationIn = 0.25f;
        [SerializeField] private float durationOut = 0.5f;

        protected override IEnumerator PlayRoutine(PostProcessingEffectManager manager)
        {
            manager.Volume.profile = profile;
            manager.effectIsPlaying = true;
            float elapsed = 0.0f;

            while (elapsed < durationIn)
            {
                elapsed = Mathf.Min(elapsed + Time.deltaTime, durationIn);
                float factor = elapsed / durationIn;
                float ease = EasingFunctions.EaseInQuint(factor);

                manager.Volume.weight = ease;

                yield return null;
            }

            elapsed = 0.0f;

            while (elapsed < durationOut)
            {
                elapsed = Mathf.Min(elapsed + Time.deltaTime, durationOut);
                float factor = elapsed / durationOut;
                float ease = 1.0f - EasingFunctions.EaseInQuint(factor);

                manager.Volume.weight = ease;

                yield return null;
            }

            manager.Volume.weight = 0.0f;
            manager.routine = null;
            manager.effectIsPlaying = false;
        }

        protected override IEnumerator StopRoutine(PostProcessingEffectManager manager)
        {
            throw new System.NotImplementedException();
        }
    }
}
