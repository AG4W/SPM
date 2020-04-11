using UnityEngine;

public class Pawn : Actor
{
    [Range(0f, 100f)][SerializeField]float detectionRange = 50f;
    [Range(-0f, 180f)][SerializeField]float detectionFieldOfView = 45f;

    [SerializeField]float aiRefreshRate = 1f;
    [SerializeField]float forceInfluenceModifier = .25f;

    [SerializeField]Actor target;
    
    [SerializeField]WeaponController weapon;

    float refreshTimer;

    protected Vector3 targetPoint;

    public float ForceInfluenceModifier { get { return forceInfluenceModifier; } }

    public Vector3 Velocity { get; protected set; }
    public Vector3 ModifiedVelocity { get; protected set; }

    protected Actor Target { get { return target; } }

    public bool HasSeenTarget { get; protected set; }

    protected override void Initalize()
    {
        base.Initalize();
    }

    void Update()
    {
        if (target == null)
            return;

        refreshTimer += Time.deltaTime;

        if (refreshTimer >= aiRefreshRate)
        {
            refreshTimer = 0f;

            UpdateTarget();
            UpdateDestination();
        }

        UpdateVelocity();
        UpdateRotation();
        UpdateTransform();
        UpdateAnimator();

        this.ModifiedVelocity = Vector3.zero;
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
    }
    protected virtual void UpdateAnimator()
    {

    }

    protected virtual void UpdateTarget()
    {
        if (HasSeenTarget)
            return;

        if (CanSeeTarget())
            HasSeenTarget = true;
    }
    protected virtual void UpdateDestination()
    {

    }

    protected void AttemptFire()
    {
        if (CanSeeTarget())
            weapon.Shoot(targetPoint);
    }

    protected bool CanSeeTarget()
    {
        if (Vector3.Distance(target.transform.position, this.transform.position) > detectionRange)
            return false;
        if (Vector3.Angle(this.transform.forward, targetPoint - this.transform.position) > detectionFieldOfView)
            return false;
        if (Physics.Linecast(this.transform.position, targetPoint))
            return false;

        return true;
    }
    public void ModifyVelocity(Vector3 change)
    {
        this.ModifiedVelocity += change;
    }
}

