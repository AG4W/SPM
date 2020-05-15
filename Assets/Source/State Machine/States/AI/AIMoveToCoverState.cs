using UnityEngine;
using UnityEngine.AI;

using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "State/AI/Move To Cover")]
public class AIMoveToCoverState : AIBaseLocomotionState
{
    [SerializeField]int samples = 8;
    [SerializeField]float sampleRadius = 8f;

    [SerializeField]bool debugMode = true;

    Vector3 coverPosition;

    public override void Enter()
    {
        List<Vector3> candidates = new List<Vector3>();

        //locate best cover position here
        for (int i = 0; i < 8; i++)
        {
            //create sample position
            Vector3 sp = new Vector3(sampleRadius * Mathf.Cos(Mathf.PI * (i * 45f) / 180f), 0f, sampleRadius * Mathf.Sin(Mathf.PI * (i * 45f) / 180f));
            sp += base.Actor.transform.position;

            if(NavMesh.FindClosestEdge(sp, out NavMeshHit hit, -1) && Vector3.Dot(hit.normal, base.Pawn.Target.transform.position.DirectionTo(base.Actor.transform.position).normalized) < -.5f)
                candidates.Add(hit.position);
        }

        candidates.OrderBy(v => v.DistanceTo(base.Actor.transform.position));

        if (debugMode)
        {
            for (int i = 0; i < candidates.Count; i++)
                Debug.DrawLine(base.Actor.transform.position, candidates[i], Color.yellow, 1f);

            Debug.DrawLine(base.Actor.transform.position, candidates.First(), Color.green, 1f);
        }

        if (candidates.Count > 0)
            coverPosition = candidates.First();
        else
            coverPosition = base.Actor.transform.position;

        base.Actor.Raise(ActorEvent.SetTargetPosition, coverPosition);
        base.Actor.Raise(ActorEvent.SetInputModifier, 1f);
    }
    public override void Tick()
    {
        base.Tick();
        base.Actor.Raise(ActorEvent.SetTargetRotation, Quaternion.LookRotation(base.Actor.transform.position.DirectionTo(base.Pawn.Target.FocusPoint.position), Vector3.up));
        base.Actor.Raise(ActorEvent.SetLookAtPosition, base.Pawn.Target.FocusPoint.position);

        if (base.Actor.transform.position.DistanceTo(coverPosition) < 2f)
        {
            if (base.Pawn.CanSeeTarget)
                base.TransitionTo<AIAttackState>();
            else
                base.TransitionTo<AISearchState>();
        }
    }
    public override void Exit()
    {
    }
}
