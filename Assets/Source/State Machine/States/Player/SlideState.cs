using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Slide")]
public class SlideState : BaseState
{
    [SerializeField]AnimationCurve curve;

    [SerializeField]float minSlideTime = 1f;
    [SerializeField]float minimumSpeedThreshold = 5f;

    float timer;
    Vector3 velocityOnEnter;

    public override void Enter()
    {
        base.Enter();
        base.Actor.Raise(ActorEvent.SetAnimatorBool, "isSliding", true);

        velocityOnEnter = base.Actor.Velocity;
        timer = 0f;
    }
    public override void Tick()
    {
        base.Tick();

        if (!base.Actor.IsGrounded)
            base.TransitionTo<FallState>();

        timer += Time.deltaTime / Time.timeScale;
        base.Actor.Raise(ActorEvent.ModifyVelocity, velocityOnEnter * curve.Evaluate(timer));

        if (base.Actor.Velocity.magnitude <= minimumSpeedThreshold || (!Input.GetKey(KeyCode.Space) && timer >= minSlideTime))
        {
            if (base.Actor.TargetInput.magnitude > .1f)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    base.TransitionTo<SprintState>();
                else
                    base.TransitionTo<MoveState>();
            }
            else
                base.TransitionTo<IdleState>();
        }
    }
    public override void Exit()
    {
        base.Actor.Raise(ActorEvent.SetAnimatorBool, "isSliding", false);
    }
}