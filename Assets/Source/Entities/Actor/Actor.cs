﻿using UnityEngine;

using System.Collections.Generic;

public class Actor : Entity
{
    [Header("Input")]
    [SerializeField]Vector3 targetInput;
    [SerializeField]Vector3 actualInput;

    [Header("Movement/Collision Properties")]
    [SerializeField]float minHeight = 1.1f;
    [SerializeField]float maxHeight = 1.9f;
    [SerializeField]float collisionRadius = .25f;
    [SerializeField]float skinWidth = .03f;

    [Header("Animation")]
    [SerializeField]MovementMode mode = MovementMode.Jog;
    [SerializeField]float inputModifier = 1f;

    [SerializeField]AimMode aimMode = AimMode.Default;
    [SerializeField]float actualAimStance;

    [SerializeField]float interpolationSpeed = 2.5f;

    [SerializeField]Stance stance = Stance.Standing;

    [SerializeField]float targetStance;
    [SerializeField]float actualStance;

    [Header("IK")]
    [SerializeField]Vector3 targetLookAtPosition;
    [SerializeField]Vector3 actualLookAtPosition;

    float[] targetLookAtWeights = new float[5];
    float[] actualLookAtWeights = new float[5];

    [SerializeField]float lookAtInterpolationSpeed = 1.5f;

    [SerializeField]float stepOverHeight = .25f;
    [SerializeField]float groundCheckDistance = .2f;

    [Header("Falling")]
    [SerializeField]float fallDuration;

    [SerializeField]bool isGrounded;
    [SerializeField]bool wasGroundedLastFrame;

    [Header("Equipment")]
    [SerializeField]WeaponController weapon;

    StateMachine machine;

    float currentHeight
    {
        get
        {
            return Mathf.Lerp(1.4f, 1.8f, actualStance);
        }
    }

    protected Animator Animator { get; private set; }

    public Vector3 Velocity { get; private set; }
    public Vector3 TargetInput { get { return targetInput; } }
    public Vector3 ActualInput { get { return actualInput; } }

    public Transform FocusPoint { get; private set; }
    public bool IsGrounded { get { return isGrounded; } set { isGrounded = value; } }

    // Använd inte Start/Awake i klasser som ärver ifrån Entity!
    // Använd istället protected override base.Initialize(), denna kallas i Start.
    // Kom ihåg att anropa basmetoden också.
    protected override void Initalize()
    {
        base.Initalize();

        this.FocusPoint = this.transform.FindRecursively("focusPoint");
        
        if (this.FocusPoint == null)
        {
            Debug.LogWarning(this.name + " is missing a focusPoint, using position of transform");
            this.FocusPoint = new GameObject("focusPoint").transform;
            this.FocusPoint.SetParent(this.transform);
            this.FocusPoint.position = this.transform.position;
        }

        this.Animator = this.GetComponent<Animator>();

        //Input
        GlobalEvents.Subscribe(GlobalEvent.SetActorTargetInput, SetTargetInput);
        GlobalEvents.Subscribe(GlobalEvent.SetActorMovementMode, SetMovementSpeed);
        GlobalEvents.Subscribe(GlobalEvent.SetActorTargetStance, SetTargetStance);
        GlobalEvents.Subscribe(GlobalEvent.SetActorTargetAimMode, SetTargetAimMode);

        //velocity
        GlobalEvents.Subscribe(GlobalEvent.ModifyActorVelocity, ModifyVelocity);

        //GroundCheck
        GlobalEvents.Subscribe(GlobalEvent.UpdateActorGroundedStatus, (object[] args) => UpdateGroundedStatus());

        //IK
        GlobalEvents.Subscribe(GlobalEvent.SetActorLookAtPosition, SetLookAtPosition);
        GlobalEvents.Subscribe(GlobalEvent.SetActorLookAtWeights, SetLookAtWeights);

        this.machine = new StateMachine(this,
            Resources.LoadAll<State>("States/Player"),
            new Dictionary<string, object>
            {
                ["jig"] = FindObjectOfType<CameraController>(),
                ["animator"] = this.GetComponent<Animator>(),
                ["actor"] = this.GetComponent<Actor>(),
                ["weapon"] = weapon,
            });
    }

    protected virtual void Update()
    {
        //Debug.Log(baseVelocity + ", " + modifiedVelocity + ", " + this.Velocity);

        Interpolate();
        UpdateAnimator();
    }
    protected virtual void OnAnimatorMove()
    {
        this.Velocity = new Vector3(this.Animator.velocity.x, this.Velocity.y, this.Animator.velocity.z);
        
        machine.Tick();
        // Vi kollar detta sist så att vi inte råkar förflytta karaktären efter att vi har kollat för kollision
        CheckCollision();

        this.transform.position += this.Velocity * (Time.deltaTime / Time.timeScale);
    }
    protected virtual void OnAnimatorIK(int layerIndex)
    {
        //kommer lookatgrejs här sen
        this.Animator.SetLookAtPosition(actualLookAtPosition);
        this.Animator.SetLookAtWeight(actualLookAtWeights[0], actualLookAtWeights[1], actualLookAtWeights[2], actualLookAtWeights[3], actualLookAtWeights[4]);
    }
    void LateUpdate()
    {
        wasGroundedLastFrame = isGrounded;
    }

    void Interpolate()
    {
        actualInput = Vector3.Lerp(actualInput, targetInput, interpolationSpeed * (Time.deltaTime / Time.timeScale));
        actualStance = Mathf.Lerp(actualStance, targetStance, interpolationSpeed * (Time.deltaTime / Time.timeScale));
        actualAimStance = Mathf.Lerp(actualAimStance, (int)aimMode, lookAtInterpolationSpeed * (Time.deltaTime / Time.timeScale));
        actualLookAtPosition = Vector3.Lerp(actualLookAtPosition, targetLookAtPosition, lookAtInterpolationSpeed * (Time.deltaTime / Time.timeScale));

        for (int i = 0; i < targetLookAtWeights.Length; i++)
            actualLookAtWeights[i] = Mathf.Lerp(actualLookAtWeights[i], targetLookAtWeights[i], lookAtInterpolationSpeed * (Time.deltaTime / Time.timeScale));
    }

    void CheckCollision()
    {
        Vector3 pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
        Vector3 pointB = this.transform.position + (Vector3.up * collisionRadius);

        Physics.CapsuleCast(pointA, pointB, collisionRadius, this.Velocity.normalized, out RaycastHit hit, Mathf.Infinity);

        float allowedMoveDistance;

        int counter = 1;
        while (hit.transform != null)
        {
            allowedMoveDistance = skinWidth / Vector3.Dot(this.Velocity.normalized, hit.normal); // får ett negativt tal (-skinWidh till oändlighet mot 0, i teorin) som måste dras av från träffdistance för att hamna på SkinWidth avstånd från träffpunkten(faller vi rakt ner, 90 deg, får vi -SkinWidth.)
            allowedMoveDistance += hit.distance; // distans till träff för att hamna på skinWidth

            if (allowedMoveDistance > this.Velocity.magnitude * (Time.deltaTime / Time.timeScale))
                break;  // fritt fram att röra sig om distansen är större än vad vi kommer röra oss denna frame

            else if (allowedMoveDistance >= 0) // om distansen är kortare än vad vi vill röra oss, så vill vi flytta karaktären fram dit
                this.transform.position += this.Velocity.normalized * allowedMoveDistance;

            if (hit.distance <= this.Velocity.magnitude)
                this.Velocity += this.Velocity.GetNormalForce(hit.normal);

            CheckOverlap();

            pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
            pointB = this.transform.position + (Vector3.up * collisionRadius);
            Physics.CapsuleCast(pointA, pointB, collisionRadius, this.Velocity.normalized, out hit, this.Velocity.magnitude + skinWidth);

            counter++;
            if (counter == 11)
                break;
        }
    }
    void CheckOverlap()
    {
        Vector3 pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
        Vector3 pointB = this.transform.position + (Vector3.up * collisionRadius);

        Vector3 closestPoint;
        Vector3 hitDirection;
        float hitDist;

        bool overlapCheckA = Physics.CheckSphere(pointA, collisionRadius);
        bool overlapCheckB = Physics.CheckSphere(pointB, collisionRadius);

        int counter = 0;
        while (overlapCheckA == true || overlapCheckB == true)
        {
            Collider[] overlapCollidersA = Physics.OverlapSphere(pointA, collisionRadius);
            if (overlapCollidersA.Length > 0)
                for (int i = 0; i < overlapCollidersA.Length; i++)
                {
                    closestPoint = overlapCollidersA[i].ClosestPoint(pointA); // punkt i den överlappande collidern som är närmast centrum på sfären

                    hitDist = Vector3.Distance(pointA, closestPoint);
                    hitDirection = closestPoint - pointA;

                    this.transform.position += -hitDirection.normalized * (collisionRadius - hitDist + skinWidth); // Vi vill flytta oss bakåt: radien på sfären minus distans

                    this.Velocity += this.Velocity.GetNormalForce(-hitDirection.normalized); // Applicera normalkraft 

                    // Uppdatera pointA/B
                    pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
                    pointB = this.transform.position + (Vector3.up * collisionRadius);
                }

            Collider[] overlapCollidersB = Physics.OverlapSphere(pointB, collisionRadius);
            if (overlapCollidersB.Length > 0)
                for (int i = 0; i < overlapCollidersB.Length; i++)
                {
                    closestPoint = overlapCollidersB[i].ClosestPoint(pointB);

                    hitDist = Vector3.Distance(pointB, closestPoint);
                    hitDirection = closestPoint - pointB;

                    this.transform.position += -hitDirection.normalized * (collisionRadius - hitDist + skinWidth);

                    this.Velocity += this.Velocity.GetNormalForce(-hitDirection.normalized);

                    pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
                    pointB = this.transform.position + (Vector3.up * collisionRadius);
                }

            // Kolla overlap igen
            overlapCheckA = Physics.CheckSphere(pointA, collisionRadius);
            overlapCheckB = Physics.CheckSphere(pointB, collisionRadius);

            if (counter >= 100)
                break;
            counter++;
        }
    }

    void UpdateGroundedStatus()
    {
        //Ray ray = new Ray(this.transform.position + (Vector3.up * stepOverHeight), Vector3.down);

        //offsetta lite uppåt för att få en mer reliable ground check
        //isGrounded = Physics.Raycast(ray, stepOverHeight + groundCheckDistance);
        isGrounded = Physics.SphereCast(this.transform.position + (Vector3.up * (stepOverHeight + collisionRadius)), collisionRadius, Vector3.down, out RaycastHit hit, stepOverHeight + groundCheckDistance);

        if (wasGroundedLastFrame && !isGrounded)
            fallDuration = 0f;

        fallDuration += (isGrounded) ? 0f : (Time.deltaTime / Time.timeScale);
        //Debug.DrawRay(ray.origin, ray.direction * (characterStepOverHeight + groundCheckDistance), isGrounded ? Color.green : Color.red);
    }
    void UpdateAnimator()
    {
        this.Animator.SetFloat("inputMagnitude", targetInput.magnitude);
        this.Animator.SetFloat("fallDuration", fallDuration);
        this.Animator.SetFloat("stance", actualStance);
        this.Animator.SetFloat("aimStance", actualAimStance);

        this.Animator.SetBool("isGrounded", isGrounded);
    }


    void SetTargetInput(object[] args)
    {
        targetInput = (Vector3)args[0];
        targetInput *= inputModifier;
    }
    void SetMovementSpeed(object[] args)
    {
        mode = (MovementMode)args[0];

        switch (mode)
        {
            case MovementMode.Crouch:
                inputModifier = 1f;
                break;
            case MovementMode.Walk:
                inputModifier = .5f;
                break;
            case MovementMode.Jog:
                inputModifier = 1f;
                break;
            case MovementMode.Sprint:
                inputModifier = 2f;
                break;
            default:
                inputModifier = .25f;
                break;
        }
    }
    void SetTargetStance(object[] args)
    {
        stance = (Stance)args[0];
        targetStance = (int)stance;
    }
    void SetTargetAimMode(object[] args)
    {
        aimMode = (AimMode)args[0];
    }

    //IK
    void SetLookAtPosition(object[] args)
    {
        targetLookAtPosition = (Vector3)args[0];
    }
    void SetLookAtWeights(object[] args)
    {
        targetLookAtWeights = (float[])args[0];
    }

    void ModifyVelocity(object[] args)
    {
        this.Velocity += (Vector3)args[0];
    }
}