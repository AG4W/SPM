using UnityEngine;

public class NonPlayerActor : Actor
{
    [SerializeField] protected float detectionRange;
    [Range(-0f, 180f)] [SerializeField] protected float detectionFieldOfView = 45f;
    [SerializeField] protected float speed;
    [SerializeField] float targetUpdatRate;
    [SerializeField]float forceInfluenceModifier = .25f;
    [SerializeField] protected Actor target;
    
    [SerializeField] WeaponController weapon;

    float targetTimer;
    protected float smallWidth = 0.1f;
    protected float distanceToTarget;

    Vector3 modifiedVelocity;

    protected Vector3 targetPoint;

    public float ForceInfluenceModifier { get { return forceInfluenceModifier; } }

    public Vector3 Velocity { get; protected set; }

    public bool HasSeenTarget { get; protected set; }

    protected override void Initalize()
    {
        base.Initalize();
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
            return;

        targetTimer += Time.deltaTime;

        if (targetTimer >= targetUpdatRate)
        {
            targetTimer = 0f;

            SearchForTarget();
            PlotMove();
        }

        UpdateVelocity();
        UpdateRotation();
        UpdateTransform();

        //AttemptFire();

        modifiedVelocity = Vector3.zero;
    }

    protected virtual void UpdateVelocity()
    {
    }
    protected virtual void UpdateRotation()
    {

    }
    void UpdateTransform()
    {
        this.transform.position += this.Velocity + modifiedVelocity;
    }

    protected virtual void SearchForTarget()
    {
        if (HasSeenTarget)
            return;

        if (CanSeeTarget())
            HasSeenTarget = true;
    }
    protected virtual void PlotMove()
    {

    }

    protected void AttemptFire()
    {
        if (CanSeeTarget())
            weapon.Shoot(targetPoint);
    }

    protected bool CanSeeTarget()
    {
        if (distanceToTarget > detectionRange)
            return false;
        if (Vector3.Angle(this.transform.forward, targetPoint - this.transform.position) > detectionFieldOfView)
            return false;
        if (Physics.Linecast(this.transform.position, targetPoint))
            return false;

        return true;
    }
    public void ModifyVelocity(Vector3 change)
    {
        this.modifiedVelocity += change;
    }
}

