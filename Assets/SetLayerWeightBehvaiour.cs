using UnityEngine;

public class SetLayerWeightBehvaiour : StateMachineBehaviour
{
    [Range(0, 1)][SerializeField]int onEnterIndex = 1;
    [Range(0f, 1f)][SerializeField]float onEnterWeight = 1f;

    [SerializeField]bool resetOnExit = true;

    float oldWeight;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        oldWeight = animator.GetLayerWeight(onEnterIndex);

        animator.SetLayerWeight(onEnterIndex, onEnterWeight);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (resetOnExit)
            animator.SetLayerWeight(onEnterIndex, oldWeight);
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
