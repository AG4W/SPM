using UnityEngine;

[System.Serializable]
public class ForcePull : Ability
{
    [SerializeField]float radius;
    [SerializeField]float distance;

    [SerializeField]AnimationCurve power;
    [SerializeField]float powerMultiplier;

    public override void Tick()
    {
        base.Tick();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        RaycastHit[] hits = Physics.SphereCastAll(ray, radius, distance);

        Debug.DrawRay(ray.origin, ray.GetPoint(distance));

        for (int i = 0; i < hits.Length; i++)
        {
            float distanceModifier = Mathf.InverseLerp(distance, 0f, hits[i].distance);

            Rigidbody rb = hits[i].transform.GetComponent<Rigidbody>();
            NonPlayerActor nap = hits[i].transform.root.GetComponent<NonPlayerActor>();

            Vector3 force = (ray.origin - hits[i].transform.root.position).normalized * power.Evaluate(base.DurationTimer01) * powerMultiplier * distanceModifier;

            //för icke-actors som fortfarande ska röra sig
            if (rb != null)
                rb.AddForce(force, ForceMode.Impulse);
            else if (nap != null)
                nap.ModifyVelocity(force * nap.ForceInfluenceModifier * Time.deltaTime);
        }
    }
}
