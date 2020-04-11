using UnityEngine;

using System.Collections.Generic;

[System.Serializable]
public class ForcePull : Ability
{
    [SerializeField]float radius;
    [SerializeField]float distance;

    [SerializeField]AnimationCurve power;
    [SerializeField]float powerMultiplier;

    readonly List<Rigidbody> caughtObjects = new List<Rigidbody>();

    public override void Activate()
    {
        base.Activate();
        caughtObjects.Clear();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        RaycastHit[] hits = Physics.SphereCastAll(ray, radius, distance);

        Debug.DrawRay(ray.origin, ray.GetPoint(distance));

        for (int i = 0; i < hits.Length; i++)
            if (hits[i].transform.GetComponent<Rigidbody>())
                caughtObjects.Add(hits[i].rigidbody);
    }
    public override void Tick()
    {
        base.Tick();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));

        for (int i = 0; i < caughtObjects.Count; i++)
        {
            if (Vector3.Distance(caughtObjects[i].position, ray.origin) > 4f)
                caughtObjects[i].AddForce((ray.origin - caughtObjects[i].transform.position).normalized * powerMultiplier * base.DurationTimer, ForceMode.Acceleration);
            else
                caughtObjects[i].velocity = Vector3.zero;
        }

        //for (int i = 0; i < hits.Length; i++)
        //{
        //    float distanceModifier = Mathf.InverseLerp(distance, 0f, hits[i].distance);

        //    Rigidbody rb = hits[i].transform.GetComponent<Rigidbody>();
        //    NonPlayerActor nap = hits[i].transform.root.GetComponent<NonPlayerActor>();

        //    Vector3 force = Vector3.zero;

        //    if (hits[i].distance < 1f)
        //    {
        //        if (rb != null)
        //            rb.velocity = Vector3.zero;
        //    }
        //    else
        //        force = (ray.origin - hits[i].transform.root.position).normalized * power.Evaluate(base.DurationTimer01) * powerMultiplier * distanceModifier;

        //    //för icke-actors som fortfarande ska röra sig
        //    if (rb != null)
        //        rb.AddForce(force, ForceMode.Impulse);
        //    else if (nap != null)
        //        nap.ModifyVelocity(force * nap.ForceInfluenceModifier * Time.deltaTime);
        //}
    }
}
