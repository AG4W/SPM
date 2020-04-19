using UnityEngine;

public abstract class AIActState : AIBaseLocomotionState
{
    [Range(0f, 1f)][SerializeField]float total = 1f;
    [Range(0f, 1f)][SerializeField]float body = .25f;
    [Range(0f, 1f)][SerializeField]float head = 1f;
    [Range(0f, 1f)][SerializeField]float eyes = 1f;
    [Range(0f, 1f)][SerializeField]float clamp = .5f;

    float[] weights;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        weights = new float[] { total, body, head, eyes, clamp };
    }
    public override void Enter()
    {
        base.Enter();
        //look for target
    }
    public override void Tick()
    {
        base.Tick();
        base.Actor.Raise(ActorEvent.UpdateAITargetStatus);
        base.Actor.Raise(ActorEvent.SetActorTargetAimMode, base.Pawn.CanSeeTarget ? AimMode.IronSight : AimMode.Default);

        if (base.Pawn.CanSeeTarget)
        {
            base.Actor.Raise(ActorEvent.SetActorTargetRotation, base.Pawn.HeadingToTarget.normalized);

            base.Actor.Raise(ActorEvent.SetActorLookAtPosition, base.Pawn.Target.FocusPoint.position);
            base.Actor.Raise(ActorEvent.SetActorLookAtWeights, weights);

            if (base.Get<WeaponController>().NeedsReload)
                base.Get<WeaponController>().Reload();

            if (!base.Get<WeaponController>().CanFire || base.Actor.ActualInput.normalized.magnitude > 1f)
                return;

            base.Get<WeaponController>().Fire(base.Pawn.Target.FocusPoint.position + (Random.insideUnitSphere * base.Pawn.Accuracy), base.Actor.ActualInput.magnitude);
        }
    }
    public override void Exit()
    {
    }
}
