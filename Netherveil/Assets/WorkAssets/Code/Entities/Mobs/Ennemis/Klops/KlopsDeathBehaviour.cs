using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class KlopsDeathBehaviour : StateMachineBehaviour
{
    VisualEffect VFX;
    [SerializeField] float blastDiameter;
    [SerializeField] int blastDamage;
    Transform mobTransform;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mobTransform = animator.gameObject.transform.parent;
        VFX = mobTransform.GetComponent<KlopsStateMachine>().ExplodingVFX;
        VFX.SetFloat("TimeToExplode", stateInfo.length);
        VFX.SetFloat("ExplosionTime", 2.5f);
        VFX.SetFloat("ExplosionRadius", blastDiameter);
        VFX.transform.position = mobTransform.position;
        VFX.Play();
        CoroutineManager.Instance.StartCoroutine(Explosion(mobTransform.GetComponent<KlopsStateMachine>(), stateInfo.length));
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CoroutineManager.Instance.StartCoroutine(BeforeDestroy(animator, VFX.GetFloat("ExplosionTime")));
    }

    private IEnumerator BeforeDestroy(Animator animator, float time)
    {
        mobTransform.gameObject.SetActive(false);
        yield return new WaitForSeconds(time);
        Destroy(mobTransform.parent.gameObject);
    }

    private IEnumerator Explosion(IAttacker attacker, float time)
    {
        yield return new WaitForSeconds(time);
        AudioManager.Instance.PlaySound(AudioManager.Instance.BombItemSFX, mobTransform.position);
        Physics.OverlapSphere(VFX.transform.position, blastDiameter / 2f - blastDiameter / 8f, LayerMask.GetMask("Entity"))
            .Select(entity => entity.GetComponent<Hero>())
            .Where(entity => entity != null)
            .ToList()
            .ForEach(currentEntity =>
            {
                currentEntity.ApplyDamage(blastDamage, attacker);
            });
    }
}
