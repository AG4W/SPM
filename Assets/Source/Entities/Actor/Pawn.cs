using UnityEngine;
using UnityEngine.AI;

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

    [SerializeField]Actor target;

    [SerializeField]WeaponController weapon;
    [SerializeField]Collider hitbox;

    [SerializeField]bool displayDebugText = true;

    //this will be refactored later

    protected float DistanceToTarget { get { return Vector3.Distance(this.transform.position, target.transform.position); } }
    protected float AngleToTarget { get { return Vector3.Dot(this.HeadingToTarget.normalized, this.transform.forward); } }
    protected Vector3 HeadingToTarget { get { return target.transform.position - this.transform.position; } }
    protected Vector3 DesiredPosition { get; set; }
    protected Vector3 TargetInput { get; set; }
    protected Vector3 ActualInput { get; private set; }
    protected Actor Target { get { return target; } }
    protected NavMeshAgent Agent { get; private set; }

    public float ForceInfluenceModifier { get { return forceInfluenceModifier; } }
    public Vector3 Velocity { get; protected set; }
    public Vector3 ModifiedVelocity { get; protected set; }
    public bool CanSeeTarget { get; protected set; }

    protected override void Initalize()
    {
        base.Initalize();
        this.Agent = this.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (target == null)
            return;

        if (displayDebugText)
        {
            debugText =
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

        var position = Camera.main.WorldToScreenPoint(this.transform.position + (Vector3.up * 2f));
        var textSize = GUI.skin.label.CalcSize(new GUIContent(debugText));
        GUI.Label(new Rect(position.x, Screen.height - position.y, textSize.x, textSize.y), debugText);
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
            debugText += "<color=red>Target outside of detection range</color>";
            this.CanSeeTarget = false;
        }
        else if (this.AngleToTarget < detectionFieldOfView)
        {
            debugText += "<color=red>Target outside of field of view</color>";
            this.CanSeeTarget = false;
        }
        else
        {
            debugText += "<color=green>Can see target!</color>";
            this.CanSeeTarget = true;
        }
        //if (Physics.Linecast(this.transform.position, targetPoint))
        //{
        //    debugText += "<color=red>Cant see target due to line of sight obstruction</color>";
        //    return false;
        //}
    }
    protected virtual void UpdateDestination()
    {

    }

    protected virtual void Attack()
    {
        weapon.Shoot(this.target.FocusPoint.position);
    }

    protected override void OnHealthZero()
    {
        base.OnHealthZero();

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

