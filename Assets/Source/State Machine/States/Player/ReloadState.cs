using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Reload")]
public class ReloadState : BaseState
{
    float timer;

    bool reloadWasComplete;

    public override void Enter()
    {
        base.Enter();

        base.Get<WeaponController>().PlayReloadSFX();

        base.Actor.Raise(ActorEvent.SetAnimatorLayer, AnimatorLayer.Reload, 1f);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 0f);

        base.Animator.SetBool("isReloading", true);
        base.Animator.SetFloat("playspeedMultiplier", base.Animator.GetCurrentAnimatorClipInfo((int)AnimatorLayer.Reload)[0].clip.length / base.Get<WeaponController>().Weapon.ReloadTime);

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

        if (Input.GetKeyDown(KeyCode.Space))
            base.TransitionTo<JumpState>();
        if (Input.GetKeyDown(KeyCode.V))
            base.TransitionTo<RollState>();

        base.Actor.Raise(ActorEvent.SetTargetStance, Input.GetKey(KeyCode.C) ? Stance.Crouched : Stance.Standing);

        //kolla reloadtimer
        //ifall vi bailar utan att completa reload pga roll/hopp så yeetar vi
    }
    public override void Exit()
    {
        if (reloadWasComplete)
            base.Actor.Raise(ActorEvent.ReloadWeapon);

        base.Animator.SetBool("isReloading", false);
        base.Actor.Raise(ActorEvent.SetAnimatorLayer, AnimatorLayer.Reload, 0f);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 1f);
    }
}
