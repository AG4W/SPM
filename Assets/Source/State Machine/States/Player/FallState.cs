using UnityEngine;

[CreateAssetMenu(menuName = ("State/Player/Fall"))]
public class FallState : GroundLostState
{
    public override void Enter()
    {
        base.Enter();
    }
    public override void Tick()
    {
        base.Tick();

        if (base.DistanceToGround <= .4f)
            base.TransitionTo<LandState>();
    }
    public override void Exit()
    {
    }
}
