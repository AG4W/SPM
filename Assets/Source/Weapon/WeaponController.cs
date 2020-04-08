using UnityEngine;

using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]int magasineSize = 20;
    [SerializeField]int shotsLeftInCurrentClip;

    [SerializeField]float projectileSpeed = 50f;
    [SerializeField]float projectileLifetime = 2f;

    [SerializeField]float fireTimer;
    [SerializeField]float fireRate = .5f;

    [SerializeField]float damage = 3f;

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

    List<ProjectilePacket> projectiles = new List<ProjectilePacket>();

    bool canFire = true;
    bool needsReload { get { return shotsLeftInCurrentClip == 0; } }

    void Start()
    {
        shotsLeftInCurrentClip = magasineSize;

        if (source == null)
        {
            source = this.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }
    }
    void Update()
    {
        for (int i = 0; i < projectiles.Count; i++)
            if (projectiles[i].Tick(Time.deltaTime))
                projectiles.RemoveAt(i);

        if (!canFire)
            TickFireTimer();
    }

    public void Shoot(Vector3 position)
    {
        if (needsReload)
        {
            Reload();
            return;
        }

        if (!canFire)
            return;

        ShootInternal(position);
    }
    public void Reload()
    {
        shotsLeftInCurrentClip = magasineSize;
        CreateReloadSFX();
    }

    void ShootInternal(Vector3 target)
    {
        canFire = false;
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
    }

    void TickFireTimer()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            canFire = true;
        }
    }

    void CreateReloadSFX()
    {
        if (reloadSounds == null || reloadSounds.Length == 0)
            return;

        source.PlayOneShot(reloadSounds.Random());
    }
    void CreateSFX()
    {
        if (shotSounds == null || shotSounds.Length == 0)
            return;

        source.pitch = Random.Range(Mathf.Min(minPitch, maxPitch), Mathf.Max(minPitch, maxPitch));
        source.PlayOneShot(shotSounds.Random());
    }
    void CreateVFX(Vector3 heading)
    {
        if (shotPrefabs == null || shotPrefabs.Length == 0)
            return;

        GameObject bullet = Instantiate(shotPrefabs.Random(), exitPoint.position, Quaternion.LookRotation(heading, Vector3.up), null);
        projectiles.Add(new ProjectilePacket(projectileLifetime, projectileSpeed, bullet));
    }
}
public struct ProjectilePacket
{
    float timer;
    float projectileLifetime;
    float projectileSpeed;

    GameObject bullet;

    public ProjectilePacket(float projectileLifetime, float projectileSpeed, GameObject bullet)
    {
        this.projectileLifetime = projectileLifetime;
        this.projectileSpeed = projectileSpeed;
        this.bullet = bullet;
        this.timer = 0f;
    }

    public bool Tick(float deltaTime)
    {
        timer += deltaTime;
        
        if(timer >= projectileLifetime)
        {
            Object.Destroy(bullet);
            return true;
        }
        
        bullet.transform.position += bullet.transform.forward * projectileSpeed * deltaTime;
        return false;
    }
}
