﻿using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Roll")]
public class RollState : BaseState
{
    public override void Enter()
    {
        base.Enter();
        base.Animator.SetBool("isRolling", true);
    }
    public override void Tick()
    {
        base.Tick();

        //if (!base.Actor.IsGrounded)
        //    base.TransitionTo<FallState>();

        if (!base.Animator.GetBool("isRolling"))
        {
            if (base.Actor.TargetInput.magnitude > .1f)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    base.TransitionTo<SprintState>();
                else
                    base.TransitionTo<MoveState>();
            }
            else
                base.TransitionTo<IdleState>();
        }
    }
    public override void Exit()
    {
    }
}
