using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcessingEffects
{
    public enum Effect
    {
        Hit,
        Blessing,
        Freeze,
        Electricity,
        Fire,
        Damnation,
        Bleeding,
        Poison
    }

    public class PostProcessingEffectManager : MonoBehaviour
    {
        static private PostProcessingEffectManager instance;
        static public PostProcessingEffectManager current
        {
            get
            {
                if (instance == null)
                    throw new Exception("No PostProcessingEffectManager in the scene !");

                return instance;
            }
        }

        [SerializeField] private Volume volume;
        public Coroutine routine = null;
        public bool effectIsPlaying = false;

        public Volume Volume => volume;

        [Header("All effects")]
        [SerializeField] private PostProcessingHitEffect hitEffect;
        [SerializeField] private PostProcessingFreezeEffect freezeEffect;
        [SerializeField] private PostProcessingBlessingEffect blessingEffect;
        [SerializeField] private PostProcessingElectricityEffect electricityEffect;
        [SerializeField] private PostProcessingDamnationEffect damnationEffect;
        [SerializeField] private PostProcessingBleedingEffect bleedingEffect;
        [SerializeField] private PostProcessingPoisonEffect poisonEffect;
        [SerializeField] private PostProcessingFireEffect fireEffect;

        private void Awake()
        {
            instance = this;
        }

        public void Enable()
        {
            volume.gameObject.SetActive(true);

            Hero hero = Utilities.Hero;
            if (hero != null)
            {
                hero.OnDeath += StopAllEffect;
            }
        }

        private void OnDisable()
        {
            Hero hero = Utilities.Hero;
            if (hero != null)
            {
                hero.OnDeath -= StopAllEffect;
            }
        }

        public void Disable()
        {
            volume.gameObject.SetActive(false);
        }

        public void Play(Effect effect, bool forceCancelPrevious = true)
        {
            if (effectIsPlaying)
            {
                if (!forceCancelPrevious)
                    return;

                effectIsPlaying = false;
                if (routine != null)
                    StopCoroutine(routine);
            }

            volume.weight = 0.0f;

            switch (effect)
            {
                case Effect.Hit:
                    hitEffect.Play(this);
                    break;
                case Effect.Blessing:
                    blessingEffect.Play(this);
                    break;
                case Effect.Freeze:
                    freezeEffect.Play(this);
                    break;
                case Effect.Electricity:
                    electricityEffect.Play(this);
                    break;
                case Effect.Fire:
                    fireEffect.Play(this);
                    break;
                case Effect.Damnation:
                    damnationEffect.Play(this);
                    break;
                case Effect.Bleeding:
                    bleedingEffect.Play(this);
                    break;
                case Effect.Poison:
                    poisonEffect.Play(this);
                    break;
            }
        }


        public void Stop(int effect)
        {
            Effect cur = (Effect)Enum.Parse(typeof(Effect), effect.ToString());
            Stop(cur, true);
        }

        public void StopAllEffect(Vector3 _)
        {
            effectIsPlaying = false;
        }

        public void Stop(Effect effect, bool forceCancelPrevious = true)
        {
            if (routine != null)
            {
                if (!forceCancelPrevious)
                {
                    Debug.LogWarning("An post processing effect is currently use.");
                    return;
                }

                StopCoroutine(routine);
            }

            switch (effect)
            {
                case Effect.Freeze:
                    freezeEffect.Stop(this);
                    break;
                case Effect.Damnation:
                    damnationEffect.Stop(this);
                    break;
            }
        }
    }
}
