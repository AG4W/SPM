using UnityEngine;

public interface IForceAffectable
{
    void ModifyVelocity(Vector3 change);
    void SetVelocity(Vector3 velocity);
}