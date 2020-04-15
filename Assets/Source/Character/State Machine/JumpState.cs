using UnityEngine;

[CreateAssetMenu(menuName = "State/Jump")]
public class JumpState : FallState
{
    [SerializeField]float jumpInputModifier = .1f;

    [SerializeField]float jumpAcceleration = 7f;
    [SerializeField]AnimationCurve jumpCurve;

    [SerializeField]float jumpDuration = 1f;
    float jumpTimer;

    public override void Enter()
    {
        base.Enter();
        ((Animator)base.Context["animator"]).SetBool("isJumping", true);
        base.Controller.IsGrounded = false;
        jumpTimer = 0f;
    }
    public override void Tick()
    {
        base.Tick();

        jumpTimer += Time.deltaTime;

        if (jumpTimer >= jumpDuration)
        {
            if (base.Controller.IsGrounded)
            {
                if (base.Controller.TargetInput.magnitude > .1f)
                    base.TransitionTo<MoveState>();
                else
                    base.TransitionTo<IdleState>();
            }
            else
                base.TransitionTo<FallState>();
        }

        Vector3 velocity = Vector3.zero;
        velocity += Vector3.up * jumpCurve.Evaluate(jumpTimer) * (base.GravitationalConstant + jumpAcceleration) * (Time.deltaTime / Time.timeScale);
        //lägg till väldigt lite styrfart för spelaren i luften
        velocity += ((base.Controller.transform.right * base.Controller.TargetInput.x) + (base.Controller.transform.forward * base.Controller.TargetInput.z)) * jumpInputModifier;
        //velocity += velocityBeforeLosingGroundContact / fallForwardVelocityDivider;
        velocity *= Mathf.Pow(base.AirResistance, (Time.deltaTime / Time.timeScale));

        GlobalEvents.Raise(GlobalEvent.ModifyPlayerVelocity, velocity);
    }
    public override void Exit()
    {
        base.Exit();
        ((Animator)base.Context["animator"]).SetBool("isJumping", false);
    }
}
