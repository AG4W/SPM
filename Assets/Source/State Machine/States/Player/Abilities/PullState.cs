using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Force/Pull")]
public class PullState : AbilityState
{
    [SerializeField]float radius = 4f;
    [SerializeField]float distance = 15f;
    [SerializeField]float power = 50f;

    [SerializeField]float originPointDistanceFromCamera = 2f;

    [SerializeField]AnimationCurve initialAcceleration;

    public override void Tick()
    {
        base.Tick();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        RaycastHit[] hits = Physics.SphereCastAll(ray, radius, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.GetComponent<IForceAffectable>() != null)
            {
                if (base.Debug)
                    UnityEngine.Debug.DrawLine(ray.origin, hits[i].point, Color.yellow);

                Vector3 fp = ray.GetPoint(3f);

                if (ray.origin.DistanceTo(hits[i].point) <= originPointDistanceFromCamera)
                    hits[i].transform.GetComponent<IForceAffectable>().SetVelocity(Vector3.zero);
                else
                    hits[i].transform.GetComponent<IForceAffectable>().ModifyVelocity(fp.DirectionFrom(hits[i].point).normalized * (base.Timer >= 1f ? power : initialAcceleration.Evaluate(base.Timer) * power));
            }
        }
    }
}
