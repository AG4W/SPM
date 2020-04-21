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
        Vector3 fp = ray.GetPoint(2f);

        RaycastHit[] hits = Physics.SphereCastAll(ray, radius, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.GetComponent<IForceAffectable>() != null)
            {
                if (base.Actor.transform.forward.Dot(fp.DirectionTo(hits[i].point).normalized) < .5f)
                    continue;

                if (hits[i].point.DistanceTo(fp) <= 3f)
                    hits[i].transform.GetComponent<IForceAffectable>().SetVelocity(Vector3.zero);
                else
                    hits[i].transform.GetComponent<IForceAffectable>().ModifyVelocity(hits[i].point.DirectionTo(fp).normalized * (base.Timer >= 1f ? power : initialAcceleration.Evaluate(base.Timer) * power));
            }
        }
    }
}
