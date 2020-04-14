using UnityEngine;
using UnityEngine.AI;

using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class Pawn : Actor
{
    string debugText;

    [SerializeField]float inputInterpolationSpeed = 2.5f;

    [Range(0f, 100f)][SerializeField]float detectionRange = 50f;
    [Range(-1f, 1f)][SerializeField]float detectionFieldOfView = 0f;

    [SerializeField]float aiRefreshRate = 1f;
    [SerializeField]float forceInfluenceModifier = .25f;
    float refreshTimer;

    [SerializeField]WeaponController weapon;
    [SerializeField]Collider hitbox;

    [SerializeField]bool startAlerted = false;
    [SerializeField]bool displayDebugText = true;

    protected string TargetDebugStatus { get; set; }
    protected string ActionDebugStatus { get; set; }

    protected float DistanceToTarget { get { return Vector3.Distance(this.transform.position, this.Target.transform.position); } }
    protected float DistanceToDesiredPosition { get { return Vector3.Distance(this.transform.position, this.DesiredPosition); } }
    protected float AngleToTarget { get { return Vector3.Dot(this.HeadingToTarget.normalized, this.transform.forward); } }

    protected Vector3 HeadingToTarget { get { return this.Target.transform.position - this.transform.position; } }
    protected Vector3 HeadingFromTarget { get { return this.transform.position - this.Target.transform.position; } }
    protected Vector3 DesiredPosition { get; private set; }
    protected Vector3 TargetInput { get; set; }
    protected Vector3 ActualInput { get; private set; }

    protected Actor Target { get; private set; }
    protected NavMeshAgent Agent { get; private set; }

    protected SortedDictionary<float, Vector3> Memory { get; } = new SortedDictionary<float, Vector3>();
    protected bool IsAlert { get; set; }
    protected bool HasSeenTarget { get; private set; }

    public float ForceInfluenceModifier { get { return forceInfluenceModifier; } }
    public Vector3 Velocity { get; protected set; }
    public Vector3 ModifiedVelocity { get; protected set; }
    public bool CanSeeTarget { get; protected set; }

    protected override void Initalize()
    {
        base.Initalize();
        this.Agent = this.GetComponent<NavMeshAgent>();
        this.Target = FindObjectOfType<LocomotionController>().GetComponent<Actor>();

        if (this.Target == null)
            Debug.LogError(this.name + " could not find player character, did you forget to drag and drop the prefab into your scene?", this.gameObject);

        GlobalEvents.Subscribe(GlobalEvent.NoiseCreated, OnNoiseCreated);

        if (!startAlerted)
            this.IsAlert = false;

        SetDesiredPosition(this.transform.position);
    }

    void Update()
    {
        if (this.Target == null)
            return;

        if (displayDebugText)
        {
            debugText =
            "Distance: " + this.DistanceToTarget + "\n" +
            "Angle: " + this.AngleToTarget + "\n" +
            TargetDebugStatus + "\n" +
            ActionDebugStatus;

            Debug.DrawLine(base.FocusPoint.position, this.Target.FocusPoint.position, CanSeeTarget ? Color.green : Color.red);
            Debug.DrawLine(base.FocusPoint.position, this.DesiredPosition, Color.blue);
        }

        refreshTimer += Time.deltaTime;

        if (refreshTimer >= aiRefreshRate)
        {
            refreshTimer = 0f;
            UpdateTargetStatus();
        }

        if (this.DistanceToDesiredPosition < 1f)
            OnDesiredPositionReached();

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
        var textSize = GUI.skin.label.CalcSize(new GUIContent(debugText));
        GUI.Label(new Rect(position.x, Screen.height - position.y, textSize.x, textSize.y), debugText);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Hit " + this.name + " for " + collision.relativeVelocity.magnitude + " damage");
        base.Health.Update(-collision.relativeVelocity.magnitude);
    }

    protected virtual void UpdateInput()
    {
        this.TargetInput = this.Agent.nextPosition.ToInput(this.transform).normalized;
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
        this.Animator.SetBool("isAlert", IsAlert);
    }

    protected virtual void UpdateTargetStatus()
    {
        if (this.DistanceToTarget > detectionRange)
        {
            TargetDebugStatus = "<color=red>Target outside of detection range</color>";
            this.CanSeeTarget = false;
            return;
        }
        if (this.AngleToTarget < detectionFieldOfView)
        {
            TargetDebugStatus = "<color=red>Target outside of field of view</color>";
            this.CanSeeTarget = false;
            return;
        }
        if (Physics.Linecast(base.FocusPoint.position, this.Target.FocusPoint.position, out RaycastHit hit, 1 << LayerMask.GetMask("Player", "Enemy")))
        {
            Debug.DrawLine(base.FocusPoint.position, hit.point, Color.magenta);

            TargetDebugStatus = "<color=red>Cant see target due to line of sight obstruction</color>";
            this.CanSeeTarget = false;
            return;
        }

        if (!this.HasSeenTarget)
            this.HasSeenTarget = true;

        TargetDebugStatus = "<color=green>Can see target!</color>";
        this.CanSeeTarget = true;
        this.IsAlert = true;
        Memory[this.DistanceToTarget] = this.Target.transform.position;
    }
    /// <summary>
    /// Set desired position of pawn.
    /// NavMeshAgent will attempt to navigate to this point as close as possible.
    /// </summary>
    /// <param name="position"></param>
    protected void SetDesiredPosition(Vector3 position)
    {
        this.DesiredPosition = position;
        this.Agent.SetDestination(this.DesiredPosition);
    }

    protected virtual void Attack()
    {
        weapon.Shoot(this.Target.FocusPoint.position);
    }

    protected virtual void OnDesiredPositionReached()
    {

    }
    protected virtual void OnNoiseCreated(object[] args)
    {
        if (this.Memory.Count > 10)
            this.Memory.Clear();

        Vector3 p = (Vector3)args[0];
        this.Memory.Add(Vector3.Distance(this.transform.position, p), p);
        this.IsAlert = true;
    }

    protected override void OnHealthChanged(float current)
    {
        base.OnHealthChanged(current);
        this.IsAlert = true;
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

