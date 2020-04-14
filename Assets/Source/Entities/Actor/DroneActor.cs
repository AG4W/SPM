using UnityEngine;

public class DroneActor : Pawn
{
    [SerializeField]float distanceFromTarget;
    [SerializeField]float speed;

    [SerializeField]Vector3[] idlePath;

    protected override void Initalize()
    {
        Vector3[] temp = new Vector3[3];
        temp[0] = this.transform.position;
        temp[1] = idlePath[0];
        temp[2] = idlePath[1];

        idlePath = temp;

        base.Initalize();
    }

    protected override void UpdateVelocity()
    {
        base.UpdateVelocity();

        if (Vector3.Distance(this.transform.position, base.DesiredPosition) > .1f)
            base.Velocity = this.transform.forward * speed * Time.deltaTime;
    }
    protected override void UpdateRotation()
    {
        base.UpdateRotation();
        this.transform.LookAt(base.Target.FocusPoint);
    }

    protected override void OnDesiredPositionReached()
    {
        base.OnDesiredPositionReached();
        base.SetDesiredPosition(base.DesiredPosition == idlePath[0] ? idlePath[1] : idlePath[0]);
    }

    void OnCollisionStay(Collision collision)
    {
        base.ModifyVelocity(collision.rigidbody.velocity * Time.deltaTime);
    }
}


/*
        if (hasSeenTarget)
        {
            Vector3 heading = targetPoint - transform.position;
distanceToTarget = Vector3.Distance(transform.position, targetPoint);
            if (distanceToTarget<detectionRange && distanceToTarget> distanceFromTarget + smallWidth)
            {
                transform.position += heading.normalized* speed * Time.deltaTime;
            }
            else if (distanceToTarget<distanceFromTarget - smallWidth)
            {
                transform.position += -heading.normalized* speed * Time.deltaTime;
            }

            transform.LookAt(target.FocusPoint.position);

            AttemptFire();
        }
        else
        {
            if (CanSeeTarget())
            {
                hasSeenTarget = true;
            }
        }
        */