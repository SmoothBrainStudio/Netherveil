using UnityEngine;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class DashBehaviour : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().StartOfDashAnimation();
        Utilities.PlayerController.RemoveCollisionOnDash(stateInfo.length);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //ensures that player is in state dash during animation
        if(stateInfo.normalizedTime < 1f)
            GameObject.FindWithTag("Player").GetComponent<Hero>().State = (int)Hero.PlayerState.DASH;
        else
            GameObject.FindWithTag("Player").GetComponent<Hero>().State = (int)Hero.PlayerState.MOTIONLESS;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().EndOfDashAnimation();
        foreach(var collider in Utilities.PlayerController.CollidersIgnored)
        {
           Physics.IgnoreCollision(Utilities.CharacterController, collider, false);
        }
        Utilities.PlayerController.CollidersIgnored.Clear();
    }
}
