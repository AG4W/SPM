using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Fall")]
public class AIFallState : AIBaseState
{
    public override void Tick()
    {
        base.Tick();

        if (base.Actor.IsGrounded)
            base.TransitionTo<AIIdleState>();
    }
    public override void Exit()
    {
    }
}
