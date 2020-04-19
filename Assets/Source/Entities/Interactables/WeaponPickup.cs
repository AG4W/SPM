using UnityEngine;

public class WeaponPickup : ConnectedEntity
{
    [SerializeField]Weapon weapon;

    public override void OnInteractStart()
    {
        base.OnInteractStart();
    }
}
