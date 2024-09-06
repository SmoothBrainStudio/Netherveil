using UnityEngine;

public class GorgonLaunchBehaviour : StateMachineBehaviour
{
    [SerializeField] float timeToRemoveHead = 0.6f;
    [SerializeField] float timeToLaunchHead = 0.8f;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > timeToRemoveHead)
        {
            animator.transform.parent.GetComponent<GorgonStateMachine>().HasRemovedHead = true;
        }
        if (stateInfo.normalizedTime > timeToLaunchHead)
        {
            animator.transform.parent.GetComponent<GorgonStateMachine>().HasLaunchAnim = true;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.transform.parent.GetComponent<GorgonStateMachine>().HasLaunchAnim = false;
        animator.transform.parent.GetComponent<GorgonStateMachine>().HasRemovedHead = false;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
