using UnityEngine;

using System.Collections;

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
    AudioSource source;
    WeaponWorldUIController uiController;

    protected LayerMask Mask { get { return mask; } }

    public Transform LeftHandIKTarget { get; private set; }

    public bool CanFire { get; private set; }
    public bool IsReloading { get; private set; }
    public bool NeedsReload { get { return shotsLeftInCurrentClip == 0; } }

    void Awake()
    {
        SetWeapon(weapon);
    }
    void Update()
    {
        if (!CanFire && !IsReloading)
        {
            fireTimer += Time.deltaTime;

            if (fireTimer >= weapon.FireRate)
            {
                fireTimer = 0f;
                CanFire = true;
            }
        }
    }

    public void Fire(Vector3 target, float magnitude)
    {
        if (!CanFire || IsReloading)
            return;

        this.CanFire = false;
        shotsLeftInCurrentClip--;
        uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);

        //apply velocity spread modifier
        Vector3 velocitySpread = new Vector3(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude)) * .05f;
        Vector3 heading = exitPoint.position.DirectionTo(target).normalized + velocitySpread;
        weapon.Fire(target, heading, exitPoint, source, mask);

        GlobalEvents.Raise(GlobalEvent.NoiseCreated, this.transform.position, weapon.NoiseValue);
    }
    public void Reload()
    {
        if (this.IsReloading)
            return;

        StartCoroutine(ReloadAsync());
    }
    IEnumerator ReloadAsync()
    {
        this.IsReloading = true;
        yield return new WaitForSeconds(weapon.ReloadTime);

        shotsLeftInCurrentClip = weapon.ClipSize;
        uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
        this.IsReloading = false;
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
        source = model.GetComponentInChildren<AudioSource>();

        //update counter and UI
        shotsLeftInCurrentClip = this.weapon.ClipSize;
        uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
    }
}
