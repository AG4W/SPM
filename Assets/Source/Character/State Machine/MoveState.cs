using UnityEngine;

[CreateAssetMenu(menuName = "State/Move")]
public class MoveState : ActState
{
    public override void Tick()
    {
        base.Tick();
        GlobalEvents.Raise(GlobalEvent.SetActorMovementMode, Input.GetKey(KeyCode.CapsLock) ? MovementMode.Walk : MovementMode.Jog);

        if (base.Actor.TargetInput.magnitude < .1f)
            base.TransitionTo<IdleState>();
        if (Input.GetKeyDown(KeyCode.LeftShift))
            base.TransitionTo<SprintState>();
        if (Input.GetKeyDown(KeyCode.Mouse1))
            base.TransitionTo<AimState>();
    }
    public override void Exit()
    {
    }
}
