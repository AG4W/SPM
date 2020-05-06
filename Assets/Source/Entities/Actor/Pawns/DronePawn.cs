using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePawn : Actor, IForceAffectable
{
    Vector3 velocity;
    [SerializeField] float maxSpeed;
    [SerializeField] float acceleration;
    float gravitationalForce = 9.82f;
    Vector3 gravity;

    [SerializeField] Vector3[] patrolPath;
    int pathIndex = 0;
    [SerializeField] [Range(1f, 0f)] float turnRatio = 0.01f;

    Vector3 targetPoint;
    [SerializeField] Actor target;
    [SerializeField] float viewDistance;
    [SerializeField] Vector3 targetOffset = Vector3.up;
    Vector3 focuspoint;

    Vector3[] chasePath = new Vector3[7];
    int chaseIndex = 0;
    int nextChasePoint = 0;
    bool chasing = false;
    bool startedChasing = false;


    void Awake()
    {
        gravity = Vector3.down * gravitationalForce;
        targetPoint = patrolPath[pathIndex];
    }

    // Update is called once per frame
    void Update()
    {
        focuspoint = target.transform.position + targetOffset;

        if (!chasing)
        {
            if (transform.position.DistanceTo(targetPoint) > maxSpeed / 4)
            {
                MoveToPoint(targetPoint);
            }
            else
            {
                targetPoint = patrolPath[pathIndex];
                pathIndex = (pathIndex + 1) % patrolPath.Length;
                //Debug.Log("Next targetPoint: " + pathIndex + " " + patrolPath[pathIndex]);
            }

            if (transform.position.DistanceTo(focuspoint) < viewDistance && !Physics.Linecast(transform.position, focuspoint))
            {
                chasing = true;
                startedChasing = true;
            }
        }
        else
        {
            TrackTarget();
            MoveToPoint(targetPoint);
        }
        this.transform.position += velocity * Time.deltaTime;

        for (int i = 0; i < patrolPath.Length; i++)
        {
            Debug.DrawLine(patrolPath[i], patrolPath[(i + 1) % patrolPath.Length]);
        }
    }

    void MoveToPoint(Vector3 targetPoint)
    {
        // gravity & couteract + force frammåt
        Vector3 directionToTarget = transform.position.DirectionTo(targetPoint);
        this.velocity += directionToTarget * acceleration;

        float dot = Vector3.Dot(velocity.normalized, directionToTarget.normalized);


        this.velocity += ((-velocity * (1f - dot)) * turnRatio);



        this.velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }

    void TrackTarget()
    {
        if (target != null)
        {
            if (startedChasing) // clear path if started chasing
            {
                startedChasing = false;
                for (int i = 0; i < chasePath.Length; i++)
                {
                    chasePath[i] = Vector3.zero;
                }
                chaseIndex = 0;
                chasePath[0] = transform.position;
                nextChasePoint = 1;
            }

            if (Physics.Linecast(chasePath[chaseIndex], focuspoint, out RaycastHit hit))
            {
                Debug.Log(hit.collider);
                Debug.Log(hit.distance);
                if (chaseIndex < chasePath.Length-1) { 
                    chaseIndex++;
                }
                Debug.Log(chaseIndex);
                Debug.Log(targetPoint);
            }
            else
            {
                chasePath[chaseIndex] = focuspoint;
            }

            if (transform.position.DistanceTo(chasePath[nextChasePoint]) < maxSpeed / 4) {
                if (nextChasePoint < chasePath.Length-1 && chasePath[nextChasePoint+1] != Vector3.zero)
                {
                    nextChasePoint++;
                }
                else
                {
                    chasing = false;
                    targetPoint = patrolPath[pathIndex];
                }
                
            }

            targetPoint = chasePath[nextChasePoint];
            

            for (int i = 0; i < chasePath.Length-1; i++)
            {
                if (chasePath[i+1] != Vector3.zero)
                Debug.DrawLine(chasePath[i], chasePath[(i + 1)]);
            }
        }
    }

    protected override void OnHealthZero()
    {
        base.OnHealthZero();
        Destroy(this.gameObject);
    }

    void IForceAffectable.ModifyVelocity(Vector3 change)
    {
        this.velocity += change;
    }

    void IForceAffectable.SetVelocity(Vector3 velocity)
    {
        this.velocity = velocity;
    }
}
