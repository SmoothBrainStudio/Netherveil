using UnityEngine;

public class MobDeathBehaviour : StateMachineBehaviour
{
    bool VFXPlayed = false;
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > 0.9f && !VFXPlayed)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.DeathVFXSFX, animator.transform.parent.position);
            GameObject.Destroy(GameObject.Instantiate(GameResources.Get<GameObject>("VFX_Death"), animator.transform.parent.position, Quaternion.identity), 30f);
            VFXPlayed = true;
        }
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Destroy(animator.transform.parent.parent.gameObject);
    }
}
