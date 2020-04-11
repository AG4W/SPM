using UnityEngine;
using UnityEngine.AI;

using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class Pawn : Actor
{

    [SerializeField]float inputInterpolationSpeed = 2.5f;

    [Range(0f, 100f)][SerializeField]float detectionRange = 50f;
    [Range(-1f, 1f)][SerializeField]float detectionFieldOfView = 0f;

    [SerializeField]float aiRefreshRate = 1f;
    [SerializeField]float forceInfluenceModifier = .25f;
    float refreshTimer;

    [SerializeField]Actor target;

    [SerializeField]WeaponController weapon;
    [SerializeField]Collider hitbox;

    [SerializeField]bool displayDebugText = true;

    protected string DebugText { get; set; }

    protected float DistanceToTarget { get { return Vector3.Distance(this.transform.position, target.transform.position); } }
    protected float AngleToTarget { get { return Vector3.Dot(this.HeadingToTarget.normalized, this.transform.forward); } }

    protected Vector3 HeadingToTarget { get { return target.transform.position - this.transform.position; } }
    protected Vector3 HeadingFromTarget { get { return this.transform.position - target.transform.position; } }
    protected Vector3 DesiredPosition { get; set; }
    protected Vector3 TargetInput { get; set; }
    protected Vector3 ActualInput { get; private set; }

    protected Actor Target { get { return target; } }
    protected NavMeshAgent Agent { get; private set; }

    protected SortedDictionary<float, Vector3> Memory { get; } = new SortedDictionary<float, Vector3>();

    public float ForceInfluenceModifier { get { return forceInfluenceModifier; } }
    public Vector3 Velocity { get; protected set; }
    public Vector3 ModifiedVelocity { get; protected set; }
    public bool CanSeeTarget { get; protected set; }

    protected override void Initalize()
    {
        base.Initalize();

        this.Agent = this.GetComponent<NavMeshAgent>();

        GlobalEvents.Subscribe(GlobalEvent.NoiseCreated, OnNoiseCreated);
    }

    void Update()
    {
        if (target == null)
            return;

        if (displayDebugText)
        {
            DebugText =
            "Distance: " + this.DistanceToTarget + "\n" +
            "Angle: " + this.AngleToTarget + "\n";

            Debug.DrawLine(this.transform.position + Vector3.up, target.transform.position + Vector3.up, CanSeeTarget ? Color.green : Color.red);
        }

        refreshTimer += Time.deltaTime;

        if (refreshTimer >= aiRefreshRate)
        {
            refreshTimer = 0f;

            UpdateTargetStatus();
            UpdateDestination();
        }

        UpdateInput();
        UpdateVelocity();
        UpdateRotation();
        UpdateTransform();

        if (CanSeeTarget)
            Attack();

        UpdateAnimator();

        this.ModifiedVelocity = Vector3.zero;
    }
    void OnGUI()
    {
        if (!displayDebugText)
            return;

        var position = Camera.main.WorldToScreenPoint(this.transform.position + (Vector3.up * 3f));
        var textSize = GUI.skin.label.CalcSize(new GUIContent(DebugText));
        GUI.Label(new Rect(position.x, Screen.height - position.y, textSize.x, textSize.y), DebugText);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit pawn for " + collision.relativeVelocity.magnitude + " damage");
        base.Health.Update(-collision.relativeVelocity.magnitude);
    }

    protected virtual void UpdateInput()
    {
        this.ActualInput = Vector3.Lerp(this.ActualInput, this.TargetInput, inputInterpolationSpeed * Time.deltaTime);
    }
    protected virtual void UpdateVelocity()
    {
    }
    protected virtual void UpdateRotation()
    {
    }
    protected virtual void UpdateTransform()
    {
        //this.transform.position += this.Velocity + modifiedVelocity;
        this.transform.position += this.Velocity + this.ModifiedVelocity;
    }
    protected virtual void UpdateAnimator()
    {

    }

    protected virtual void UpdateTargetStatus()
    {
        if (this.DistanceToTarget > detectionRange)
        {
            DebugText += "<color=red>Target outside of detection range</color>\n";
            this.CanSeeTarget = false;
            return;
        }
        if (this.AngleToTarget < detectionFieldOfView)
        {
            DebugText += "<color=red>Target outside of field of view</color>\n";
            this.CanSeeTarget = false;
            return;
        }
        //if (Physics.Linecast(base.FocusPoint.position, this.Target.FocusPoint.position))
        //{
        //    debugText += "<color=red>Cant see target due to line of sight obstruction</color>\n";
        //    this.CanSeeTarget = false;
        //    return;
        //}

        DebugText += "<color=green>Can see target!</color>";
        this.CanSeeTarget = true;
        Memory[this.DistanceToTarget] = this.Target.transform.position;
    }
    protected virtual void UpdateDestination()
    {

    }

    protected virtual void Attack()
    {
        weapon.Shoot(this.target.FocusPoint.position);
    }

    void OnNoiseCreated(object[] args)
    {
        if (this.Memory.Count > 10)
            this.Memory.Clear();

        Vector3 p = (Vector3)args[0];
        this.Memory.Add(Vector3.Distance(this.transform.position, p), p);
    }

    protected override void OnHealthZero()
    {
        base.OnHealthZero();

        GlobalEvents.Unsubscribe(GlobalEvent.NoiseCreated, OnNoiseCreated);

        Destroy(this);
        Destroy(this.Animator);
        Destroy(this.Agent);
        Destroy(hitbox);
    }

    public void ModifyVelocity(Vector3 change)
    {
        this.ModifiedVelocity += change;
    }
}

