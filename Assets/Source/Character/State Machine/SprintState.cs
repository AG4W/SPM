using UnityEngine;

[CreateAssetMenu(menuName = "State/Sprint")]
public class SprintState : BaseLocomotionState
{
    public override void Enter()
    {
        base.Enter();
        GlobalEvents.Raise(GlobalEvent.SetMovementMode, MovementMode.Sprint);
    }
    public override void Tick()
    {
        base.Tick();

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (base.Controller.TargetInput.magnitude > .1f)
                base.TransitionTo<MoveState>();
            else
                base.TransitionTo<IdleState>();
        }
        if (Input.GetKeyDown(KeyCode.C))
            base.TransitionTo<MoveState>();
    }
    public override void Exit()
    {
        GlobalEvents.Raise(GlobalEvent.SetMovementMode, MovementMode.Jog);
    }
}
