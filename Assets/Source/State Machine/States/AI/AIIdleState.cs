using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Relaxed Idle")]
public class AIIdleState : AIBaseLocomotionState
{
    [SerializeField]float alertOthersDistance = 15f;

    float idleIndex;
    float idleTime;
    float timer = 0f;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        base.Actor.Subscribe(ActorEvent.OnActorHealthChanged, (object[] args) => {
            if (!base.IsActiveState)
                return;

            //alert others
            GlobalEvents.Raise(GlobalEvent.AlertOthers, base.Actor, alertOthersDistance);
            base.TransitionTo<AISearchState>();
        });

        GlobalEvents.Subscribe(GlobalEvent.AlertOthers, (object[] args) => {
                                        //dont alert myself, derp
            if (!base.IsActiveState || ((Actor)args[0]) == base.Actor)
                return;

            if (base.Actor.transform.position.DistanceTo(((Actor)args[0]).transform.position) <= (float)args[1])
                base.TransitionTo<AISearchState>();

            Debug.DrawLine(base.Actor.transform.position, ((Actor)args[0]).transform.position, base.Actor.transform.position.DistanceTo(((Actor)args[0]).transform.position) <= (float)args[1] ? Color.green : Color.red, 2f);
        });
    }

    public override void Enter()
    {
        base.Enter();

        idleIndex = Random.Range(0f, 1f);
        idleTime = Random.Range(5f, 15f);
        timer = 0f;

        base.Actor.Raise(ActorEvent.SetTargetPosition, base.Actor.transform.position);
        base.Actor.Raise(ActorEvent.SetTargetInput, Vector3.zero);

        base.Get<Animator>().SetBool("isIdling", true);
    }
    public override void Tick()
    {
        base.Tick();

        timer += Time.deltaTime;

        if(timer >= idleTime)
        {
            idleIndex = Random.Range(0f, 1f);
            idleTime = Random.Range(5f, 15f);
            timer = 0f;
        }

        base.Get<Animator>().SetFloat("randomIndex", idleIndex, .25f, Time.deltaTime);

        if (base.Pawn.CanSeeTarget)
        {
            //alert other nearby pawns
            GlobalEvents.Raise(GlobalEvent.AlertOthers, base.Actor, alertOthersDistance);
            base.TransitionTo<AIAttackState>();
        }
    }
    public override void Exit()
    {
        base.Get<Animator>().SetBool("isIdling", false);
    }
}
