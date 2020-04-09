using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneActor : NonPlayerActor
{
    [SerializeField] float distanceFromTarget;

    [SerializeField] Vector3[] idlePath;

    

    protected override void Initalize()
    {
        base.Initalize();
    }

    protected override void UpdateVelocity()
    {
        base.UpdateVelocity();
        if (Vector3.Distance(this.transform.position, targetPoint) > 0.5f)
        {
            base.Velocity = this.transform.forward * speed * Time.deltaTime;
        }
    }

    protected override void UpdateRotation()
    {
        base.UpdateRotation();

        this.transform.LookAt(targetPoint);
    }

    protected override void PlotMove()
    {
        base.PlotMove();
        //if (CanSeeTarget()) {
        //    targetPoint = (this.transform.position - target.FocusPoint.position).normalized * distanceFromTarget;
        //}
        //else
        //{
        //}
        targetPoint = idlePath.Random();

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