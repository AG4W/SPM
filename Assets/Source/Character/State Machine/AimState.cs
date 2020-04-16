using UnityEngine;

[CreateAssetMenu(menuName = "State/Aim")]
public class AimState : ActState
{
    public override void Enter()
    {
        base.Enter();

        GlobalEvents.Raise(GlobalEvent.SetMovementMode, MovementMode.Walk);
        GlobalEvents.Raise(GlobalEvent.SetTargetAimMode, AimMode.IronSight);
    }
    public override void Tick()
    {
        base.Tick();

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            if (base.Controller.TargetInput.magnitude > .1f)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift))
                    base.TransitionTo<SprintState>();
                else
                    base.TransitionTo<MoveState>();
            }
            else
                base.TransitionTo<IdleState>();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
            base.TransitionTo<SprintState>();
    }
    public override void Exit()
    {
        GlobalEvents.Raise(GlobalEvent.SetTargetAimMode, AimMode.Default);
    }
}
