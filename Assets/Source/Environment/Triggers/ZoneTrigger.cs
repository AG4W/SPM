using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class ZoneTrigger : MonoBehaviour
{
    [Header("Leave this empty to use player")]
    [SerializeField]GameObject triggeringObject;
    [Header("On trigger")]
    [SerializeField]UnityEvent events;

    Collider zone;

    void Awake()
    {
        triggeringObject = triggeringObject == null ? FindObjectOfType<PlayerActor>().gameObject : triggeringObject;

        zone = this.GetComponent<Collider>();
        zone.isTrigger = true;
    }
    void Update()
    {
        if(zone.bounds.Contains(triggeringObject.transform.position))
        {
            events?.Invoke();
            Destroy(this.gameObject);
        }
    }
}