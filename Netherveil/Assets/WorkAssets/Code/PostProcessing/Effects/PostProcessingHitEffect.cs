using PostProcessingEffects.Effects;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PostProcessingEffects
{
    [System.Serializable]
    public class PostProcessingHitEffect : PostProcessingAbstractEffect
    {
        [SerializeField] private VolumeProfile profile;
        [SerializeField] private float duration = 0.5f;
        private Vignette vignette;

        protected override IEnumerator PlayRoutine(PostProcessingEffectManager manager)
        {
            manager.Volume.profile = profile;
            manager.effectIsPlaying = true;
            float elapsed = 0.0f;

            if (profile.TryGet(out vignette))
            {
                if (Utilities.Hero.LastDamagesSuffered > 10 &&  Utilities.Hero.LastDamagesSuffered < 50)
                {
                    vignette.intensity.value = .65f;
                }
                else if (Utilities.Hero.LastDamagesSuffered >= 20 && Utilities.Hero.LastDamagesSuffered < 25)
                {
                    vignette.intensity.value = .8f;
                }
                else if (Utilities.Hero.LastDamagesSuffered >= 25)
                {
                    vignette.intensity.value = .9f;
                }
            }

            while (elapsed < duration)
            {
                elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
                float factor = elapsed / duration;
                float ease = Mathf.Sin(factor * Mathf.PI);

                manager.Volume.weight = ease;

                yield return null;
            }

            manager.Volume.weight = 0.0f;
            manager.routine = null;
            manager.effectIsPlaying = false;
            vignette.intensity.value = 0.5f;
        }

        public IEnumerator PlayRoutine(PostProcessingLowHP manager)
        {
            manager.Volume.profile = profile;
            manager.effectIsPlaying = true;
            float elapsed = 0.0f;

            if (profile.TryGet(out vignette))
            {
               vignette.intensity.value = .5f;
            }

            while (elapsed < duration)
            {
                elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
                float factor = elapsed / duration;
                float ease = Mathf.Sin(factor * Mathf.PI);

                manager.Volume.weight = ease;

                yield return null;
            }

            manager.Volume.weight = 0.0f;
            manager.routine = null;
            manager.effectIsPlaying = false;
            vignette.intensity.value = 0.5f;
        }

        protected override IEnumerator StopRoutine(PostProcessingEffectManager manager)
        {
            throw new System.NotImplementedException();
        }
    }
}
