using UnityEngine;

public class Actor : Entity
{
    [Header("Actor Settings")]
    [Header("Behaviour Collection Filepath")]
    [SerializeField]string path = "States/";

    [Header("Input")]
    [SerializeField]Vector3 targetInput;
    [SerializeField]Vector3 actualInput;

    float inputModifier = 1f;
    [SerializeField]float inputInterpolationSpeed = 2.5f;

    [Header("Movement/Collision Properties")]
    [SerializeField]float collisionRadius = .25f;
    [SerializeField]float stepOverHeight = .25f;
    [SerializeField]float groundCheckDistance = .5f;

    [SerializeField]float skinWidth = .03f;
    [SerializeField]float staticFriction = .5f;
    [SerializeField]float dynamicFriction = .4f;

    [SerializeField]float height;

    [Header("Equipment")]
    WeaponController weaponController;

    protected virtual float CurrentHeight => height;
    protected float Height { get { return height; } }
    protected float CollisionRadius { get { return collisionRadius; } }
    protected float SkinWidth { get { return skinWidth; } }

    protected string Path { get { return path; } }
    protected WeaponController WeaponController { get { return weaponController; } }

    protected StateMachine StateMachine { get; private set; }

    public Vector3 Velocity { get; protected set; }
    public Vector3 TargetInput { get { return targetInput; } }
    public Vector3 ActualInput { get { return actualInput; } }

    public Transform FocusPoint { get; private set; }
    public bool IsGrounded { get; set; }

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

        weaponController = this.GetComponent<WeaponController>();

        this.Subscribe(ActorEvent.SetActorTargetInput, SetTargetInput);

        //GroundCheck
        this.Subscribe(ActorEvent.UpdateActorGroundedStatus, (object[] args) => UpdateGroundedStatus());
        //velocity
        this.Subscribe(ActorEvent.ModifyActorVelocity, ModifyVelocity);
        this.Subscribe(ActorEvent.SetActorWeapon, (object[] args) => weaponController.SetWeapon((Weapon)args[0]));

        this.StateMachine = InitializeStateMachine();
    }
    protected virtual StateMachine InitializeStateMachine()
    {
        return null;
    }

    protected virtual void Update()
    {
        Interpolate();
    }
    protected virtual void Interpolate()
    {
        actualInput = Vector3.Lerp(actualInput, targetInput, inputInterpolationSpeed * (Time.deltaTime / Time.timeScale));
    }

    void SetTargetInput(object[] args)
    {
        targetInput = (Vector3)args[0];
        targetInput *= inputModifier;
    }
    protected void SetInputModifier(float modifier)
    {
        inputModifier = modifier;
    }

    //vitals
    protected override void OnHealthZero()
    {
        base.OnHealthZero();

        Destroy(this);
    }

    //physics
    void UpdateGroundedStatus()
    {
        //Ray ray = new Ray(this.transform.position + (Vector3.up * stepOverHeight), Vector3.down);

        //offsetta lite uppåt för att få en mer reliable ground check
        //isGrounded = Physics.Raycast(ray, stepOverHeight + groundCheckDistance);
        this.IsGrounded = Physics.SphereCast(this.transform.position + (Vector3.up * (stepOverHeight + collisionRadius)), collisionRadius, Vector3.down, out RaycastHit hit, stepOverHeight + groundCheckDistance);
        //Debug.DrawRay(ray.origin, ray.direction * (characterStepOverHeight + groundCheckDistance), isGrounded ? Color.green : Color.red);
    }
    protected void CheckCollision()
    {
        Vector3 pointA = this.transform.position + (Vector3.up * (CurrentHeight - collisionRadius));
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
            {
                Vector3 tnf = this.Velocity.GetNormalForce(hit.normal);
                this.Velocity += tnf;
                this.Velocity = Friction(this.Velocity, tnf);
            }

            CheckOverlap();

            pointA = this.transform.position + (Vector3.up * (CurrentHeight - collisionRadius));
            pointB = this.transform.position + (Vector3.up * collisionRadius);
            Physics.CapsuleCast(pointA, pointB, collisionRadius, this.Velocity.normalized, out hit, this.Velocity.magnitude + skinWidth);

            counter++;
            if (counter == 11)
                break;
        }

        if(counter < 2)
            CheckOverlap(); // ifall vi breakar ur while-loopen vill vi fortfarande kolla overlap
    }
    protected virtual void CheckOverlap()
    {
    }

    Vector3 Friction(Vector3 velocity, Vector3 normalForce)
    {
        /* Om magnituden av vår hastighet är mindre än den statiska friktionen (normalkraften multiplicerat med den statiska friktionskoefficienten)
         * sätter vi vår hastighet till noll, annars adderar vi den motsatta riktningen av hastigheten multiplicerat med den dynamiska friktionen 
         * (normalkraften multiplicerat med den dynamiska friktionskoefficienten).
         */
        if (velocity.magnitude < (normalForce.magnitude * staticFriction))
        {
            velocity.x = 0f;
            velocity.z = 0f;
            return velocity;
        }
        else
        {
            velocity += -velocity.normalized * (normalForce.magnitude * dynamicFriction);
            return velocity;
        }
    }
    void ModifyVelocity(object[] args)
    {
        this.Velocity += (Vector3)args[0];
    }
}
