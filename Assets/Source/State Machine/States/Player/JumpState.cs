using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Jump")]
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

        base.Actor.IsGrounded = false;
        jumpTimer = 0f;
    }
    public override void Tick()
    {
        base.Tick();

        jumpTimer += Time.deltaTime;

        if (jumpTimer >= jumpDuration)
        {
            if (base.Actor.IsGrounded)
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
            else
                base.TransitionTo<FallState>();
        }

        Vector3 velocity = Vector3.zero;
        velocity += Vector3.up * jumpCurve.Evaluate(jumpTimer) * jumpAcceleration * (Time.deltaTime / Time.timeScale);
        //lägg till väldigt lite styrfart för spelaren i luften

        velocity += ((base.Actor.transform.right * base.Actor.TargetInput.x) + (base.Actor.transform.forward * base.Actor.TargetInput.z)) * (jumpInputModifier * (jumpDuration - jumpTimer)); //minska inputpåverkan längre in i hoppet
        //velocity += velocityBeforeLosingGroundContact / fallForwardVelocityDivider;
        velocity *= Mathf.Pow(base.AirResistance, (Time.deltaTime / Time.timeScale));

        base.Actor.Raise(ActorEvent.ModifyActorVelocity, velocity);
    }
    public override void Exit()
    {
        base.Exit();
        ((Animator)base.Context["animator"]).SetBool("isJumping", false);
    }
}
