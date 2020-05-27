using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Force/Pull")]
public class PullState : AbilityState
{
    [SerializeField]float radius = 4f;
    [SerializeField]float distance = 15f;
    [SerializeField]float power = 50f;

    [SerializeField]AnimationCurve initialAcceleration;

    RaycastHit[] hits = new RaycastHit[48];

    public override void Tick()
    {
        base.Tick();

        Ray ray = base.Camera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        Vector3 fp = ray.origin + (ray.direction.normalized * 2.5f);
        fp += Vector3.up;
        Physics.SphereCastNonAlloc(ray, radius, hits, distance, affectableMask);

        // Using the force
        for (int i = 0; i < hits.Length; i++)
        {
            if(hits[i].transform != null)
                if (hits[i].transform.GetComponent<IForceAffectable>() != null && Vector3.Dot(base.Actor.transform.forward, base.Actor.transform.position.DirectionTo(hits[i].transform.position)) >= -.25f)
                {
                    hits[i].transform.GetComponent<IForceAffectable>().ModifyVelocity(hits[i].transform.position.DirectionTo(fp).normalized * (base.Timer >= 1f ? power : initialAcceleration.Evaluate(base.Timer) * power) + base.Actor.Velocity);

                    if (hits[i].transform.position.DistanceTo(fp) <= 2f)
                        hits[i].transform.GetComponent<IForceAffectable>().SetVelocity(base.Actor.Velocity);
                }
        }
    }
}
