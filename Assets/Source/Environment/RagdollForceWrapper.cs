using UnityEngine;

public class RagdollForceWrapper : MonoBehaviour, IForceAffectable, IDamageable
{
    [Tooltip("Controls type of impact vfx created when this object is hit")][SerializeField]IDamageableType type = IDamageableType.Organic;

    int count;
    Rigidbody body;

    IDamageableType IDamageable.Type => type;

    void Awake()
    {
        count = this.transform.root.GetComponentsInChildren<Rigidbody>().Length;
        body = this.GetComponent<Rigidbody>();
    }

    void IDamageable.OnHit(float damage)
    {

    }
    void IForceAffectable.ModifyVelocity(Vector3 velocity) => body.AddForce(velocity / (float)count, ForceMode.Impulse);
    void IForceAffectable.SetVelocity(Vector3 velocity) => body.velocity = velocity;
}
