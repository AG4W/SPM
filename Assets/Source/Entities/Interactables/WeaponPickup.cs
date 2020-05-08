using UnityEngine;
using UnityEngine.Events;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    [SerializeField]Weapon weapon;

    [SerializeField]UnityEvent events;

    public string Prompt =>
        weapon.name + "\n" +
        "<size=10>DMG: " + weapon.Damage + "</size>\n" +
        "<size=10>Clipsize: " + weapon.ClipSize + "</size>\n" +
        "<size=10>Reload: " + weapon.ReloadTime + "s</size>\n\n" +
        "[<color=teal>E</color>]: to equip";
    public float InteractionDistance => 4f;

    public Vector3 Position => this.transform.position;

    void Awake()
    {
        GameObject g = Instantiate(weapon.Prefab, this.transform.position, Quaternion.identity, this.transform);

        BoxCollider bc = this.gameObject.AddComponent<BoxCollider>();
        bc.size = new Vector3(1f, 1f, .25f);

        this.gameObject.AddComponent<Rigidbody>();
        this.gameObject.GetComponent<Rigidbody>().mass = 25f;
        this.gameObject.AddComponent<RigidBodyForceWrapper>();

        //stäng av alla ljus
        foreach (Light light in g.GetComponentsInChildren<Light>())
            light.enabled = false;

        this.transform.FindRecursively("muzzleFlash").gameObject.SetActive(false);
    }
    public void Interact()
    {
        GlobalEvents.Raise(GlobalEvent.SetPlayerWeapon, weapon);
        events?.Invoke();

        Destroy(this.gameObject);
    }
}
