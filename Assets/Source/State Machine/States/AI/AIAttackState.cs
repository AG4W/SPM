using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Attack")]
public class AIAttackState : AIBaseLocomotionState
{
    //tid innan AIn går till hunt-state
    //kombineras med AIns combatmode för att få lite procedurellt beteende
    [SerializeField]float baseMehTime = 4f;
    //250ms är peak human average
    [SerializeField]float reactionSpeed = .25f;

    float timeSinceTargetLastSeen = 0f;
    float reactionTimer = 0f;

    Vector3 lookAt;

    public override void Enter()
    {
        base.Enter();

        timeSinceTargetLastSeen = 0f;
        reactionTimer = 0f;

        SetInputModifier();

        if (base.Pawn.Mode == AICombatMode.Defensive)
            base.Actor.Raise(ActorEvent.SetTargetStance, Stance.Crouched);

        base.Actor.Raise(ActorEvent.SetTargetAimMode, AimMode.IronSight);
        base.Actor.Raise(ActorEvent.SetLookAtWeights, base.Weights);
    }
    public override void Tick()
    {
        base.Tick();

        reactionTimer += Time.deltaTime;

        if (!base.Pawn.CanSeeTarget)
        {
            reactionTimer = 0f;
            timeSinceTargetLastSeen += Time.deltaTime;

            if (timeSinceTargetLastSeen >= baseMehTime * (int)base.Pawn.Mode)
                base.TransitionTo<AISearchState>();
        }
        else
        {
            timeSinceTargetLastSeen = 0f;
            lookAt = base.Pawn.Target.FocusPoint.position;

            base.Actor.Raise(ActorEvent.SetLastKnownPositionOfTarget, base.Pawn.Target.transform.position);
        }

        SetTargetPosition();

        base.Actor.Raise(ActorEvent.SetLookAtPosition, lookAt);
        base.Actor.Raise(ActorEvent.SetTargetRotation, Quaternion.LookRotation(base.Actor.transform.position.DirectionTo(base.Pawn.LastKnownPositionOfTarget), Vector3.up));

        if (base.Get<WeaponController>().NeedsReload)
            base.TransitionTo<AIReloadState>();
        if (base.Get<WeaponController>().CanFire && reactionTimer >= reactionSpeed)
            base.Get<WeaponController>().Fire(base.Pawn.CanSeeTarget ? base.Pawn.Target.FocusPoint.position : base.Pawn.LastKnownPositionOfTarget + Vector3.up * 1.5f, base.Actor.ActualInput.magnitude / 2f);
    }
    public override void Exit()
    {
        base.Actor.Raise(ActorEvent.SetTargetStance, Stance.Standing);
        base.Actor.Raise(ActorEvent.SetTargetAimMode, AimMode.Default);
    }

    void SetInputModifier()
    {
        switch (base.Pawn.Mode)
        {
            case AICombatMode.Aggressive:
                base.Actor.Raise(ActorEvent.SetInputModifier, 1f);
                break;
            case AICombatMode.Cautious:
                base.Actor.Raise(ActorEvent.SetInputModifier, .5f);
                break;
            case AICombatMode.Defensive:
                base.Actor.Raise(ActorEvent.SetInputModifier, 0f);
                break;
            default:
                base.Actor.Raise(ActorEvent.SetInputModifier, .5f);
                break;
        }
    }
    void SetTargetPosition()
    {
        switch (base.Pawn.Mode)
        {
            case AICombatMode.Aggressive:
                base.Actor.Raise(ActorEvent.SetTargetPosition, Vector3.Lerp(base.Actor.transform.position, base.Pawn.LastKnownPositionOfTarget, .25f));
                break;
            case AICombatMode.Cautious:
                base.Actor.Raise(ActorEvent.SetTargetPosition, Vector3.Lerp(base.Actor.transform.position, base.Pawn.LastKnownPositionOfTarget, .75f));
                break;
            case AICombatMode.Defensive:
                base.Actor.Raise(ActorEvent.SetTargetPosition, base.Actor.transform.position);
                break;
            default:
                base.Actor.Raise(ActorEvent.SetTargetPosition, base.Actor.transform.position);
                break;
        }
    }
}

