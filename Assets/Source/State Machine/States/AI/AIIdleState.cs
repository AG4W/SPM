using UnityEngine;

[CreateAssetMenu(menuName = "State/AI/Relaxed Idle")]
public class AIIdleState : AIBaseLocomotionState
{
    float idleIndex;
    float idleTime;
    float timer = 0f;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        base.Actor.Subscribe(ActorEvent.OnActorHealthChanged, (object[] args) => {
            if (!base.IsActiveState)
                return;

            base.TransitionTo<AIHuntState>();
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
            base.TransitionTo<AIAttackState>();
    }
    public override void Exit()
    {
        base.Get<Animator>().SetBool("isIdling", false);
    }
}
