using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerActor : Actor
{
    [SerializeField] float detectionRange;
    [Range(-0f, 180f)] [SerializeField] float detectionFieldOfView = .25f;
    [SerializeField] float speed;
    [SerializeField] float distanceFromTarget;
    [SerializeField] float targetUpdatRate;
    [SerializeField] Actor target;
    [SerializeField] WeaponController weapon;

    private float targetTimer;
    private float smallWidth = 0.1f;
    private float distanceToTarget;
    private Vector3 targetPoint;
    private bool hasSeenTarget = false;



    protected override void Initalize()
    {
        base.Initalize();
        targetPoint = target.FocusPoint.position;
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

        if (hasSeenTarget)
        {
            Vector3 heading = targetPoint - transform.position;
            distanceToTarget = Vector3.Distance(transform.position, targetPoint);
            if (distanceToTarget < detectionRange && distanceToTarget > distanceFromTarget + smallWidth)
            {
                transform.position += heading.normalized * speed * Time.deltaTime;
            }
            else if (distanceToTarget < distanceFromTarget - smallWidth)
            {
                transform.position += -heading.normalized * speed * Time.deltaTime;
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
    }

    protected virtual void PlotMove()
    {
        targetPoint = target.FocusPoint.position;
    }

    void AttemptFire()
    {

        if (CanSeeTarget())
        weapon.Shoot(targetPoint);
    }

    bool CanSeeTarget()
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

