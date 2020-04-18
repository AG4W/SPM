using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Aim")]
public class AimState : ActState
{
    public override void Enter()
    {
        base.Enter();

        base.Actor.Raise(ActorEvent.SetActorMovementMode, MovementMode.Walk);
        base.Actor.Raise(ActorEvent.SetActorTargetAimMode, AimMode.IronSight);

        GlobalEvents.Raise(GlobalEvent.SetCameraAimMode, AimMode.IronSight);
        GlobalEvents.Raise(GlobalEvent.SetCameraFOVMultiplier, ((WeaponController)base.Context["weapon"]).ZoomMultiplier);
    }
    public override void Tick()
    {
        base.Tick();

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            if (base.Actor.TargetInput.magnitude > .1f)
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
        base.Actor.Raise(ActorEvent.SetActorTargetAimMode, AimMode.Default); 
        GlobalEvents.Raise(GlobalEvent.SetCameraAimMode, AimMode.Default);
        GlobalEvents.Raise(GlobalEvent.SetCameraFOVMultiplier, 0f);
    }
}
