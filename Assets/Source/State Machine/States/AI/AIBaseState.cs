using UnityEngine;

public abstract class AIBaseState : State
{
    [SerializeField]float gravitationalConstant = 9.82f;

    protected HumanoidPawn Pawn { get { return (HumanoidPawn)base.Actor; } }

    public override void Tick()
    {
        // Ground Check
        base.Actor.Raise(ActorEvent.UpdateActorGroundedStatus);
        //Gravity
        base.Actor.Raise(ActorEvent.ModifyActorVelocity, Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale));
    }
}
