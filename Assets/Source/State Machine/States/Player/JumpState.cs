using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Jump")]
public class JumpState : GroundLostState
{
    [SerializeField]float jumpInputModifier = .1f;
    [SerializeField]float jumpAcceleration = 7f;
    [SerializeField]float jumpDuration = 1f;
    [SerializeField]AnimationCurve jumpCurve;

    [SerializeField]float landThreshold = .75f;
    [SerializeField]float feetOffset = .9f;
    [SerializeField]float heightOffset = .4f;

    float jumpTimer;
    
    public override void Enter()
    {
        base.Enter();
        base.Animator.SetBool("isJumping", true);

        jumpTimer = 0f;
    }
    public override void Tick()
    {
        base.Tick();

        jumpTimer += Time.deltaTime;

        // For collisionbox
        UpdateFeetOffset();
        UpdateHeightOffset();

        if (jumpTimer >= jumpDuration)
        {
            //if (base.DistanceToGround <= landThreshold)
                base.TransitionTo<LandState>();
            //else
             //   base.TransitionTo<FallState>();
        }

        Vector3 velocity = Vector3.zero;
        velocity += Vector3.up * jumpCurve.Evaluate(jumpTimer) * jumpAcceleration * (Time.deltaTime / Time.timeScale);

        //lägg till väldigt lite styrfart för spelaren i luften
        velocity += ((base.Actor.transform.right * base.Actor.TargetInput.x) + (base.Actor.transform.forward * base.Actor.TargetInput.z)) * (jumpInputModifier * (jumpDuration - jumpTimer)); //minska inputpåverkan längre in i hoppet
        
        //velocity += velocityBeforeLosingGroundContact / fallForwardVelocityDivider;
        velocity *= Mathf.Pow(base.AirResistance, (Time.deltaTime / Time.timeScale));

        base.Actor.Raise(ActorEvent.ModifyVelocity, velocity);
    }

    ////// Offsets needed for collisionbox-adjustments during jumpmotion for correct collision checks
    void UpdateFeetOffset()
    {
        if (jumpTimer < jumpDuration / 2f)
            base.Actor.SetCollisionLowPoint(Mathf.Lerp(0f, feetOffset, jumpTimer * 1.2f));
        if (jumpTimer >= jumpDuration / 2f)
            base.Actor.SetCollisionLowPoint(Mathf.Lerp(feetOffset, 0f, jumpTimer * 0.8f));
    }
    void UpdateHeightOffset() 
    {
        if (jumpTimer < jumpDuration / 2f)
            base.Actor.SetCollisionHighPoint(Mathf.Lerp(0f, heightOffset, jumpTimer * 1.2f));
        if (jumpTimer >= jumpDuration / 2f)
            base.Actor.SetCollisionHighPoint(Mathf.Lerp(heightOffset, 0f, jumpTimer * 0.8f));
    }

    public override void Exit()
    {
        base.Actor.SetCollisionLowPoint(0f);
        base.Actor.SetCollisionHighPoint(0f);

        base.Animator.SetBool("isJumping", false);
    }
}
