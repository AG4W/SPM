using UnityEngine;

public class RagdollForceWrapper : MonoBehaviour, IForceAffectable
{
    int count;
    Rigidbody body;

    void Awake()
    {
        count = this.transform.root.GetComponentsInChildren<Rigidbody>().Length;
        body = this.GetComponent<Rigidbody>();
    }

    void IForceAffectable.ModifyVelocity(Vector3 velocity) => body.AddForce(velocity / count, ForceMode.Impulse);
    void IForceAffectable.SetVelocity(Vector3 velocity) => body.velocity = velocity;
}
