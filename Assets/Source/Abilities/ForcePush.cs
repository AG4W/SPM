using UnityEngine;

[System.Serializable]
public class ForcePush : Ability
{
    [SerializeField]float radius;
    [SerializeField]float distance;

    [SerializeField]AnimationCurve power;
    [SerializeField]float powerMultiplier;

    Transform caster;

    public override void Activate(Context context)
    {
        base.Activate(context);
        caster = (Transform)context.data["caster"];
    }

    public override void Tick()
    {
        base.Tick();

        if (base.IsActive)
        {
            Vector3 point = caster.transform.position + (Vector3.up * 1f);
            Vector3 direction = caster.transform.forward;

            RaycastHit[] hits = Physics.SphereCastAll(point, radius, direction, distance);

            Debug.DrawRay(point, direction * distance);

            for (int i = 0; i < hits.Length; i++)
            {
                Rigidbody rb = hits[i].transform.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    float distanceModifier = Mathf.InverseLerp(distance, 0f, hits[i].distance);
                    rb.AddForce(direction * power.Evaluate(base.DurationTimer01) * powerMultiplier * distanceModifier, ForceMode.Force);
                }
            }
        }
    }
}
