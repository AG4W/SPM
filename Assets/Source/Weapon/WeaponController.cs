using UnityEngine;

using System.Collections;
using UnityEngine.Rendering;
using UnityEditor.Experimental;

public class WeaponController : MonoBehaviour
{
    int shotsLeftInCurrentClip;

    float fireTimer;
    
    [SerializeField]Weapon weapon;
    [SerializeField]Transform anchorPoint;

    [SerializeField]LayerMask mask;

    Actor owner;

    //cached stuff
    GameObject model;
    GameObject muzzleFlash;
    Animator animator;
    AudioSource source;
    WeaponWorldUIController uiController;

    protected LayerMask Mask { get { return mask; } }

    public Transform LeftHandIKTarget { get; private set; }
    public Transform ExitPoint { get; private set; }

    public Weapon Weapon { get { return weapon; } }

    public bool CanFire { get; private set; }
    public bool NeedsReload { get { return shotsLeftInCurrentClip == 0; } }

    public void Initialize(Actor owner)
    {
        this.owner = owner;

        owner.Subscribe(ActorEvent.FireWeapon, FireWeapon);
        owner.Subscribe(ActorEvent.ReloadWeapon, ReloadWeapon);
        owner.Subscribe(ActorEvent.SetWeapon, SetWeapon);

        //inita
        owner.Raise(ActorEvent.SetWeapon, this.weapon);
    }
    void Update()
    {
        if (!this.CanFire && this.Weapon != null)
        {
            fireTimer += Time.deltaTime;

            if (fireTimer >= .03f)
                muzzleFlash.SetActive(false);

            if (fireTimer >= weapon.FireRate)
            {
                fireTimer = 0f;
                animator?.SetLayerWeight(1, 0f);
                this.CanFire = true;
            }
        }
    }

    void FireWeapon(object[] args)
    {
        Vector3 target = (Vector3)args[0];
        float magnitude = (float)args[1];

        if (!CanFire || NeedsReload)
            return;

        this.CanFire = false;

        muzzleFlash.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
        muzzleFlash.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
        muzzleFlash.SetActive(true);

        Vector3 velocitySpread = new Vector3(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude)) * .05f;
        Vector3 heading = this.ExitPoint.position.DirectionTo(target).normalized + velocitySpread;

        if (this.weapon.RepeatFirings > 1)
            this.StartCoroutine(FireAsync(target, heading, this.ExitPoint, mask));
        else
        {
            shotsLeftInCurrentClip--;
            uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
            this.weapon.OnFire(owner, target, heading, this.ExitPoint, mask);
        }

        animator?.Rebind();
        animator?.SetFloat("playspeedMultiplier", animator.GetCurrentAnimatorClipInfo(1)[0].clip.length / this.weapon.FireRate);
        animator?.SetLayerWeight(1, 1f);

        GlobalEvents.Raise(GlobalEvent.NoiseCreated, this.transform.position, weapon.NoiseValue);
    }
    IEnumerator FireAsync(Vector3 target, Vector3 heading, Transform exitPoint, LayerMask mask)
    {
        int count = shotsLeftInCurrentClip > this.weapon.RepeatFirings ? this.weapon.RepeatFirings : shotsLeftInCurrentClip;

        for (int i = 0; i < count; i++)
        {
            shotsLeftInCurrentClip--;
            uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);

            //apply velocity spread modifier
            weapon.OnFire(owner, target, heading, exitPoint, mask);
            yield return new WaitForSeconds(this.weapon.TimeBetweenFirings);
        }
    }

    void ReloadWeapon(object[] args)
    {
        shotsLeftInCurrentClip = weapon.ClipSize;
        uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
    }

    public void PlayReloadSFX()
    {
        source.PlayOneShot(this.weapon.ReloadSFX.Random());
    }

    public void StopAllSFX()
    {
        source.Stop();
    }

    void SetWeapon(params object[] args)
    {
        //delete old model
        if (model != null)
            Destroy(model);

        if (args[0] == null)
            this.weapon = null;
        else
        {
            //create copy to avoid collision with multiple controllers pointing to the same weapon
            this.weapon = Instantiate((Weapon)args[0]);

            //create new
            model = Instantiate(weapon.Prefab, anchorPoint.position, anchorPoint.rotation, anchorPoint);

            //update relevant transforms and cache them
            this.ExitPoint = model.transform.FindRecursively("exitPoint");
            this.LeftHandIKTarget = model.transform.FindRecursively("leftIK");
            animator = model.GetComponent<Animator>();
            uiController = model.GetComponentInChildren<WeaponWorldUIController>();
            muzzleFlash = model.transform.FindRecursively("muzzleFlash").gameObject;
            source = model.GetComponentInChildren<AudioSource>();

            muzzleFlash.SetActive(false);

            //update counter and UI
            shotsLeftInCurrentClip = this.weapon.ClipSize;
            uiController.UpdateUI(shotsLeftInCurrentClip, this.weapon.ClipSize);
        }

        owner.Raise(ActorEvent.SetLeftHandTarget, args[0] == null ? null : this.LeftHandIKTarget);
        owner.Raise(ActorEvent.SetLeftHandWeight, args[0] == null ? 0f : 1f);

        this.CanFire = this.weapon != null;
    }
    void OnDeath()
    {
        //skapa en pickupable av nuvarande vapen
        //så att spelaren kan ta upp det
    }
}
