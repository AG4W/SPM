﻿using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Force/Push")]
public class PushState : AbilityState
{
    [SerializeField]float radius = 4f;
    [SerializeField]float distance = 15f;
    [SerializeField]float power = 50f;
    [Tooltip("Defines the cone infront of the player in which objects should be affected by the force")]
    [SerializeField] [Range(-1f, 0f)] float angularThreshold = -.25f;

    [SerializeField]AnimationCurve initialAcceleration;

    RaycastHit[] hits = new RaycastHit[48];

    public override void Tick()
    {
        base.Tick();

        Ray ray = base.Camera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        Physics.SphereCastNonAlloc(ray, radius, hits, distance, affectableMask);

        // Using the force
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform != null)
                if (hits[i].transform.GetComponent<IForceAffectable>() != null && Vector3.Dot(base.Actor.transform.forward, base.Actor.transform.position.DirectionTo(hits[i].transform.position)) >= angularThreshold)
                hits[i].transform.GetComponent<IForceAffectable>().ModifyVelocity(ray.direction.normalized * (base.Timer >= 1f ? power : initialAcceleration.Evaluate(base.Timer) * power));
        }
    }
}
