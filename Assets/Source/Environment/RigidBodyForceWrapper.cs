using UnityEngine;

public class RigidBodyForceWrapper : MonoBehaviour, IForceAffectable
{
    Rigidbody body;

    void Awake() => body = this.GetComponent<Rigidbody>();

    void IForceAffectable.ModifyVelocity(Vector3 velocity) => body.AddForce(velocity, ForceMode.Impulse);
    void IForceAffectable.SetVelocity(Vector3 velocity) => body.velocity = velocity;
}
