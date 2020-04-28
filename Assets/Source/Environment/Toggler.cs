using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Toggler : MonoBehaviour
{
    [Header("Leave this empty to use player")]
    [SerializeField]GameObject triggeringObject;
    [Header("Objects to be toggled.")]
    [SerializeField]GameObject[] objects;

    Collider zone;

    void Awake()
    {
        triggeringObject = triggeringObject == null ? FindObjectOfType<PlayerActor>().gameObject : triggeringObject;

        Debug.Assert(objects != null && objects.Length > 0, this.name + " has no objects assigned to it and is useless, please fix.", this.gameObject);

        zone = this.GetComponent<Collider>();
        zone.isTrigger = true;
    }
    void Update()
    {
        if(zone.bounds.Contains(triggeringObject.transform.position))
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].SetActive(!objects[i].activeSelf);
                Destroy(this.gameObject);
            }
        }
    }
}