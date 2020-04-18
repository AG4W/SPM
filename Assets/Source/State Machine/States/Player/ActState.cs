using UnityEngine;

public abstract class ActState : BaseLocomotionState
{
    protected override void OnInitialize()
    {
        base.OnInitialize();

        base.Actor.Subscribe(ActorEvent.ReloadActorWeapon, (object[] args) =>
        {
            if (this.IsActiveState)
                ((WeaponController)base.Context["weapon"]).Reload();
        });
    }

    public override void Tick()
    {
        base.Tick();

        //fire
        if (Input.GetKey(KeyCode.Mouse0))
        {                                                                                         //    \/ aaaaaaaaay lmao
            if (!((WeaponController)base.Context["weapon"]).CanFire || base.Actor.ActualInput.normalized.magnitude > 1f)
                return;

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

            base.Actor.Raise(ActorEvent.FireActorWeapon, hit.transform != null ? hit.point : ray.GetPoint(300f), base.Actor.ActualInput.magnitude);
        }
    }
}
