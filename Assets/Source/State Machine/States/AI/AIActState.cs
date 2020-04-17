using UnityEngine;

public abstract class AIActState : AIBaseLocomotionState
{
    [Range(0f, 1f)][SerializeField]float total = 1f;
    [Range(0f, 1f)][SerializeField]float body = .25f;
    [Range(0f, 1f)][SerializeField]float head = 1f;
    [Range(0f, 1f)][SerializeField]float eyes = 1f;
    [Range(0f, 1f)][SerializeField]float clamp = .5f;

    protected override void OnInitialize()
    {
        base.OnInitialize();
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
            base.Actor.Raise(ActorEvent.SetActorLookAtWeights, new float[] { total, body, head, eyes, clamp });

            if (!((WeaponController)base.Context["weapon"]).CanFire || base.Actor.ActualInput.normalized.magnitude > 1f)
                return;

            Vector3 fp = base.Pawn.Target.FocusPoint.position;
            fp += Random.insideUnitSphere * (base.Pawn.Accuracy * 10f);

            base.Actor.Raise(ActorEvent.FireActorWeapon, fp);
        }
    }
    public override void Exit()
    {
    }
}
