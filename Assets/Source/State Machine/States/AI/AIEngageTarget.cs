using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Engage Target")]
public class AIEngageTarget : AIBaseLocomotionState
{
    public override void Enter()
    {
        base.Enter();
    }
    public override void Tick()
    {
        base.Tick();

        if (base.Get<WeaponController>().NeedsReload)
            base.TransitionTo<AIReloadState>();
        if (!base.Get<WeaponController>().CanFire)
            return;
        if (base.Pawn.CanSeeTarget)
            base.Get<WeaponController>().Fire(base.Pawn.Target.FocusPoint.position, base.Actor.ActualInput.magnitude);
    }
    public override void Exit()
    {
    }
}