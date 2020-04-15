using UnityEngine;

public class BaseLocomotionState : BaseState
{
    public override void Enter()
    {
        base.Enter();
    }
    public override void Tick()
    {
        base.Tick();
        
        ((Animator)base.Context["animator"]).SetFloat("actualStance", base.Controller.ActualStance);

        GlobalEvents.Raise(GlobalEvent.SetTargetStance, Input.GetKey(KeyCode.C) ? Stance.Crouched : Stance.Standing);
    }
    public override void Exit()
    {
        base.Exit();
    }
}
