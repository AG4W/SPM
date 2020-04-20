using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    [SerializeField]Weapon weapon;

    public string Prompt => weapon.name;
    public float InteractionDistance => 4f;

    public Vector3 Position => this.transform.position;

    void Awake()
    {
        Instantiate(weapon.Prefab, this.transform.position, Quaternion.identity, this.transform);

        this.gameObject.AddComponent<BoxCollider>();
        this.gameObject.AddComponent<Rigidbody>();
        this.gameObject.AddComponent<RigidBodyForceWrapper>();
    }
    public void Interact()
    {
        GlobalEvents.Raise(GlobalEvent.SetPlayerWeapon, weapon);
        Destroy(this.gameObject);
    }
}
