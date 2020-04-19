using UnityEngine;

public class WeaponController : MonoBehaviour
{
    int shotsLeftInCurrentClip;

    float fireTimer;
    
    [SerializeField]Weapon weapon;
    [SerializeField]Transform anchorPoint;

    [SerializeField]LayerMask mask;

    //cached stuff
    GameObject model;
    Transform exitPoint;
    Light muzzleFlash;
    AudioSource source;
    WeaponWorldUIController uiController;

    protected LayerMask Mask { get { return mask; } }

    public Transform LeftHandIKTarget { get; private set; }
    public Weapon Weapon { get { return weapon; } }

    public bool CanFire { get; private set; }
    public bool NeedsReload { get { return shotsLeftInCurrentClip == 0; } }

    void Awake()
    {
        SetWeapon(weapon);
    }
    void Update()
    {
        if (!CanFire)
        {
            fireTimer += Time.deltaTime;

            if (fireTimer >= .03f)
                muzzleFlash.gameObject.SetActive(false);

            if (fireTimer >= weapon.FireRate)
            {
                fireTimer = 0f;
                CanFire = true;
            }
        }
    }

    public void Fire(Vector3 target, float magnitude)
    {
        if (!CanFire || NeedsReload)
            return;

        this.CanFire = false;
        shotsLeftInCurrentClip--;
        uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
        muzzleFlash.gameObject.SetActive(true);

        //apply velocity spread modifier
        Vector3 velocitySpread = new Vector3(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude)) * .05f;
        Vector3 heading = exitPoint.position.DirectionTo(target).normalized + velocitySpread;
        weapon.Fire(target, heading, exitPoint, source, mask);

        GlobalEvents.Raise(GlobalEvent.NoiseCreated, this.transform.position, weapon.NoiseValue);
    }
    public void Reload()
    {
        shotsLeftInCurrentClip = weapon.ClipSize;
        uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
        source.PlayOneShot(this.weapon.ReloadSFX.Random());
    }

    public void SetWeapon(Weapon weapon)
    {
        //create copy to avoid collision with multiple controllers on the same weapon
        this.weapon = Instantiate(weapon);

        //delete old model
        if (model != null)
            Destroy(model);

        //create new
        model = Instantiate(weapon.Prefab, anchorPoint.position, anchorPoint.rotation, anchorPoint);

        //grab relevant transforms and cache them
        exitPoint = model.transform.FindRecursively("exitPoint");
        this.LeftHandIKTarget = model.transform.FindRecursively("leftIK");
        uiController = model.GetComponentInChildren<WeaponWorldUIController>();
        muzzleFlash = model.transform.FindRecursively("muzzleFlash").GetComponent<Light>();
        muzzleFlash.gameObject.SetActive(false);
        source = model.GetComponentInChildren<AudioSource>();

        //update counter and UI
        shotsLeftInCurrentClip = this.weapon.ClipSize;
        uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
    }
    public void OnDeath()
    {

    }
}
