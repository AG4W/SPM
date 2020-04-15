using UnityEngine;

using System.Collections.Generic;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]GameObject[] torches;

    [SerializeField]Vector3 velocity;

    [SerializeField]Vector3 targetInput;
    [SerializeField]Vector3 actualInput;

    [SerializeField]Vector3 lookAtPosition;
    [SerializeField]float[] targetLookAtWeights;
    [SerializeField]float[] actualLookAtWeights;
    [SerializeField]float lookAtInterpolationSpeed = 1.5f;

    [SerializeField]MovementMode mode = MovementMode.Jog;
    [SerializeField]float inputModifier = 1f;

    [SerializeField]Stance stance = Stance.Standing;

    [SerializeField]float targetStance;
    [SerializeField]float actualStance;

    [Header("Movement/Collision Properties")]
    [SerializeField]float minHeight = 1.1f;
    [SerializeField]float maxHeight = 1.9f;
    [SerializeField]float collisionRadius = .25f;
    [SerializeField]float skinWidth = .03f;

    [SerializeField]float staticFriction = .8f;
    [SerializeField]float dynamicFriction = .5f;
    [SerializeField]float airResistance = .2f;

    [SerializeField]float stepOverHeight = .25f;
    [SerializeField]float groundCheckDistance = .2f;

    [Header("Falling")]
    [SerializeField]float fallDuration;

    [SerializeField]bool isGrounded;
    [SerializeField]bool wasGroundedLastFrame;

    [SerializeField]bool inIronSights = false;

    [Header("Combat Mode")]
    [SerializeField]float combatInterpolationSpeed = 2.5f;

    [Header("Equipment")]
    [SerializeField]WeaponController weapon;

    Transform jig;
    Animator animator;

    StateMachine machine;

    float currentHeight { 
        get 
        {
            //prova detta, borde ge mjukare övergång
            return Mathf.Lerp(1.4f, 1.8f, actualStance);

            //if (actualStance < 0.8f)
            //    return 1.4f; 
            //else 
            //    return 1.8f; 
        } 
    }

    public Vector3 Velocity { get { return velocity; } }
    public Vector3 TargetInput { get { return targetInput; } }
    public Vector3 ActualInput { get { return actualInput; } }

    public float ActualStance { get { return actualStance; } }

    public bool IsGrounded { get { return isGrounded; } set { isGrounded = value; } }

    void Awake()
    {
        jig = FindObjectOfType<CameraController>().transform;

        if (jig == null)
            Debug.LogError("LocomotionController could not find Camera Jig, did you forget to drag the prefab into your scene?");

        animator = this.GetComponentInChildren<Animator>();
        actualLookAtWeights = new float[5];

        //Input
        GlobalEvents.Subscribe(GlobalEvent.SetTargetInput, SetTargetInput);
        GlobalEvents.Subscribe(GlobalEvent.SetMovementMode, SetMovementSpeed);
        GlobalEvents.Subscribe(GlobalEvent.SetTargetStance, SetTargetStance);

        //velocity
        GlobalEvents.Subscribe(GlobalEvent.ModifyPlayerVelocity, ModifyVelocity);

        //GroundCheck
        GlobalEvents.Subscribe(GlobalEvent.UpdatePlayerGroundedStatus, (object[] args) => UpdateGroundedStatus());

        //IK
        GlobalEvents.Subscribe(GlobalEvent.SetPlayerLookAtPosition, SetLookAtPosition);
        GlobalEvents.Subscribe(GlobalEvent.SetPlayerLookAtWeights, SetLookAtWeights);

        GlobalEvents.Subscribe(GlobalEvent.ForcePowerActivated, (object[] args) => 
        {
            if (!isGrounded)
                return;

            this.animator.SetFloat("castIndex", (int)((Ability)args[0]).AnimationIndex);
            this.animator.SetTrigger("cast");
            this.animator.SetLayerWeight(2, 1f);
        });
        GlobalEvents.Subscribe(GlobalEvent.Reload, (object[] args) =>
        {
            if (!isGrounded)
                return;

            weapon.Reload();
        });
        //flytta detta till en separat controller sen, borde nog inte vara här
        GlobalEvents.Subscribe(GlobalEvent.ToggleTorches, (object[] args) =>
        {
            for (int i = 0; i < torches.Length; i++)
                torches[i].SetActive(!torches[i].activeSelf);
        });

        //undvik att låsa spelaren i idle mode
        this.animator.SetBool("isAlert", true);
        this.machine = new StateMachine(this,
            Resources.LoadAll<State>("States/Player"),
            new Dictionary<string, object>
            {
                ["controller"] = this,
                ["jig"] = FindObjectOfType<CameraController>(),
                ["animator"] = this.GetComponent<Animator>(),
                ["actor"] = this.GetComponent<Actor>(),
                ["weapon"] = this.weapon,
            });
    }
    void Update()
    {
        //UpdateGroundedStatus();
        GatherInput();

        CorrectStance();
        UpdateAnimator();
    }

    void OnAnimatorMove()
    {
        velocity.x = this.animator.velocity.x;
        velocity.z = this.animator.velocity.z;

        //tick modifierar velocity på många olika sätt
        //måste ske i sync med animatorn
        machine.Tick();

        // Vi kollar detta sist så att vi inte råkar förflytta karaktären efter att vi har kollat för kollision
        CheckCollision();

        this.transform.position += velocity * (Time.deltaTime / Time.timeScale);
    }
    void CheckCollision()
    {
        Vector3 pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
        Vector3 pointB = this.transform.position + (Vector3.up * collisionRadius);
        Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out RaycastHit hit, Mathf.Infinity);

        float allowedMoveDistance;

        int counter = 1;
        while (hit.transform != null)
        {
            allowedMoveDistance = skinWidth / Vector3.Dot(velocity.normalized, hit.normal); // får ett negativt tal (-skinWidh till oändlighet mot 0, i teorin) som måste dras av från träffdistance för att hamna på SkinWidth avstånd från träffpunkten(faller vi rakt ner, 90 deg, får vi -SkinWidth.)
            allowedMoveDistance += hit.distance; // distans till träff för att hamna på skinWidth

            if (allowedMoveDistance > velocity.magnitude * (Time.deltaTime / Time.timeScale))
                break;  // fritt fram att röra sig om distansen är större än vad vi kommer röra oss denna frame

            else if (allowedMoveDistance >= 0) // om distansen är kortare än vad vi vill röra oss, så vill vi flytta karaktären fram dit
                this.transform.position += velocity.normalized * allowedMoveDistance;

            if (hit.distance <= velocity.magnitude)
                velocity += velocity.GetNormalForce(hit.normal);

            CheckOverlap();

            pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
            pointB = this.transform.position + (Vector3.up * collisionRadius);
            Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out hit, velocity.magnitude + skinWidth);

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

                    velocity += velocity.GetNormalForce(-hitDirection.normalized); // Applicera normalkraft 

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

                    velocity += velocity.GetNormalForce(-hitDirection.normalized);

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
    void OnAnimatorIK(int layerIndex)
    {
        //kommer lookatgrejs här sen
        animator.SetLookAtPosition(lookAtPosition);
        animator.SetLookAtWeight(actualLookAtWeights[0], actualLookAtWeights[1], actualLookAtWeights[2], actualLookAtWeights[3], actualLookAtWeights[4]);
    }
    void LateUpdate()
    {
        wasGroundedLastFrame = isGrounded;
    }

    void GatherInput()
    {
        actualInput = Vector3.Lerp(actualInput, targetInput, combatInterpolationSpeed * (Time.deltaTime / Time.timeScale));
        actualStance = Mathf.Lerp(actualStance, targetStance, (combatInterpolationSpeed * 2f) * (Time.deltaTime / Time.timeScale));

        for (int i = 0; i < targetLookAtWeights.Length; i++)
            actualLookAtWeights[i] = Mathf.Lerp(actualLookAtWeights[i], targetLookAtWeights[i], lookAtInterpolationSpeed * (Time.deltaTime / Time.timeScale));

        inIronSights = Input.GetKey(KeyCode.Mouse1);

        if (Input.GetKeyDown(KeyCode.Space))
            GlobalEvents.Raise(GlobalEvent.Jump); 
        if (Input.GetKeyDown(KeyCode.V))
            GlobalEvents.Raise(GlobalEvent.Roll);
        if (Input.GetKeyDown(KeyCode.R))
            GlobalEvents.Raise(GlobalEvent.Reload);
        if (Input.GetKeyDown(KeyCode.F))
            GlobalEvents.Raise(GlobalEvent.ToggleTorches);
    }

    void UpdateAnimator()
    {
        animator.SetFloat("inputMagnitude", targetInput.magnitude);
        animator.SetFloat("fallDuration", fallDuration);

        animator.SetBool("isGrounded", isGrounded);
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
                inputModifier = .5f;
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
    void CorrectStance()
    {
        Physics.Raycast(this.transform.position, Vector3.up, out RaycastHit hit, maxHeight);

        //om vi träffar något och om vi är mellan dem
        if (hit.transform && hit.distance <= maxHeight)
        {
            //hämta vart vi är mellan minHöjd och maxHöjd <-- RELATIVT i skala 0 .. 1
            float location = Mathf.InverseLerp(minHeight, maxHeight, hit.distance);
            //översätt den platsen till vår skala för animatorns crouch
            float stance = Mathf.Lerp(0f, 1f, location);

            //skriv bara över targetStance ifall vår stance är lägre, annars låter vi spelaren croucha fritt.
            if (stance < actualStance)
                actualStance = stance;
        }
    }

    void SetLookAtPosition(object[] args)
    {
        lookAtPosition = (Vector3)args[0];
    }
    void SetLookAtWeights(object[] args)
    {
        targetLookAtWeights = (float[])args[0];
    }

    void ModifyVelocity(object[] args)
    {
        this.velocity += (Vector3)args[0];
    }
}
public enum MovementMode
{
    Crouch,
    Walk,
    Jog,
    Sprint
}
public enum Stance
{
    Crouched,
    Standing,
}
