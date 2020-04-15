using UnityEngine;

[CreateAssetMenu(menuName = "State/Idle")]
public class IdleState : BaseState
{
    public override void Initialize()
    {
        base.Initialize();
        GlobalEvents.Subscribe(GlobalEvent.Jump, (object[] args) => base.TransitionTo<JumpState>());
    }
    public override void Enter() 
    {
        base.Enter();
    }
    public override void Tick()
    {
        base.Tick();

        GlobalEvents.Raise(GlobalEvent.UpdatePlayerRotation);

        if (base.Controller.TargetInput.magnitude > .1f)
            base.TransitionTo<MoveState>();
    }
    public override void Exit() 
    {
        base.Exit();
    }
}
