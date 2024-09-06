using PostProcessingEffects.Effects;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcessingEffects
{
    [System.Serializable]
    public class PostProcessingBlessingEffect : PostProcessingAbstractEffect
    {
        [SerializeField] private VolumeProfile profile;
        [SerializeField] private float durationIn = 1.5f;
        [SerializeField] private float durationWait = 0.5f;
        [SerializeField] private float durationOut = 1.0f;

        protected override IEnumerator PlayRoutine(PostProcessingEffectManager manager)
        {
            manager.Volume.profile = profile;
            manager.effectIsPlaying = true;

            float elapsed = 0.0f;
            while (elapsed < durationIn)
            {
                elapsed = Mathf.Min(elapsed + Time.deltaTime, durationIn);
                float factor = elapsed / durationIn;
                float ease = EasingFunctions.EaseOutQuint(factor);

                manager.Volume.weight = ease;

                yield return null;
            }

            yield return new WaitForSeconds(durationWait);

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
            manager.effectIsPlaying = false;
            manager.routine = null;
        }

        protected override IEnumerator StopRoutine(PostProcessingEffectManager manager)
        {
            throw new System.NotImplementedException();
        }
    }
}
