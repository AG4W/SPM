using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Burst")]
public class BurstWeapon : Weapon
{
    [Header("Burst Weapon Specific")]
    [SerializeField]int roundsPerBurst = 3;
    [SerializeField]float interBurstTime = .01f;

    public override int RepeatFirings => roundsPerBurst;
    public override float TimeBetweenFirings => interBurstTime;

    public override void Fire(Vector3 target, Vector3 heading, Transform exitPoint, AudioSource source, LayerMask mask)
    {
        Vector3 h = heading + new Vector3(Random.Range(-base.Spread, base.Spread), Random.Range(-base.Spread, base.Spread), Random.Range(-base.Spread, base.Spread));
        Physics.Raycast(exitPoint.position, h, out RaycastHit hit, Mathf.Infinity, mask);

        if (hit.transform != null)
        {
            Entity e = hit.transform.root.GetComponent<Entity>();

            //hit something else, create hit marker or something    
            if (e == null)
            {
                if (hit.transform.GetComponent<Rigidbody>())
                {
                    //do some dropoff so we dont shoot stuff around on the entire map
                    hit.transform.GetComponent<Rigidbody>().AddForce(h.normalized * (base.StoppingPower / hit.point.DistanceTo(exitPoint.position)), ForceMode.Impulse);
                }
            }
            else
                e.Health.Update(-base.Damage);
        }

        //Debug.DrawLine(base.ExitPoint.position, hit.transform != null ? hit.point : base.ExitPoint.position + h.normalized * 50f, hit.transform != null ? Color.red : Color.yellow, .5f);
        base.CreateShotVFX(h, hit, exitPoint);
        base.CreateShotSFX(source);
    }
}
