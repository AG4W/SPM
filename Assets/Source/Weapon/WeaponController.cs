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
    GameObject muzzleFlash;
    AudioSource source;
    WeaponWorldUIController uiController;

    protected LayerMask Mask { get { return mask; } }

    public Transform LeftHandIKTarget { get; private set; }
    public Transform ExitPoint { get; private set; }

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
                muzzleFlash.SetActive(false);

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
        muzzleFlash.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
        muzzleFlash.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
        muzzleFlash.SetActive(true);

        Vector3 velocitySpread = new Vector3(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude)) * .05f;
        Vector3 heading = ExitPoint.position.DirectionTo(target).normalized + velocitySpread;

        if (this.weapon.RepeatFirings > 1)
            this.StartCoroutine(FireAsync(target, heading, this.ExitPoint, source, mask));
        else
        {
            shotsLeftInCurrentClip--;
            this.weapon.Fire(target, heading, this.ExitPoint, source, mask);
            uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
        }

        GlobalEvents.Raise(GlobalEvent.NoiseCreated, this.transform.position, weapon.NoiseValue);
    }
    IEnumerator FireAsync(Vector3 target, Vector3 heading, Transform exitPoint, AudioSource source, LayerMask mask)
    {
        int count = shotsLeftInCurrentClip > this.weapon.RepeatFirings ? this.weapon.RepeatFirings : shotsLeftInCurrentClip;

        for (int i = 0; i < count; i++)
        {
            shotsLeftInCurrentClip--;
            uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);

            //apply velocity spread modifier
            weapon.Fire(target, heading, exitPoint, source, mask);
            yield return new WaitForSeconds(this.weapon.TimeBetweenFirings);
        }
    }

    public void Reload()
    {
        shotsLeftInCurrentClip = weapon.ClipSize;
        uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
        source.PlayOneShot(this.weapon.ReloadSFX.Random());
    }

    public void SetWeapon(Weapon weapon)
    {
        //create copy to avoid collision with multiple controllers pointing to the same weapon
        this.weapon = Instantiate(weapon);

        //delete old model
        if (model != null)
            Destroy(model);

        //create new
        model = Instantiate(weapon.Prefab, anchorPoint.position, anchorPoint.rotation, anchorPoint);

        //update relevant transforms and cache them
        ExitPoint = model.transform.FindRecursively("exitPoint");
        this.LeftHandIKTarget = model.transform.FindRecursively("leftIK");
        uiController = model.GetComponentInChildren<WeaponWorldUIController>();
        muzzleFlash = model.transform.FindRecursively("muzzleFlash").gameObject;
        muzzleFlash.SetActive(false);
        source = model.GetComponentInChildren<AudioSource>();

        //update counter and UI
        shotsLeftInCurrentClip = this.weapon.ClipSize;
        uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
    }
    public void OnDeath()
    {
        //skapa en pickupable av nuvarande vapen
        //så att spelaren kan ta upp det
    }
}
