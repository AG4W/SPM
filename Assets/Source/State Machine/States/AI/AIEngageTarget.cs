using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Engage Target")]
public class AIEngageTarget : AIBaseLocomotionState
{
    [SerializeField]AIFireMode mode = AIFireMode.Default;

    int shotsFired;

    public override void Enter()
    {
        base.Enter();

        shotsFired = 0;
    }
    public override void Tick()
    {
        base.Tick();

        if (shotsFired >= (base.Get<WeaponController>().Weapon.ClipSize / (int)mode))
            base.TransitionTo<AILookForCover>();
        if (base.Pawn.CanSeeTarget)
        {
            if (base.Get<WeaponController>().NeedsReload)
                base.TransitionTo<AIReloadState>();
            if (!base.Get<WeaponController>().CanFire)
                return;

            base.Get<WeaponController>().Fire(base.Pawn.Target.FocusPoint.position, base.Actor.ActualInput.magnitude);
            shotsFired++;
        }
    }
    public override void Exit()
    {
    }
}
public enum AIFireMode
{
    Suppression,
    Default,
    Conservative
}
