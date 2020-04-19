using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ForcePull : Ability
{
    [SerializeField]float radius = 4;
    [SerializeField]float distance = 15f;
    [SerializeField]float stopDistance = 4f;

    [SerializeField]float powerMultiplier = 500f;
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
                if(base.Debug)
                    UnityEngine.Debug.DrawLine(ray.origin, hits[i].point, Color.Lerp(Color.white, Color.red, base.DurationTimer01));

                Vector3 fp = ray.GetPoint(3f);

                hits[i].transform.GetComponent<IForceAffectable>().ModifyVelocity(fp.DirectionFrom(hits[i].point).normalized * powerMultiplier * power.Evaluate(base.DurationTimer01));
            }
        }
    }
}
