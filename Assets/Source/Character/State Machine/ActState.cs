﻿using UnityEngine;

public abstract class ActState : BaseLocomotionState
{
    public override void Tick()
    {
        base.Tick();

        //fire
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (!((WeaponController)base.Context["weapon"]).CanFire)
                return;

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

            GlobalEvents.Raise(GlobalEvent.FireWeapon, (WeaponController)base.Context["weapon"], hit.transform != null ? hit.point : ray.GetPoint(300f));
        }
    }
}
