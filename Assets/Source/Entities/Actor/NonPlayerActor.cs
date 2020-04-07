using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerActor : Actor
{
    [SerializeField] float detectionRange;
    [SerializeField] float speed;
    [SerializeField] float distanceFromTarget;
    [SerializeField] float targetUpdatRate;
    [SerializeField] Actor target;
    [SerializeField] WeaponController weapon;

    private float targetTimer;
    private float smallWidth = 0.1f;
    private Vector3 targetPoint;

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


        Vector3 heading = targetPoint - transform.position;
        float distance = Vector3.Distance(transform.position, targetPoint);
        if (distance < detectionRange && distance > distanceFromTarget + smallWidth)
        {
            transform.position += heading.normalized * speed * Time.deltaTime;
        }
        else if (distance < distanceFromTarget - smallWidth)
        {
            transform.position += -heading.normalized * speed * Time.deltaTime;
        }

        transform.LookAt(target.FocusPoint.position);
    }

    protected virtual void PlotMove()
    {
        targetPoint = target.FocusPoint.position;
    }
}

