﻿using UnityEngine;

public abstract class ActState : BaseLocomotionState
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
    }

    public override void Tick()
    {
        base.Tick();

        if (base.Actor.ActualInput.normalized.magnitude > 1f)
            return;

        //weapon
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (((WeaponController)base.Context["weapon"]).NeedsReload && !((WeaponController)base.Context["weapon"]).IsReloading)
                base.TransitionTo<ReloadState>();

            if (!((WeaponController)base.Context["weapon"]).CanFire)
                return;

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

            base.Actor.Raise(ActorEvent.FireActorWeapon, hit.transform != null ? hit.point : ray.GetPoint(300f), base.Actor.ActualInput.magnitude);
        }
        if (Input.GetKeyDown(KeyCode.R))
            base.TransitionTo<ReloadState>();

        //abilities
        if (Input.GetKey(KeyCode.Alpha1))
            base.TransitionTo<PushState>();
        if (Input.GetKey(KeyCode.Alpha2))
            base.TransitionTo<PullState>();
        if (Input.GetKey(KeyCode.Alpha3))
            base.TransitionTo<TimeDilationState>();
    }
}
