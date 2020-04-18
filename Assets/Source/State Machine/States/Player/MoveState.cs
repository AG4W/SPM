using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Move")]
public class MoveState : ActState
{
    public override void Tick()
    {
        base.Tick();
        base.Actor.Raise(ActorEvent.SetActorMovementMode, Input.GetKey(KeyCode.CapsLock) ? MovementMode.Walk : MovementMode.Jog);

        if (base.Actor.TargetInput.magnitude < .1f)
            base.TransitionTo<IdleState>();
        if (Input.GetKey(KeyCode.LeftShift))
            base.TransitionTo<SprintState>();
        if (Input.GetKey(KeyCode.Mouse1))
            base.TransitionTo<AimState>();
    }
    public override void Exit()
    {
    }
}
