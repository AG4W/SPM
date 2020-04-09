using UnityEngine;

public class NonPlayerActor : Actor
{
    [SerializeField] protected float detectionRange;
    [Range(-0f, 180f)] [SerializeField] protected float detectionFieldOfView = 45f;
    [SerializeField] protected float speed;
    [SerializeField] float targetUpdatRate;
    [SerializeField] protected Actor target;
    [SerializeField] WeaponController weapon;

    float targetTimer;
    protected float smallWidth = 0.1f;
    protected float distanceToTarget;

    protected Vector3 targetPoint;

    public bool HasSeenTarget { get; protected set; }

    public Vector3 Velocity { get; protected set; }

    protected override void Initalize()
    {
        base.Initalize();
    }

    // Update is called once per frame
    void Update()
    {
        targetTimer += Time.deltaTime;

        if (targetTimer >= targetUpdatRate)
        {
            targetTimer = 0f;

            PlotMove();
        }

        UpdateVelocity();
        UpdateRotation();
        UpdateTransform();
    }

    protected virtual void UpdateVelocity()
    {

    }

    protected virtual void UpdateRotation()
    {

    }

    void UpdateTransform()
    {
        this.transform.position += this.Velocity;
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
}

