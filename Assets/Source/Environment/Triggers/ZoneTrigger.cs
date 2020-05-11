using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]

public class ZoneTrigger : MonoBehaviour
{
    [Header("Leave this empty to use player")]
    [SerializeField]GameObject triggeringObject;
    
    [SerializeField]bool repeatable = false;

    [Header("On trigger")]
    [Tooltip("Drag prefab")]
    [SerializeField]UnityEvent events;

    Collider zone;

    void Awake()
    {
        triggeringObject = triggeringObject == null ? FindObjectOfType<PlayerActor>().gameObject : triggeringObject;

        zone = this.GetComponent<Collider>();
        zone.isTrigger = true;
        zone.gameObject.layer = LayerMask.NameToLayer("TriggerZone");
    }
    void Update()
    {
        if(zone.bounds.Contains(triggeringObject.transform.position))
        {
            events?.Invoke();

            if(!repeatable)
                Destroy(this.gameObject);
        }
    }
}