using UnityEngine;

[CreateAssetMenu(menuName = "State/Idle")]
public class IdleState : BaseLocomotionState
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        GlobalEvents.Subscribe(GlobalEvent.Jump, (object[] args) => base.TransitionTo<JumpState>());
    }
    public override void Enter() 
    {
        base.Enter();
    }
    public override void Tick()
    {
        base.Tick();

        if (base.Controller.TargetInput.magnitude > .1f)
            base.TransitionTo<MoveState>();
    }
    public override void Exit() 
    {
        base.Exit();
    }
}
