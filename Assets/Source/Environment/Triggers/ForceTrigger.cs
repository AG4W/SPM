using UnityEngine;
using UnityEngine.Events;

public class ForceTrigger : MonoBehaviour, IForceAffectable
{
    [SerializeField]float forceThreshold = 0f;

    [SerializeField]GameObject[] objects;

    [SerializeField]UnityEvent events;

    void IForceAffectable.ModifyVelocity(Vector3 change)
    {
        if (change.magnitude < forceThreshold)
            return;

        for (int i = 0; i < objects.Length; i++)
            objects[i].SetActive(!objects[i].activeSelf);

        events?.Invoke();
        Destroy(this);
    }
    void IForceAffectable.SetVelocity(Vector3 velocity)
    {
        if (velocity.magnitude < forceThreshold)
            return;

        for (int i = 0; i < objects.Length; i++)
            objects[i].SetActive(!objects[i].activeSelf);

        events?.Invoke();
        Destroy(this);
    }
}
