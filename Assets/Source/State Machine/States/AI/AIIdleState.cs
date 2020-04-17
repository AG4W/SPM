using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Idle")]
public class AIIdleState : AIActState
{
    public override void Enter()
    {
        base.Enter();
        base.Actor.Raise(ActorEvent.SetActorTargetStance, Stance.Standing);
    }

    public override void Tick()
    {
        base.Tick();
    }
}
