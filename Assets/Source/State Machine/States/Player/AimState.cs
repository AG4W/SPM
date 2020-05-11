using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Aim")]
public class AimState : ActState
{
    public override void Enter()
    {
        base.Enter();

        //base.Actor.Raise(ActorEvent.SetInputModifier, (float).5f);
        base.Actor.Raise(ActorEvent.SetTargetAimMode, AimMode.IronSight);
    }
    public override void Tick()
    {
        base.Tick();

        if (!Input.GetKey(KeyCode.Mouse1))
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
        base.Actor.Raise(ActorEvent.SetTargetAimMode, AimMode.Default); 
    }
}
