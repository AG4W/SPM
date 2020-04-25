using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

using System.Collections.Generic;
using System;

public class HumanoidPawn : HumanoidActor, IForceAffectable
{
    [Range(1f, 100)][SerializeField]float sightRange = 30f;
    [Range(-1f, 1f)][SerializeField]float fieldOfView = 0f;

    [SerializeField]float rotationSpeed = 5f;

    [Range(0f, 1f)][SerializeField]float accuracy = .5f;

    [SerializeField]LayerMask sightMask;

    [SerializeField]GameObject model;
    [SerializeField]GameObject ragdoll;

    NavMeshAgent agent;

    protected float DistanceToTarget { get { return Vector3.Distance(this.transform.position, this.Target.transform.position); } }
    protected float AngleToTarget { get { return Vector3.Dot(this.transform.forward, this.HeadingToTarget.normalized); } }

    public float Accuracy { get { return accuracy; } }
    public Vector3 HeadingToTarget { get { return this.Target.transform.position - this.transform.position; } }
    public Vector3 TargetPosition { get; private set; }
    public Vector3 LastKnownPositionOfTarget { get; private set; }

    public bool CanSeeTarget { get; private set; }
    public bool HasBeenAlerted { get; private set; } = false;

    public HumanoidActor Target { get; private set; }

    protected override void Initalize()
    {
        base.Initalize();

        this.Subscribe(ActorEvent.SetActorTargetPosition, SetTargetPosition);

        this.Subscribe(ActorEvent.UpdateAITargetStatus, UpdateCanSeeTargetStatus);
        this.Subscribe(ActorEvent.UpdateActorAlertStatus, UpdateAlertedStatus);

        this.Target = FindObjectOfType<PlayerActor>();

        this.agent = this.GetComponent<NavMeshAgent>();
        this.agent.updatePosition = false;
        this.agent.updateRotation = false;
        this.agent.avoidancePriority = Random.Range(1, 99);

        this.TargetPosition = this.transform.position;
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
            },
            typeof(AIIdleState));
    }

    protected override void Update()
    {
        base.Update();

        this.agent.speed = this.Animator.velocity.magnitude;

        UpdateRotation();
        UpdateInput();

        if (HasBeenAlerted && CanSeeTarget)
            UpdateLastKnownPositionOfTarget();

        Debug.DrawLine(this.transform.position, this.TargetPosition, Color.blue);
        Debug.DrawLine(base.FocusPoint.position, this.Target.FocusPoint.position, CanSeeTarget ? Color.green : Color.red);
    }

    void SetTargetPosition(object[] args)
    {
        float distance = 2f;
        NavMesh.SamplePosition((Vector3)args[0], out NavMeshHit hit, distance, -1);

        for (int i = 0; i < 5; i++)
        {
            distance += 2f;
            NavMesh.SamplePosition((Vector3)args[0], out hit, distance, -1);

            if (hit.hit)
                break;
        }

        if (hit.hit)
            this.TargetPosition = hit.position;
        else
            this.TargetPosition = this.transform.position;

        this.agent.SetDestination(this.TargetPosition);
    }

    void UpdateRotation()
    {
        Vector3 dir;

        if (CanSeeTarget)
            dir = this.transform.position.DirectionTo(this.Target.transform.position);
        else
        {
            if (HasBeenAlerted)
                dir = this.transform.position.DirectionTo(LastKnownPositionOfTarget);
            else
            {
                if (this.transform.position.DistanceTo(this.TargetPosition) > 1f)
                    dir = this.transform.position.DirectionTo(this.TargetPosition);
                else
                    dir = this.transform.forward;
            }
        }

        dir.Normalize();

        if(dir != Vector3.zero)
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(dir, Vector3.up), rotationSpeed * Time.deltaTime);
    }
    void UpdateInput()
    {
        if (this.transform.position.DistanceTo(this.TargetPosition) > .5f)
            this.Raise(ActorEvent.SetActorTargetInput, this.agent.nextPosition.ToInput(this.transform));
        else
            this.Raise(ActorEvent.SetActorTargetInput, Vector3.zero);
    }
    void UpdateLastKnownPositionOfTarget()
    {
        this.LastKnownPositionOfTarget = this.Target.FocusPoint.position;
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
