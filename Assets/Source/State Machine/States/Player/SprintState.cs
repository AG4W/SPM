﻿using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Sprint")]
public class SprintState : BaseLocomotionState
{
    public override void Enter()
    {
        base.Enter();
        base.Actor.Raise(ActorEvent.SetInputModifier, 2f);
    }
    public override void Tick()
    {
        base.Tick();

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            if (base.Actor.TargetInput.magnitude > .1f)
                base.TransitionTo<MoveState>();
            else
                base.TransitionTo<IdleState>();
        }
        if (Input.GetKeyDown(KeyCode.C))
            base.TransitionTo<MoveState>();
    }
    public override void Exit()
    {
        base.Actor.Raise(ActorEvent.SetInputModifier, 1f);
    }
}
