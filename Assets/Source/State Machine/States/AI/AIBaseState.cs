using UnityEngine;

public abstract class AIBaseState : State
{
    [SerializeField]float gravitationalConstant = 9.82f;

    protected HumanoidPawn Pawn { get { return (HumanoidPawn)base.Actor; } }

    protected override void OnInitialize()
    {
        base.OnInitialize();
    }
    public override void Tick()
    {
        // Ground Check
        //base.Actor.Raise(ActorEvent.UpdateGroundedStatus);
        //Gravity
        base.Actor.Raise(ActorEvent.ModifyVelocity, Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale));
        base.Actor.Raise(ActorEvent.SetAnimatorFloat, "targetMagnitude", base.Actor.TargetInput.magnitude);
    }
}
