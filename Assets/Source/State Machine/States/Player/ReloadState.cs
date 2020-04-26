using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Reload")]
public class ReloadState : BaseState
{
    float timer;

    bool reloadWasComplete;

    public override void Enter()
    {
        base.Enter();

        base.Actor.Raise(ActorEvent.SetActorAnimatorLayer, AnimatorLayer.Reload, 1f);
        base.Actor.Raise(ActorEvent.SetActorLeftHandWeight, 0f);

        base.Actor.Raise(ActorEvent.SetActorAnimatorFloat, "playspeedMultiplier", base.Get<Animator>().GetCurrentAnimatorClipInfo((int)AnimatorLayer.Reload)[0].clip.length / base.Get<WeaponController>().Weapon.ReloadTime);
        base.Actor.Raise(ActorEvent.SetActorAnimatorBool, "isReloading", true);

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
            base.Return();
        }

        if (!base.Actor.IsGrounded)
            base.TransitionTo<FallState>();
        if (Input.GetKeyDown(KeyCode.Space))
            base.TransitionTo<JumpState>();
        if (Input.GetKeyDown(KeyCode.V))
            base.TransitionTo<RollState>();

        base.Actor.Raise(ActorEvent.SetActorTargetStance, Input.GetKey(KeyCode.C) ? Stance.Crouched : Stance.Standing);

        //kolla reloadtimer
        //ifall vi bailar utan att completa reload pga roll/hopp så yeetar vi
    }
    public override void Exit()
    {
        if (reloadWasComplete)
            base.Get<WeaponController>().Reload();

        base.Actor.Raise(ActorEvent.SetActorAnimatorBool, "isReloading", false);
        base.Actor.Raise(ActorEvent.SetActorAnimatorLayer, AnimatorLayer.Reload, 0f);
        base.Actor.Raise(ActorEvent.SetActorLeftHandWeight, 1f);
    }
}
