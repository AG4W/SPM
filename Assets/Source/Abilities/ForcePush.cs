using UnityEngine;

[System.Serializable]
public class ForcePush : Ability
{
    [SerializeField]float radius;
    [SerializeField]float distance;
    [SerializeField]float powerMultiplier;

    [SerializeField]AnimationCurve power;

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
                    UnityEngine.Debug.DrawLine(ray.origin, hits[i].point, Color.Lerp(Color.white, Color.red, base.DurationTimer01));

                hits[i].transform.GetComponent<IForceAffectable>().ModifyVelocity(ray.origin.DirectionTo(hits[i].point).normalized * powerMultiplier * power.Evaluate(base.DurationTimer01));
            }
        }
    }
}
