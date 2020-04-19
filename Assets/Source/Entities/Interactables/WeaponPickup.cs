using UnityEngine;

public class WeaponPickup : ConnectedEntity
{
    Weapon weapon;

    public void RuntimeInitialize(Weapon weapon)
    {
        this.weapon = weapon;
    }

    protected override void OnInteractionStart()
    {
        base.OnInteractionStart();
    }
}
