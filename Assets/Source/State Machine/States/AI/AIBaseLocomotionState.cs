using UnityEngine;

public abstract class AIBaseLocomotionState : AIBaseState
{
    public override void Tick()
    {
        base.Tick();

        if (!base.Actor.IsGrounded)
            base.TransitionTo<AIFallState>();
    }
}
