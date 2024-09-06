using UnityEngine;

public class TransitionBehaviour : StateMachineBehaviour
{
    private bool isFinished = false;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        isFinished = false;
    }

    //OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (isFinished || stateInfo.normalizedTime <= 1.0f)
            return;

        if (!animator.TryGetComponent(out Transition transition))
            throw new System.Exception("An animator transition behaviour is not attach to a Transition.");

        transition.OnTransitionEnd?.Invoke();
        isFinished = true;
    }
}
