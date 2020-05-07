using UnityEngine;

public abstract class BaseLocomotionState : BaseState
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
    }
    public override void Tick()
    {
        base.Tick();

        if (!base.Actor.IsGrounded)
            base.TransitionTo<FallState>();
        if (Input.GetKeyDown(KeyCode.Mouse1))
            base.TransitionTo<AimState>();
        if (Input.GetKeyDown(KeyCode.Space))
            base.TransitionTo<RollState>();

        //base.TransitionTo<JumpState>();
        base.Actor.Raise(ActorEvent.SetTargetStance, Input.GetKey(KeyCode.C) ? Stance.Crouched : Stance.Standing);
    }
}
