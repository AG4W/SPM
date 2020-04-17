using UnityEngine;

[CreateAssetMenu(menuName = "State/Fall")]

public class FallState : BaseState
{
    [SerializeField]Vector3 velocityBeforeLosingGroundContact;

    [SerializeField]float fallMomentumDeceleration = .5f;

    public override void Enter()
    {
        base.Enter();

        velocityBeforeLosingGroundContact = base.Actor.Velocity;
        velocityBeforeLosingGroundContact.y = 0f;
    }
    public override void Tick()
    {
        base.Tick();

        if (velocityBeforeLosingGroundContact.magnitude > .01f)
            velocityBeforeLosingGroundContact = Vector3.Lerp(velocityBeforeLosingGroundContact, Vector3.zero, fallMomentumDeceleration * (Time.deltaTime / Time.timeScale));
        else
            velocityBeforeLosingGroundContact = Vector3.zero;

        GlobalEvents.Raise(GlobalEvent.ModifyActorVelocity, velocityBeforeLosingGroundContact);
        
        if (((Animator)base.Context["animator"]).GetBool("isJumping") == false && base.Actor.IsGrounded)
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
    }
}
