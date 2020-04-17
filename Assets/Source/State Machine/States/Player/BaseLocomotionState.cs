using UnityEngine;

public abstract class BaseLocomotionState : BaseState
{
    protected override void OnInitialize()
    {
        base.OnInitialize();

        GlobalEvents.Subscribe(GlobalEvent.Jump, (object[] args) =>
        {
            if (base.IsActiveState)
                base.TransitionTo<JumpState>();
        });
        GlobalEvents.Subscribe(GlobalEvent.Roll, (object[] args) =>
        {
            if (base.IsActiveState)
                base.TransitionTo<RollState>();
        });
    }
    public override void Tick()
    {
        base.Tick();

        if (!base.Actor.IsGrounded)
            base.TransitionTo<FallState>();
        if (Input.GetKeyDown(KeyCode.Mouse1))
            base.TransitionTo<AimState>();

        base.Actor.Raise(ActorEvent.SetActorTargetStance, Input.GetKey(KeyCode.C) ? Stance.Crouched : Stance.Standing);
    }
}
