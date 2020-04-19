using UnityEngine;
using UnityEngine.AI;

public class RigidBodyForceWrapper : MonoBehaviour, IForceAffectable
{
    Rigidbody[] rbs;

    void Awake()
    {
        rbs = this.GetComponentsInChildren<Rigidbody>();
    }

    void IForceAffectable.ModifyVelocity(Vector3 change)
    {
        for (int i = 0; i < rbs.Length; i++)
            rbs[i].AddForce(change, ForceMode.Force);
    }
    void IForceAffectable.SetVelocity(Vector3 velocity)
    {
        for (int i = 0; i < rbs.Length; i++)
        {
            rbs[i].velocity = velocity;
            //rbs[i].angularVelocity = velocity;
        }
    }
}
