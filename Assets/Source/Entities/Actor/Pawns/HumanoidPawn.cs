using UnityEngine;

using System.Collections.Generic;
using System;

public class HumanoidPawn : HumanoidActor, IForceAffectable
{
    [Range(1f, 100)][SerializeField]float sightRange = 30f;
    [Range(-1f, 1f)][SerializeField]float fieldOfView = 0f;

    [SerializeField]float rotationSpeed = 5f;

    [Range(0f, 1f)][SerializeField]float accuracy = .5f;

    [SerializeField]GameObject model;
    [SerializeField]GameObject ragdoll;

    Quaternion targetRotation;

    protected float DistanceToTarget { get { return Vector3.Distance(this.transform.position, this.Target.transform.position); } }
    protected float AngleToTarget { get { return Vector3.Dot(this.transform.forward, this.HeadingToTarget.normalized); } }

    public float Accuracy { get { return accuracy; } }
    public Vector3 HeadingToTarget { get { return this.Target.transform.position - this.transform.position; } }

    public bool CanSeeTarget { get; private set; }

    public HumanoidActor Target { get; private set; }

    protected override void Initalize()
    {
        base.Initalize();

        this.Subscribe(ActorEvent.SetActorTargetRotation, SetTargetRotation);

        this.Subscribe(ActorEvent.UpdateAITargetStatus, UpdateCanSeeTargetStatus);
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
            },
            typeof(AIIdleState));
    }

    protected override void Update()
    {
        base.Update();

        UpdateRotation();
        Debug.DrawLine(base.FocusPoint.position, this.Target.FocusPoint.position, CanSeeTarget ? Color.green : Color.red);
    }

    void SetTargetRotation(object[] args)
    {
        if((Vector3)args[0] != Vector3.zero)
            targetRotation = Quaternion.LookRotation((Vector3)args[0], Vector3.up);
    }
    void UpdateRotation()
    {
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
        if(Physics.Linecast(base.FocusPoint.position, this.Target.FocusPoint.position))
        {

            this.CanSeeTarget = false;
            return;
        }

        this.CanSeeTarget = true;
    }

    protected override void OnHealthZero()
    {
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
