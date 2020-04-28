using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Force/Push")]
public class PushState : AbilityState
{
    [SerializeField]float radius = 4f;
    [SerializeField]float distance = 15f;
    [SerializeField]float power = 50f;

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

                hits[i].transform.GetComponent<IForceAffectable>().ModifyVelocity(ray.direction.normalized * (base.Timer >= 1f ? power : initialAcceleration.Evaluate(base.Timer) * power));
            }
        }
    }
}
