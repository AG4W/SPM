using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Reload")]
public class AIReloadState : AIBaseLocomotionState
{
    float timer;
    bool reloadWasComplete;

    public override void Enter()
    {
        base.Enter();

        base.Actor.Raise(ActorEvent.SetAnimatorLayer, AnimatorLayer.Reload, 1f);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 0f);

        if(base.Pawn.Mode == AICombatMode.Defensive)
            base.Actor.Raise(ActorEvent.SetTargetStance, Stance.Crouched);

        base.Animator.SetFloat("playspeedMultiplier", base.Animator.GetCurrentAnimatorClipInfo((int)AnimatorLayer.Reload)[0].clip.length / base.Get<WeaponController>().Weapon.ReloadTime);
        base.Animator.SetBool("isReloading", true);

        timer = 0f;
        reloadWasComplete = false;
    }
    public override void Tick()
    {
        base.Tick();

        timer += Time.deltaTime / Time.timeScale;

        if (timer >= base.Get<WeaponController>().Weapon.ReloadTime)
        {
            reloadWasComplete = true;

            if (base.Pawn.CanSeeTarget)
                base.TransitionTo<AIAttackState>();
            else
                base.TransitionTo<AISearchState>();
        }

        base.Actor.Raise(ActorEvent.SetTargetRotation, Quaternion.LookRotation(base.Actor.transform.position.DirectionTo(base.Pawn.LastKnownPositionOfTarget), Vector3.up));
    }
    public override void Exit()
    {
        if (reloadWasComplete)
            base.Actor.Raise(ActorEvent.ReloadWeapon);

        base.Animator.SetBool("isReloading", false);
        base.Actor.Raise(ActorEvent.SetAnimatorLayer, AnimatorLayer.Reload, 0f);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 1f);
        base.Actor.Raise(ActorEvent.SetTargetStance, Stance.Standing);
    }
}
