using UnityEngine;

[CreateAssetMenu(menuName = "State/Move")]
public class MoveState : BaseLocomotionState
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
        GlobalEvents.Raise(GlobalEvent.SetMovementSpeed, Input.GetKey(KeyCode.LeftShift) ? MovementMode.Sprint : (Input.GetKey(KeyCode.CapsLock) ? MovementMode.Walk : MovementMode.Jog));

        if (base.Controller.TargetInput.magnitude < .1f)
            base.TransitionTo<IdleState>();
        if (!base.Controller.IsGrounded)
            base.TransitionTo<FallState>();
    }
    public override void Exit()
    {
        base.Exit();
    }
}
