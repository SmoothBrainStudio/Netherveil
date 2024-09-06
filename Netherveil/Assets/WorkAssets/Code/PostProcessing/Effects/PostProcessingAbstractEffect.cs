using System.Collections;

namespace PostProcessingEffects.Effects
{
    public abstract class PostProcessingAbstractEffect
    {
        public void Play(PostProcessingEffectManager manager)
        {
            manager.StartCoroutine(PlayRoutine(manager));
        }

        public void Stop(PostProcessingEffectManager manager)
        {
            manager.StartCoroutine(StopRoutine(manager));
        }

        protected abstract IEnumerator PlayRoutine(PostProcessingEffectManager manager);
        protected abstract IEnumerator StopRoutine(PostProcessingEffectManager manager);
    }
}
