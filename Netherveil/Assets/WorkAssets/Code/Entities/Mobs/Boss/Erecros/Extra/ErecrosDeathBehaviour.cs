using UnityEngine;

public class ErecrosDeathBehaviour : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.DeathVFXSFX, animator.transform.parent.position);
        Destroy(Instantiate(GameResources.Get<GameObject>("VFX_Death"), animator.transform.parent.position, Quaternion.identity), 30f);
        Destroy(animator.transform.parent.parent.gameObject);

        LevelLoader.current.LoadScene("Outro", true);
    }
}
