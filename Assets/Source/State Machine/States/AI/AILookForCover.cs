using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "State/AI/Find Cover")]
public class AILookForCover : AIBaseLocomotionState
{
    [SerializeField]float searchRadius = 5f;
    [SerializeField]float coverBufferDistance = 1f;

    [SerializeField]LayerMask mask;

    Vector3 position;

    public override void Enter()
    {
        base.Enter();

        position = GetSafePosition();

        base.Actor.Raise(ActorEvent.SetActorTargetPosition, position);
        base.Actor.Raise(ActorEvent.SetActorMovementMode, MovementMode.Sprint);
    }
    public override void Tick()
    {
        base.Tick();

        if (base.Actor.transform.position.DistanceTo(position) < .5f)
            base.TransitionTo<AIEngageTarget>();
    }
    public override void Exit()
    {
        base.Actor.Raise(ActorEvent.SetActorMovementMode, MovementMode.Jog);
    }

    Vector3 GetSafePosition()
    {
        Vector3 pos = base.Actor.transform.position;
        float d = Mathf.Infinity;
        
        //find suitable positions in circle around
        for (int i = 0; i < 8; i++)
        {
            Vector3 samplePosition = base.Actor.transform.position.PointOnCircle(searchRadius, i * 45f);
            //yuck
            if (!ValidatePosition(samplePosition))
                continue;

            NavMesh.FindClosestEdge(samplePosition, out NavMeshHit hit, -1);
            float dot = Vector3.Dot(base.Pawn.Target.transform.position, hit.normal);

            if (dot < d)
            {
                d = dot;
                pos = hit.position;
            }

            //Debug.DrawLine(base.Actor.transform.position, samplePosition, Color.yellow, 1f);
        }

        return pos;
    }
    //make sure its actually cover and not just end of the map somewhere
    bool ValidatePosition(Vector3 prospect)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 sampleDirection = prospect.PointOnCircle(searchRadius, i * 45f);

            if (Physics.Raycast(prospect + Vector3.up, prospect.DirectionTo(sampleDirection).normalized, 2f, mask))
            {
                //Debug.DrawRay(prospect + Vector3.up, prospect.DirectionTo(sampleDirection).normalized * 2f, Color.green, 1f);
                return true;
            }
            //else
            //    Debug.DrawRay(prospect + Vector3.up, prospect.DirectionTo(sampleDirection).normalized * 2f, Color.red, 1f);
        }

        return false;
    }
}
