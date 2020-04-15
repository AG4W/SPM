using UnityEngine;

[CreateAssetMenu(menuName = "State/Move")]
public class MoveState : ActState
{
    public override void Tick()
    {
        base.Tick();
        GlobalEvents.Raise(GlobalEvent.SetMovementMode, Input.GetKey(KeyCode.CapsLock) ? MovementMode.Walk : MovementMode.Jog);

        if (base.Controller.TargetInput.magnitude < .1f)
            base.TransitionTo<IdleState>();
        if (Input.GetKeyDown(KeyCode.LeftShift))
            base.TransitionTo<SprintState>();
    }
    public override void Exit()
    {
    }
}
