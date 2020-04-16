using UnityEngine;

[CreateAssetMenu(menuName = "State/Idle")]
public class IdleState : ActState
{
    public override void Tick()
    {
        base.Tick();

        if (base.Controller.TargetInput.magnitude > .1f)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                base.TransitionTo<SprintState>();
            else
                base.TransitionTo<MoveState>();
        }
    }
    public override void Exit()
    {
    }
}
