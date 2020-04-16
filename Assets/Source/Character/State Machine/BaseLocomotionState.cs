﻿using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.Mouse1))
            base.TransitionTo<AimState>();

        GlobalEvents.Raise(GlobalEvent.SetTargetStance, Input.GetKey(KeyCode.C) ? Stance.Crouched : Stance.Standing);
    }
}
