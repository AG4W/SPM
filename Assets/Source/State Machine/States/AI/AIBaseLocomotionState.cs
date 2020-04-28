using UnityEngine;
using UnityEngine.AI;

public abstract class AIBaseLocomotionState : AIBaseState
{
    [Range(0f, 1f)][SerializeField]float total = 1f;
    [Range(0f, 1f)][SerializeField]float body = .25f;
    [Range(0f, 1f)][SerializeField]float head = 1f;
    [Range(0f, 1f)][SerializeField]float eyes = 1f;
    [Range(0f, 1f)][SerializeField]float clamp = 1f;

    protected float[] Weights { get; private set; }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Weights = new float[] { total, body, head, eyes, clamp };
    }
    public override void Tick()
    {
        base.Tick();

        base.Actor.Raise(ActorEvent.UpdateAITargetStatus);

        base.Actor.Raise(ActorEvent.SetTargetInput, base.Get<NavMeshAgent>().steeringTarget.ToInput(base.Actor.transform).normalized);

        if (base.Actor.ActualInput.magnitude > 1f)
            return;

        //if (base.Get<WeaponController>().NeedsReload)
        //    base.TransitionTo<AIReloadState>();
        //if (!base.Get<WeaponController>().CanFire)
        //    return;
        //if (base.Pawn.CanSeeTarget)
        //    base.Get<WeaponController>().Fire(base.Pawn.Target.FocusPoint.position, base.Actor.ActualInput.magnitude);
    }
}
