using UnityEngine;

public abstract class AIBaseState : State
{
    [SerializeField]float gravitationalConstant = 9.82f;

    protected Animator Animator { get; private set; }

    protected HumanoidPawn Pawn { get { return (HumanoidPawn)base.Actor; } }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        this.Animator = base.Get<Animator>();
    }
    public override void Tick()
    {
        // Ground Check
        //base.Actor.Raise(ActorEvent.UpdateGroundedStatus);
        //Gravity
        base.Actor.Raise(ActorEvent.ModifyVelocity, Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale));
        this.Animator.SetFloat("targetMagnitude", base.Actor.TargetInput.magnitude);
    }
}
