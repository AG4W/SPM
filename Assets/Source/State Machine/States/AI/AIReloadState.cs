using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Reload")]
public class AIReloadState : AIBaseLocomotionState
{
    float timer;
    bool reloadWasComplete;

    public override void Enter()
    {
        base.Enter();

        base.Actor.Raise(ActorEvent.SetActorAnimatorLayer, AnimatorLayer.Reload, 1f);
        base.Actor.Raise(ActorEvent.SetActorAnimatorBool, "isReloading", true);
        base.Actor.Raise(ActorEvent.SetActorLeftHandWeight, 0f);
        base.Actor.Raise(ActorEvent.SetActorTargetStance, Stance.Crouched);

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
            base.TransitionTo<AILookForCover>();
        }

    }
    public override void Exit()
    {
        if (reloadWasComplete)
            base.Get<WeaponController>().Reload();

        base.Actor.Raise(ActorEvent.SetActorAnimatorLayer, AnimatorLayer.Reload, 0f);
        base.Actor.Raise(ActorEvent.SetActorAnimatorBool, "isReloading", false);
        base.Actor.Raise(ActorEvent.SetActorLeftHandWeight, 1f);
        base.Actor.Raise(ActorEvent.SetActorTargetStance, Stance.Standing);
    }
}
