﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class WeaponController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]int magasineSize = 20;
    [SerializeField]int shotsLeftInCurrentClip;

    [SerializeField]float fireTimer;
    [SerializeField]float fireRate = .5f;
    [SerializeField]float baseSpread = 0f;

    [SerializeField]float damage = 3f;
    [SerializeField]float reloadTime = 2f;
    [SerializeField]float stoppingPower = 10f;

    [SerializeField]float noiseValue = 50f;

    [SerializeField]float zoomMultiplier = 2f;

    [SerializeField]LayerMask mask;

    [Header("Visual")]
    [SerializeField]GameObject[] shotPrefabs;
    [SerializeField]GameObject[] impactPrefabs;
    [SerializeField]GameObject[] hitPrefabs;

    [SerializeField]Transform exitPoint;

    [Header("Audio")]
    [SerializeField]AudioSource source;
    [SerializeField]AudioClip[] shotSounds;
    [Range(0f, 2f)][SerializeField]float minPitch = .75f;
    [Range(0f, 2f)][SerializeField]float maxPitch = 1.25f;

    [SerializeField]AudioClip[] reloadSounds;

    [Header("World UI")]
    [SerializeField]Color low = Color.red;
    [SerializeField]Color high = Color.green;

    [SerializeField]float textBlinkDuration = .1f;

    [SerializeField]Text[] texts;
    [SerializeField]string[] reloadPrompts;
    [SerializeField]Image[] images;

    [SerializeField]Light[] lights;

    protected float Damage { get { return damage; } }
    protected float StoppingPower { get { return stoppingPower; } }
    protected float BaseSpread { get { return baseSpread; } }

    protected Transform ExitPoint { get { return exitPoint; } }

    protected LayerMask Mask { get { return mask; } }

    public float ZoomMultiplier { get { return zoomMultiplier; } }
    public bool CanFire { get; private set; }
    public bool IsReloading { get; private set; }

    void Start()
    {
        shotsLeftInCurrentClip = magasineSize;

        if (source == null)
        {
            source = this.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        this.GetComponentInChildren<Canvas>().worldCamera = Camera.main;

        UpdateWorldUI();
    }
    void Update()
    {
        if (!CanFire && !IsReloading)
            TickFireTimer();
    }

    public void FireWeapon(object[] args)
    {
        if (CanFire && !IsReloading)
            FireWeapon((Vector3)args[0], (float)args[1]);
    }
    void FireWeapon(Vector3 target, float magnitude)
    {
        this.CanFire = false;
        shotsLeftInCurrentClip--;

        //apply velocity spread from actor movement
        OnFireWeapon(target, exitPoint.position.DirectionTo(target).normalized + (new Vector3(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude))) * .025f);
        UpdateWorldUI();

        GlobalEvents.Raise(GlobalEvent.NoiseCreated, this.transform.position, noiseValue);

        if (shotsLeftInCurrentClip == 0)
            Reload();
    }
    protected virtual void OnFireWeapon(Vector3 target, Vector3 heading)
    {
        //apply weapon spread
        heading += new Vector3(Random.Range(-baseSpread, baseSpread), Random.Range(-baseSpread, baseSpread), Random.Range(-baseSpread, baseSpread));
        Physics.Raycast(exitPoint.position, heading.normalized, out RaycastHit hit, Mathf.Infinity, mask);

        if (hit.transform != null)
        {
            Entity e = hit.transform.root.GetComponent<Entity>();

            //hit something else, create hit marker or something    
            if (e == null)
            {
                if (hit.transform.GetComponent<Rigidbody>())
                    hit.transform.GetComponent<Rigidbody>().AddForce(heading.normalized * (this.StoppingPower / hit.point.DistanceTo(this.ExitPoint.position)), ForceMode.Impulse);
            }
            else
                e.Health.Update(-damage);
        }

        Debug.DrawLine(exitPoint.position, hit.transform != null ? hit.point : exitPoint.position + heading.normalized * 300f, Color.red);

        CreateSFX();
        CreateVFX(heading, hit);
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
        CreateReloadSFX();

        for (int i = 0; i < texts.Length; i++)
            texts[i].text = reloadPrompts.Random();

        float t = 0f;
        float bt = 0f;

        while (t <= reloadTime)
        {
            t += Time.deltaTime;
            bt += Time.deltaTime;

            if(bt >= textBlinkDuration)
            {
                for (int i = 0; i < texts.Length; i++)
                    texts[i].gameObject.SetActive(!texts[i].gameObject.activeSelf);

                for (int i = 0; i < images.Length; i++)
                    images[i].gameObject.SetActive(!images[i].gameObject.activeSelf);

                for (int i = 0; i < lights.Length; i++)
                    lights[i].gameObject.SetActive(!lights[i].gameObject.activeSelf);

                bt = 0f;
            }

            yield return null;
        }

        for (int i = 0; i < texts.Length; i++)
            texts[i].gameObject.SetActive(true);
        for (int i = 0; i < images.Length; i++)
            images[i].gameObject.SetActive(true);
        for (int i = 0; i < lights.Length; i++)
            lights[i].gameObject.SetActive(true);
        
        shotsLeftInCurrentClip = magasineSize;
        UpdateWorldUI();

        this.IsReloading = false;
    }

    void TickFireTimer()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            CanFire = true;
        }
    }

    void UpdateWorldUI()
    {
        float t = shotsLeftInCurrentClip / (float)magasineSize;
        Color c = Color.Lerp(low, high, t);

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].material.SetColor("_Color", c);
            texts[i].material.SetColor("_Emission", c * 8f);
            texts[i].color = c;

            texts[i].text = shotsLeftInCurrentClip.ToString();
        }

        for (int i = 0; i < images.Length; i++)
        {
            images[i].material.SetColor("_Color", c);
            images[i].material.SetColor("_Emission", c * 8f);
            images[i].color = c;
            images[i].fillAmount = t;
        }

        for (int i = 0; i < lights.Length; i++)
            lights[i].color = c;
    }

    protected virtual void CreateVFX(Vector3 heading, RaycastHit hit)
    {
        GameObject bullet = Instantiate(shotPrefabs.Random(), exitPoint.position, Quaternion.LookRotation(heading, Vector3.up), null);
        bullet.GetComponent<ProjectileEntity>().Initialize(hit);

        if(hit.transform != null)
        {
            Entity e = hit.transform.root.GetComponent<Entity>();

            //dont create hit markers on actors 
            if (e == null)
                Instantiate(hitPrefabs.Random(), hit.point, Quaternion.LookRotation(hit.normal), hit.transform);
                
            Instantiate(impactPrefabs.Random(), hit.point, Quaternion.LookRotation(hit.normal, Vector3.up), hit.transform);
        }
    }
    protected virtual void CreateSFX()
    {
        if (shotSounds == null || shotSounds.Length == 0)
            return;

        source.pitch = Random.Range(Mathf.Min(minPitch, maxPitch), Mathf.Max(minPitch, maxPitch));
        source.PlayOneShot(shotSounds.Random());
    }
    void CreateReloadSFX()
    {
        if (reloadSounds == null || reloadSounds.Length == 0)
            return;

        source.PlayOneShot(reloadSounds.Random());
    }
}
