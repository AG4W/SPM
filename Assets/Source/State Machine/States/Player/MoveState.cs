using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Move")]
public class MoveState : ActState
{
    public override void Tick()
    {
        base.Tick();
        base.Actor.Raise(ActorEvent.SetInputModifier, Input.GetKey(KeyCode.CapsLock) ? .5f : 1f);

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
