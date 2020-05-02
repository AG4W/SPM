using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Force/Pull")]
public class PullState : AbilityState
{
    [SerializeField]float radius = 4f;
    [SerializeField]float distance = 15f;
    [SerializeField]float power = 50f;

    [SerializeField]AnimationCurve initialAcceleration;

    public override void Tick()
    {
        base.Tick();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        Vector3 fp = ray.origin + (ray.direction.normalized * 4f);
        fp += Vector3.up;

        RaycastHit[] hits = Physics.SphereCastAll(ray, radius, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.GetComponent<IForceAffectable>() != null)
            {
                hits[i].transform.GetComponent<IForceAffectable>().ModifyVelocity(hits[i].transform.position.DirectionTo(fp).normalized * (base.Timer >= 1f ? power : initialAcceleration.Evaluate(base.Timer) * power) + base.Actor.Velocity);

                if (hits[i].transform.position.DistanceTo(fp) <= 2f)
                    hits[i].transform.GetComponent<IForceAffectable>().SetVelocity(base.Actor.Velocity);
            }
        }
    }
}
