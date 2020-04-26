using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Find Cover")]
public class AILookForCover : AIBaseLocomotionState
{
    [SerializeField]float searchRadius = 5f;
    [SerializeField]float updateRate = 2.5f;

    [SerializeField]LayerMask mask;

    float timer;

    Vector3 position;

    public override void Enter()
    {
        base.Enter();

        FindCover();
        //check if we couldnt find valid cover

        base.Actor.Raise(ActorEvent.SetActorTargetPosition, position);
        base.Actor.Raise(ActorEvent.SetActorMovementMode, MovementMode.Sprint);

        timer = 0f;
    }
    public override void Tick()
    {
        base.Tick();

        timer += Time.deltaTime;

        if(timer >= updateRate)
            FindCover();
    }
    public override void Exit()
    {
        base.Actor.Raise(ActorEvent.SetActorMovementMode, MovementMode.Jog);
    }

    void FindCover()
    {
        Collider[] candidates = Physics.OverlapSphere(base.Pawn.transform.position, searchRadius, mask);

        if (candidates.Length == 0)
            position = base.Pawn.transform.position;
        else
        {
            Collider bestZone = null;
            float dot = -Mathf.Infinity;

            //find best cover
            for (int i = 0; i < candidates.Length; i++)
            {
                //ignore cover that doesnt actually give cover
                if (!Physics.Linecast(candidates[i].transform.position + Vector3.up * .5f, base.Pawn.Target.transform.position + Vector3.up * .5f))
                    continue;

                float d = Vector3.Dot(candidates[i].transform.forward, base.Pawn.transform.position.DirectionTo(base.Pawn.Target.transform.position));

                if(d > dot)
                {
                    dot = d;
                    bestZone = candidates[i];
                }
            }

            //get a random position within the cover zone
            if (bestZone != null)
                position = bestZone.transform.position + new Vector3(
                    Random.Range(-bestZone.bounds.extents.x / 2, bestZone.bounds.extents.x / 2),
                    0f,
                    Random.Range(-bestZone.bounds.extents.z / 2, bestZone.bounds.extents.z / 2));
            //should never happen, but if we somehow cant find a larger dot than mathf.infinity
            else
                position = base.Pawn.transform.position;
        }
    }
}
