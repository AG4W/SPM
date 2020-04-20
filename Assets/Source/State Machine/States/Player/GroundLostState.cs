using UnityEngine;

public abstract class GroundLostState : BaseState
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
        base.Get<Animator>().SetFloat("distanceFromGround", this.DistanceToGround);

        if (velocityBeforeLosingGroundContact.magnitude > .01f)
            velocityBeforeLosingGroundContact = Vector3.Lerp(velocityBeforeLosingGroundContact, Vector3.zero, fallMomentumDeceleration * (Time.deltaTime / Time.timeScale));
        else
            velocityBeforeLosingGroundContact = Vector3.zero;

        base.Actor.Raise(ActorEvent.ModifyActorVelocity, velocityBeforeLosingGroundContact);
    }
}
