using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Burst")]
public class BurstWeapon : Weapon
{
    [Header("Burst Weapon Specific")]
    [SerializeField]int roundsPerBurst = 3;
    [SerializeField]float interBurstTime = .01f;

    public override int RepeatFirings => roundsPerBurst;
    public override float TimeBetweenFirings => interBurstTime;

    public override void OnFire(Actor shooter, Vector3 target, Vector3 heading, Transform exitPoint, AudioSource source, LayerMask mask)
    {
        Vector3 h = heading + new Vector3(Random.Range(-base.Spread, base.Spread), Random.Range(-base.Spread, base.Spread), Random.Range(-base.Spread, base.Spread));
        Physics.Raycast(exitPoint.position, h, out RaycastHit hit, Mathf.Infinity, mask);

        hit.transform?.GetComponent<IForceAffectable>()?.ModifyVelocity(h.normalized * (base.StoppingPower / hit.point.DistanceTo(exitPoint.position)));
        hit.transform?.GetComponent<IDamageable>()?.OnHit(-base.Damage);

        if (hit.transform?.GetComponent<IDamageable>() != null)
            shooter.Raise(ActorEvent.ShotHit, hit.transform.GetComponent<IDamageable>());
        else
            shooter.Raise(ActorEvent.ShotMissed);

        //Debug.DrawLine(base.ExitPoint.position, hit.transform != null ? hit.point : base.ExitPoint.position + h.normalized * 50f, hit.transform != null ? Color.red : Color.yellow, .5f);
        base.CreateShotVFX(h, hit, exitPoint);
        base.CreateShotSFX(source);
    }
}
