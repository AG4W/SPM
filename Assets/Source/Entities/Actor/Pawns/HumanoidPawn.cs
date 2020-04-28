using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

using System.Collections.Generic;
using System;

public class HumanoidPawn : HumanoidActor, IForceAffectable, IAICombatMode
{
    [SerializeField]AICombatMode mode = AICombatMode.Cautious;

    [Range(1f, 100)][SerializeField]float sightRange = 30f;
    [Range(-1f, 1f)][SerializeField]float fieldOfView = 0f;

    [Range(0f, 1f)][SerializeField]float accuracy = .5f;

    [SerializeField]LayerMask sightMask;

    [SerializeField]GameObject model;
    [SerializeField]GameObject ragdoll;

    NavMeshAgent agent;

    //flytta dessa uppåt i hierarkin
    Quaternion targetRotation;
    Quaternion actualRotation;

    protected float DistanceToTarget { get { return Vector3.Distance(this.transform.position, this.Target.transform.position); } }
    protected float AngleToTarget { get { return Vector3.Dot(this.transform.forward, this.HeadingToTarget.normalized); } }

    public float Accuracy { get { return accuracy; } }
    public Vector3 HeadingToTarget { get { return this.Target.transform.position - this.transform.position; } }
    public Vector3 LastKnownPositionOfTarget { get; private set; }

    public bool CanSeeTarget { get; private set; }

    public GameObject RagdollPrefab => ragdoll;
    public Actor Target { get; private set; }
    public AICombatMode Mode => mode;

    protected override void Initalize()
    {
        this.agent = this.GetComponent<NavMeshAgent>();
        this.agent.updatePosition = false;
        this.agent.updateRotation = false;
        this.agent.avoidancePriority = Random.Range(1, 99);
        targetRotation = this.transform.rotation;

        base.Initalize();

        this.Subscribe(ActorEvent.SetTargetPosition, SetTargetPosition);
        this.Subscribe(ActorEvent.SetTargetRotation, SetTargetRotation);
        this.Subscribe(ActorEvent.SetLastKnownPositionOfTarget, SetLastKnownPositionOfTarget);

        this.Subscribe(ActorEvent.UpdateAITargetStatus, UpdateCanSeeTargetStatus);
        this.Subscribe(ActorEvent.UpdateAlertStatus, UpdateAlertedStatus);

        this.Subscribe(ActorEvent.OnAIForceAffectStart, OnForceAffectStart);
        this.Subscribe(ActorEvent.OnAIForceAffectEnd, OnForceAffectEnd);

        this.Target = FindObjectOfType<PlayerActor>();
    }
    protected override StateMachine InitializeStateMachine()
    {
        return new StateMachine(this,
            Resources.LoadAll<State>(base.Path),
            new Dictionary<Type, object>
            {
                [typeof(Animator)] = this.GetComponent<Animator>(),
                [typeof(Actor)] = this,
                [typeof(WeaponController)] = base.WeaponController,
                [typeof(NavMeshAgent)] = this.agent
            },
            typeof(AIIdleState));
    }

    protected override void Update()
    {
        base.Update();

        //this.agent.speed = this.Animator.velocity.magnitude;
        this.agent.nextPosition = this.transform.position;
        this.transform.rotation = actualRotation;

        if (this.transform.position.DistanceTo(this.agent.destination) < this.agent.radius)
            this.Raise(ActorEvent.SetTargetInput, Vector3.zero);

        Debug.DrawLine(this.transform.position, this.agent.destination, Color.blue);
        Debug.DrawLine(base.FocusPoint.position, this.Target.FocusPoint.position, CanSeeTarget ? Color.green : Color.red);
    }
    protected override void Interpolate()
    {
        base.Interpolate();
        actualRotation = Quaternion.Slerp(actualRotation, targetRotation, 2.5f * Time.deltaTime);
    }

    void SetTargetPosition(object[] args)
    {
        float distance = 2f;
        NavMesh.SamplePosition((Vector3)args[0], out NavMeshHit hit, distance, -1);

        for (int i = 0; i < 10; i++)
        {
            distance += 2f;
            NavMesh.SamplePosition((Vector3)args[0], out hit, distance, -1);

            if (hit.hit && this.transform.position.DistanceTo(hit.position) > this.agent.radius)
            {
                this.agent.SetDestination(hit.position);
                break;
            }
        }
    }
    void SetTargetRotation(object[] args) => targetRotation = (Quaternion)args[0];
    void SetLastKnownPositionOfTarget(object[] args)
    {
        this.LastKnownPositionOfTarget = (Vector3)args[0];
    }

    void UpdateCanSeeTargetStatus(object[] args)
    {
        if(this.DistanceToTarget > sightRange)
        {
            this.CanSeeTarget = false;
            return;
        }
        if(this.AngleToTarget < fieldOfView)
        {
            this.CanSeeTarget = false;
            return;
        }
        if(Physics.Linecast(base.FocusPoint.position, this.Target.FocusPoint.position, sightMask))
        {
            this.CanSeeTarget = false;
            return;
        }

        this.CanSeeTarget = true;
    }
    void UpdateAlertedStatus(object[] args)
    {
    }

    void OnForceAffectStart(object[] args)
    {
        model.SetActive(false);

        GameObject rd = Instantiate(ragdoll, this.transform.position, this.transform.rotation, this.transform);
    }
    void OnForceAffectEnd(object[] args)
    {
    }

    protected override void OnHealthZero()
    {
        base.OnHealthZero();

        Destroy(this.GetComponent<Collider>());
        Destroy(model);

        GameObject rd = Instantiate(ragdoll, this.transform.position, this.transform.rotation, null);

        //instansiera och lägg till wrappers för varje rigidbody i samma loop
        foreach (Rigidbody rb in rd.GetComponentsInChildren<Rigidbody>())
            rb.gameObject.AddComponent<RagdollForceWrapper>();

        Destroy(this.gameObject);
    }

    void IForceAffectable.ModifyVelocity(Vector3 change)
    {
        base.Velocity += change;
    }
    void IForceAffectable.SetVelocity(Vector3 velocity)
    {
        base.Velocity = velocity;
    }
}
