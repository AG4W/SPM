using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Idle")]
public class AIIdleState : AIBaseLocomotionState
{
    public override void Enter()
    {
        base.Enter();

        base.Actor.Raise(ActorEvent.SetActorTargetStance, Stance.Standing);
        base.Actor.Raise(ActorEvent.SetActorTargetPosition, base.Actor.transform.position);
        base.Actor.Raise(ActorEvent.SetActorTargetInput, Vector3.zero);

        base.Get<Animator>().SetBool("isAlert", true);
    }
    public override void Tick()
    {
        base.Tick();

        if (base.Pawn.CanSeeTarget)
        {
            base.Actor.Raise(ActorEvent.UpdateActorAlertStatus, true);
            base.TransitionTo<AILookForCover>();
        }
    }
    public override void Exit()
    {
    }
}
