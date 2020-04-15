using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class WeaponController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]int magasineSize = 20;
    [SerializeField]int shotsLeftInCurrentClip;

    [SerializeField]float fireTimer;
    [SerializeField]float fireRate = .5f;

    [SerializeField]float damage = 3f;
    [SerializeField]float reloadTime = 2f;

    [SerializeField]float noiseValue = 50f;

    [SerializeField]LayerMask mask;

    [Header("Visual")]
    [SerializeField]GameObject[] shotPrefabs;
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

    [SerializeField]Text[] texts;
    [SerializeField]Image[] images;

    [SerializeField]Light[] lights;

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

        UpdateWorldUI();
        GlobalEvents.Subscribe(GlobalEvent.FireWeapon, OnFireWeapon);
    }
    void Update()
    {
        if (!CanFire && !IsReloading)
            TickFireTimer();
    }

    public void Reload()
    {
        StartCoroutine(ReloadAsync());
    }
    IEnumerator ReloadAsync()
    {
        this.IsReloading = true;

        CreateReloadSFX();

        for (int i = 0; i < texts.Length; i++)
            texts[i].text = "Ø";

        yield return new WaitForSeconds(reloadTime);

        shotsLeftInCurrentClip = magasineSize;
        UpdateWorldUI();

        this.IsReloading = false;
    }

    void OnFireWeapon(object[] args)
    {
        if (this != (WeaponController)args[0])
            return;

        if (CanFire && !IsReloading)
            FireWeapon((Vector3)args[1]);
    }
    void FireWeapon(Vector3 target)
    {
        CanFire = false;
        shotsLeftInCurrentClip--;

        Vector3 heading = target - exitPoint.position;

        Physics.Raycast(exitPoint.position, heading.normalized, out RaycastHit hit, Mathf.Infinity, mask);

        if(hit.transform != null)
        {
            Entity e = hit.transform.root.GetComponent<Entity>();

            //hit something else, create hit marker or something
            if (e == null)
            {
            }
            else
                e.Health.Update(-damage);

            Debug.DrawLine(exitPoint.transform.position, hit.point);
        }

        CreateSFX();
        CreateVFX(heading);

        UpdateWorldUI();
        GlobalEvents.Raise(GlobalEvent.NoiseCreated, this.transform.position, noiseValue);

        if (shotsLeftInCurrentClip == 0)
            Reload();
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

    void CreateVFX(Vector3 heading)
    {
        if (shotPrefabs == null || shotPrefabs.Length == 0)
            return;

        GameObject bullet = Instantiate(shotPrefabs.Random(), exitPoint.position, Quaternion.LookRotation(heading, Vector3.up), null);
    }
    void CreateSFX()
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
