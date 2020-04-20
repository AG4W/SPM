using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Jump")]
public class JumpState : GroundLostState
{
    [SerializeField]float jumpInputModifier = .1f;
    [SerializeField]float jumpAcceleration = 7f;
    [SerializeField]float jumpDuration = 1f;
    [SerializeField]AnimationCurve jumpCurve;

    [SerializeField]float landThreshold = .75f;

    float jumpTimer;
    
    public override void Enter()
    {
        base.Enter();
        base.Actor.Raise(ActorEvent.SetActorAnimatorBool, "isJumping", true);

        GlobalEvents.Raise(GlobalEvent.SetCameraMode, CameraMode.Jump);

        jumpTimer = 0f;
    }
    public override void Tick()
    {
        base.Tick();

        jumpTimer += Time.deltaTime;

        if (jumpTimer >= jumpDuration)
        {
            Debug.Log(base.DistanceToGround);

            if (base.DistanceToGround <= landThreshold)
                base.TransitionTo<LandState>();
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
        base.Get<Animator>().SetBool("isJumping", false);
    }
}
