using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Land")]
public class LandState : BaseState
{
    public override void Enter()
    {
        base.Enter();
        base.Animator.SetBool("isLanding", true);
    }
    public override void Tick()
    {
        base.Tick();

        if (base.Actor.ActualInput.magnitude < .1f)
            base.TransitionTo<IdleState>();
        else
        {
            if (Input.GetKey(KeyCode.LeftShift))
                base.TransitionTo<SprintState>();
            else
                base.TransitionTo<MoveState>();
        }
    }
    public override void Exit()
    {
        base.Animator.SetBool("isLanding", false);
    }
}
