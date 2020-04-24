using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Target Spotted")]
public class AITargetSpottedState : AIBaseLocomotionState
{
    public override void Enter()
    {
        base.Enter();

        //alert other close AI
    }
    public override void Tick()
    {
        base.Tick();

        //make decision?
        //find cover?
        //attack
        base.TransitionTo<AILookForCover>();
    }
    public override void Exit()
    {
    }
}
