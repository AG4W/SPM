using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Search")]
public class AISearchState : AIBaseLocomotionState
{
    [SerializeField]float randomOffset = 5f;

    float lookAtTimer;

    Vector3 lookAt;
    Vector3 position = Vector3.zero;

    public override void Enter()
    {
        base.Enter();

        lookAt = (base.Actor.transform.forward * Random.Range(5f, 10f)) + (base.Actor.transform.up * Random.Range(1f, 4f)) + (base.Actor.transform.right * Random.Range(-5f, 5f));

        SetInputModifier();

        base.Pawn.SetLookAtPosition(base.Actor.transform.position + lookAt);
        base.Pawn.SetLookAtWeights(base.Weights);
    }
    public override void Tick()
    {
        base.Tick();

        if (base.Pawn.CanSeeTarget)
            base.TransitionTo<AIMoveToCoverState>();

        lookAtTimer += Time.deltaTime;

        if(lookAtTimer >= 3f)
        {
            lookAt = (base.Actor.transform.forward * Random.Range(5f, 10f)) + (base.Actor.transform.up * Random.Range(1f, 4f)) + (base.Actor.transform.right * Random.Range(-5f, 5f));
            base.Pawn.SetLookAtPosition(base.Actor.transform.position + lookAt);
            lookAtTimer = 0f;
        }

        if (position == Vector3.zero || base.Actor.transform.position.DistanceTo(position) < 2f)
        {
            position = base.Pawn.Target.transform.position + new Vector3(Random.Range(-randomOffset, randomOffset), 0f, Random.Range(-randomOffset, randomOffset));

            base.Actor.Raise(ActorEvent.SetTargetPosition, position);
            base.Actor.Raise(ActorEvent.SetTargetRotation, Quaternion.LookRotation(base.Actor.transform.position.DirectionTo(position), Vector3.up));
        }

        //base.Actor.Raise(ActorEvent.SetTargetRotation, Quaternion.LookRotation(position.ToInput(base.Pawn.transform).normalized, Vector3.up));
    }
    public override void Exit()
    {
        base.Actor.Raise(ActorEvent.SetInputModifier, 1f);
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
                base.Actor.Raise(ActorEvent.SetInputModifier, .5f);
                break;
            default:
                base.Actor.Raise(ActorEvent.SetInputModifier, .5f);
                break;
        }
    }
}
