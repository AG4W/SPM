﻿using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Force/Push")]
public class PushState : AbilityState
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
        //RaycastHit[] hits = Physics.SphereCastAll(ray, radius, distance, affectableMask);

        Physics.SphereCastNonAlloc(ray, radius, hits, distance, affectableMask);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform != null)
                if (hits[i].transform.GetComponent<IForceAffectable>() != null)
                hits[i].transform.GetComponent<IForceAffectable>().ModifyVelocity(ray.direction.normalized * (base.Timer >= 1f ? power : initialAcceleration.Evaluate(base.Timer) * power));
        }
    }
}
