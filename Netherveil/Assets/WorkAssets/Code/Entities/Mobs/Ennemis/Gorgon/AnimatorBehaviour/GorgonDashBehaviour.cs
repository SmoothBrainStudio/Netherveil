using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class GorgonDashBehaviour : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject gorgon = animator.transform.parent.gameObject;
        VisualEffect vfx = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_Dash_Gorgon")).GetComponent<VisualEffect>();
        vfx.gameObject.GetComponent<VFXStopper>().Duration = stateInfo.length * 3;
        vfx.SetSkinnedMeshRenderer("New SkinnedMeshRenderer", gorgon.GetComponentInChildren<SkinnedMeshRenderer>());
        vfx.GetComponent<VFXPropertyBinder>().GetPropertyBinders<VFXTransformBinderCustom>().ToArray()[0].Target = gorgon.GetComponentInChildren<VFXTarget>().transform;
        vfx.gameObject.GetComponent<VFXStopper>().PlayVFX();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    //animator.transform.parent.gameObject.GetComponent<Gorgon>().dashVFX.Stop();
    //}

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
