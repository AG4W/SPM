using UnityEngine;

public abstract class AIBaseLocomotionState : AIBaseState
{
    [Range(0f, 1f)][SerializeField]float total = 1f;
    [Range(0f, 1f)][SerializeField]float body = .25f;
    [Range(0f, 1f)][SerializeField]float head = 1f;
    [Range(0f, 1f)][SerializeField]float eyes = 1f;
    [Range(0f, 1f)][SerializeField]float clamp = 1f;

    float[] weights;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        weights = new float[] { total, body, head, eyes, clamp };
    }
    public override void Tick()
    {
        base.Tick();

        base.Actor.Raise(ActorEvent.UpdateAITargetStatus);
        base.Actor.Raise(ActorEvent.SetActorLookAtPosition, base.Pawn.CanSeeTarget ? base.Pawn.Target.FocusPoint.position : (base.Pawn.transform.position + Vector3.up * 1.5f) + base.Pawn.transform.forward);
        base.Actor.Raise(ActorEvent.SetActorLookAtWeights, weights);

        if (base.Actor.ActualInput.magnitude > 1f)
            return;

        if (base.Get<WeaponController>().NeedsReload)
            base.TransitionTo<AIReloadState>();
        if (!base.Get<WeaponController>().CanFire)
            return;
        if (base.Pawn.CanSeeTarget)
            base.Get<WeaponController>().Fire(base.Pawn.Target.FocusPoint.position, base.Actor.ActualInput.magnitude);
    }
}
