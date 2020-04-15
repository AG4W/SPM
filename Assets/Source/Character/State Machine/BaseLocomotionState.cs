using UnityEngine;

public abstract class BaseLocomotionState : BaseState
{
    protected override void OnInitialize()
    {
        base.OnInitialize();

        GlobalEvents.Subscribe(GlobalEvent.Jump, (object[] args) => {
            if (base.IsActiveState)
                base.TransitionTo<JumpState>();
        });
        GlobalEvents.Subscribe(GlobalEvent.Roll, (object[] args) => {
            if (base.IsActiveState)
                base.TransitionTo<RollState>();
        });
    }
    public override void Tick()
    {
        base.Tick();

        if (!base.Controller.IsGrounded)
            base.TransitionTo<FallState>();

        ((Animator)base.Context["animator"]).SetFloat("actualStance", base.Controller.ActualStance);

        GlobalEvents.Raise(GlobalEvent.SetTargetStance, Input.GetKey(KeyCode.C) ? Stance.Crouched : Stance.Standing);
    }
}
