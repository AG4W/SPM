using UnityEngine;

public abstract class ActState : BaseLocomotionState
{
    [Header("ActState settings")]
    [Range(0.01f, 1f)][SerializeField]float minimumForceRequiredToEnterForceState = .2f;
    [SerializeField]LayerMask aimMask;

    protected bool IsPerformingAction { get; set; }

    protected override void OnInitialize()
    {
        base.OnInitialize();
    }

    public override void Tick()
    {
        base.Tick();

        if (base.Actor.ActualInput.magnitude >= 1.4f || IsPerformingAction)
            return;

        //weapon
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (base.Get<WeaponController>().NeedsReload & base.Get<WeaponController>().Weapon != null)
                base.TransitionTo<ReloadState>();

            if (!base.Get<WeaponController>().CanFire)
                return;

            //skjut ifrån kamera
            Ray ray = base.Camera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask);

            base.Actor.Raise(ActorEvent.FireWeapon, hit.transform != null && hit.transform.root != base.Get<Actor>().transform ? hit.point : ray.GetPoint(300f), base.Actor.ActualInput.magnitude / 2f);

            //base.Actor.Raise(ActorEvent.SetActorAnimatorLayer, AnimatorLayer.Fire, 1f);
            //base.Actor.Raise(ActorEvent.SetActorAnimatorFloat, "recoil", 1f);
            //base.Actor.Raise(ActorEvent.SetActorAnimatorFloat, "playspeedMultiplier", base.Get<Animator>().GetCurrentAnimatorClipInfo((int)AnimatorLayer.Reload)[0].clip.length / base.Get<WeaponController>().Weapon.FireRate / 2f);

            GlobalEvents.Raise(GlobalEvent.ModifyCameraTraumaCapped, base.Get<WeaponController>().Weapon.TraumaValue);
        }
        if (Input.GetKeyDown(KeyCode.R) & base.Get<WeaponController>().Weapon != null)
            base.TransitionTo<ReloadState>();

        //abilities
        if(base.Player.Force.CurrentInPercent >= minimumForceRequiredToEnterForceState)
        {
            if (Input.GetKey(KeyCode.Alpha1))
                base.TransitionTo<PushState>();
            if (Input.GetKey(KeyCode.Alpha2))
                base.TransitionTo<PullState>();
            if (Input.GetKey(KeyCode.Alpha3))
                base.TransitionTo<TimeDilationState>();
        }
    }
}
