using PostProcessingEffects.Effects;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcessingEffects
{
    [System.Serializable]
    public class PostProcessingFreezeEffect : PostProcessingAbstractEffect
    {
        [SerializeField] private VolumeProfile profile;
        [SerializeField] private float durationIn = 0.5f;
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
                float ease = EasingFunctions.EaseOutSin(factor);

                manager.Volume.weight = ease;

                yield return null;
            }

            manager.routine = null;
        }

        protected override IEnumerator StopRoutine(PostProcessingEffectManager manager)
        {
            manager.Volume.profile = profile;
            float elapsed = 0.0f;

            while (elapsed < durationOut)
            {
                elapsed = Mathf.Min(elapsed + Time.deltaTime, durationOut);
                float factor = elapsed / durationOut;
                float ease = 1.0f - EasingFunctions.EaseOutSin(factor);

                manager.Volume.weight = ease;

                yield return null;
            }

            manager.routine = null;
            manager.effectIsPlaying = false;
        }
    }
}
