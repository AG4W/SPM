using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Default")]
public class Weapon : ScriptableObject
{
    [SerializeField]int clipSize = 20;

    [SerializeField]float damage = 3f;
    [Range(.03f, 2f)][SerializeField]float fireRate = .5f;
    [SerializeField]float stoppingPower = 10f;
    [SerializeField]float spread = 0f;
    [SerializeField]float reloadTime = 2f;
    [SerializeField]float noiseValue = 50f;

    [Range(0f, 1f)][SerializeField]float traumaValue = .2f;
    //[SerializeField]float zoomMultiplier = 2f;

    [Header("Visual")]
    [SerializeField]GameObject prefab;
 
    [SerializeField]GameObject[] shots;
    [SerializeField]GameObject[] impacts;
    [SerializeField]GameObject[] hits;

    [Header("Audio")]
    [SerializeField]AudioClip[] shotSFX;
    [Range(0f, 2f)][SerializeField]float minPitch = .75f;
    [Range(0f, 2f)][SerializeField]float maxPitch = 1.25f;

    [SerializeField]AudioClip[] reloadSFX;

    public int ClipSize => clipSize;
    public virtual int RepeatFirings => 1;
    public virtual float TimeBetweenFirings => 0f;

    public float Damage => damage;
    public float FireRate => fireRate;
    public float StoppingPower => stoppingPower;
    public float Spread => spread;
    public float ReloadTime => reloadTime;
    public float NoiseValue => noiseValue;
    public float TraumaValue => traumaValue;
    //public float ZoomMultiplier => zoomMultiplier;

    public GameObject Prefab => prefab;
    public GameObject[] Shots => shots;
    public GameObject[] Impacts => impacts;
    public GameObject[] Hits => hits;

    public AudioClip[] ShotSFX => shotSFX;
    public float MinPitch => minPitch;
    public float MaxPitch => maxPitch;

    public AudioClip[] ReloadSFX => reloadSFX;

    public virtual void Fire(Vector3 target, Vector3 heading, Transform exitPoint, AudioSource source, LayerMask mask)
    {
        Physics.Raycast(exitPoint.position, heading.normalized, out RaycastHit hit, Mathf.Infinity, mask);

        hit.transform?.GetComponent<IForceAffectable>()?.ModifyVelocity(heading.normalized * (this.StoppingPower / hit.point.DistanceTo(exitPoint.position)));
        hit.transform?.GetComponent<IDamageable>()?.OnHit(-this.Damage);

        Debug.DrawLine(exitPoint.position, hit.transform != null ? hit.point : exitPoint.position + heading.normalized * 300f, Color.red);

        CreateShotVFX(heading, hit, exitPoint);
        CreateShotSFX(source);
    }
    protected virtual void CreateShotVFX(Vector3 heading, RaycastHit hit, Transform exitPoint)
    {
        Instantiate(shots.Random(), exitPoint.position, Quaternion.LookRotation(heading, Vector3.up), null).GetComponent<ProjectileEntity>().Initialize(hit);

        if (hit.transform != null)
        {
            Actor a = hit.transform.GetComponent<Actor>();

            //dont create hit markers on actors 
            if (a == null)
                Instantiate(hits.Random(), hit.point, Quaternion.LookRotation(hit.normal), hit.transform);

            Instantiate(impacts.Random(), hit.point, Quaternion.LookRotation(hit.normal, Vector3.up), hit.transform);
        }
    }
    protected virtual void CreateShotSFX(AudioSource source)
    {
        source.pitch = Random.Range(minPitch, maxPitch);
        source.PlayOneShot(shotSFX.Random());
    }
}
