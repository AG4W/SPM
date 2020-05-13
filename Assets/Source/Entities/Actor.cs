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
    [Tooltip("Layers the Actor should collide with")]
    [SerializeField]LayerMask collisionMask;
    [SerializeField]float collisionRadius = .25f;
    [Range(.01f, .1f)][SerializeField]float skinWidth = .03f;

    [Tooltip("Friction needed to overcome when standing still")]
    [Range(0f, 1f)][SerializeField]float staticFriction = .5f;
    [Tooltip("Friction needed to overcome when moving")]
    [Range(0f, 1f)][SerializeField]float dynamicFriction = .4f;

    [Tooltip("Speed modifier on Y-axis when jumping on a slope")]
    [Range(.25f, .75f)][SerializeField]float jumpOnSlopeYaxisModifier = .75f;

    [SerializeField]float height = 1.8f;

    [Header("Equipment")]
    WeaponController weaponController;

    Vital health;
    AudioSource source;
    
    [SerializeField]StateMachine stateMachine;

    protected virtual float CurrentHeight => height;
    protected float Height { get { return height + heightOffset; } }
    float heightOffset = 0f;

    protected virtual float CurrentFeetOffset => feetOffset;
    float feetOffset = 0f;
    protected float CollisionRadius { get { return collisionRadius; } }
    protected float SkinWidth { get { return skinWidth; } }

    protected LayerMask CollisionMask { get { return collisionMask; } }

    protected string Path { get { return path; } }
    protected WeaponController WeaponController { get { return weaponController; } }

    protected StateMachine StateMachine { get { return stateMachine; } private set { stateMachine = value; } }

    public Vector3 Velocity { get; protected set; }
    public Vector3 TargetInput { get { return targetInput * this.inputModifier; } }
    public Vector3 ActualInput { get { return actualInput; } }

    public Transform FocusPoint { get; private set; }
    public Transform EyePoint { get; private set; }

    protected override void Initalize()
    {
        base.Initalize();

        this.FocusPoint = this.transform.FindRecursively("focusPoint");
        this.EyePoint = this.transform.FindRecursively("eyePoint");

        if (this.FocusPoint == null)
        {
            Debug.LogWarning(this.name + " is missing a focusPoint, using position of transform");
            this.FocusPoint = new GameObject("focusPoint").transform;
            this.FocusPoint.SetParent(this.transform);
            this.FocusPoint.position = this.transform.position;
        }
        if (this.EyePoint == null)
        {
            Debug.LogWarning(this.name + " is missing an eyePoint, using position of transform");
            this.EyePoint = new GameObject("eyePoint").transform;
            this.EyePoint.SetParent(this.transform);
            this.EyePoint.position = this.transform.position;
        }

        weaponController = this.GetComponent<WeaponController>();
        weaponController.Initialize(this);

        source = this.transform.FindRecursively("voiceSource").GetComponent<AudioSource>();

        this.Subscribe(ActorEvent.SetTargetInput, SetTargetInput);
        this.Subscribe(ActorEvent.SetInputModifier, SetInputModifier);
        this.Subscribe(ActorEvent.PlayAudio, (object[] args) => {
            if(args.Length > 1)
                source.pitch = Random.Range((float)args[1], (float)args[2]);

            source.PlayOneShot((AudioClip)args[0]);
        });

        //velocity
        this.Subscribe(ActorEvent.ModifyVelocity, ModifyVelocity);
    }

    protected override void Update()
    {
        base.Update();

        Interpolate();
    }

    //Inputs
    protected virtual void Interpolate() => actualInput = Vector3.Lerp(actualInput, this.TargetInput, inputInterpolationSpeed * (Time.deltaTime / Time.timeScale));

    void SetTargetInput(object[] args) => targetInput = (Vector3)args[0];
    void SetInputModifier(object[] args) => inputModifier = (float)args[0];

    protected override void OnHealthChanged(float change)
    {
        base.OnHealthChanged(change);
        this.Raise(ActorEvent.OnActorHealthChanged, this.health);
    }

    //vitals
    protected override void OnHealthZero()
    {
        base.OnHealthZero();
        Destroy(this);
    }

    //physics
    protected void CheckCollision()
    {
        Vector3 pointA = this.transform.position + (Vector3.up * (this.CurrentHeight - this.collisionRadius));
        Vector3 pointB = this.transform.position + (Vector3.up * (CurrentFeetOffset + collisionRadius));

        float allowedMoveDistance;
        float hitSurfaceSteepness;

        int counter = 1;
        while (Physics.CapsuleCast(pointA, pointB, collisionRadius, this.Velocity.normalized, out RaycastHit hit, Mathf.Infinity, collisionMask))
        {
            hitSurfaceSteepness = Vector3.Dot(Vector3.down, hit.normal); // Plan mark blir -1. Vertikal vägg blir 0. Plant tak blir 1.

            allowedMoveDistance = skinWidth / Vector3.Dot(this.Velocity.normalized, hit.normal); // får ett negativt tal (-skinWidth till oändlighet mot 0, i teorin) som måste dras av från träffdistance för att hamna på SkinWidth avstånd från träffpunkten(faller vi rakt ner, 90 deg, får vi -SkinWidth.)
            allowedMoveDistance += hit.distance; // distans till träff för att hamna på skinWidth

            if (allowedMoveDistance > this.Velocity.magnitude * (Time.deltaTime / Time.timeScale))
                break;  // fritt fram att röra sig denna frame
            else if (allowedMoveDistance >= 0) // om distansen är kortare än vad vi vill röra oss, och över noll, så vill vi flytta karaktären till skinWidth avstånd från ytan
                this.transform.position += this.Velocity.normalized * allowedMoveDistance;

            if (hit.distance <= this.Velocity.magnitude * (Time.deltaTime / Time.timeScale) + skinWidth) // Applicera normalkraft och friktion om vi kommer träffa en yta
            {
                // Tar bort velocity.y om man träffat en brant backe och inte hoppar/faller för att motverka klättring
                if (hitSurfaceSteepness > -.8f)
                    this.Velocity = new Vector3(this.Velocity.x, -1f, this.Velocity.z); //(Krulls:NOTE) -1f kan vi vilja ändra. -gravity?

                Vector3 tempNormalForce = this.Velocity.GetNormalForce(hit.normal);
                this.Velocity += tempNormalForce;

                // Minskar velocity.y vid hopp för att slippa boost av normalforce vid kollisioner
                this.Velocity = Friction(this.Velocity, tempNormalForce);
            }

            CheckOverlap();

            counter++;
            if (counter == 11)
                break;

            pointA = this.transform.position + (Vector3.up * (CurrentHeight - collisionRadius));
            pointB = this.transform.position + (Vector3.up * (CurrentFeetOffset + collisionRadius));
        }

        if (counter < 2)
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

        if (Vector3.Dot(velocity, Vector3.down) > 0.9f) // Om vi faller applicera inte friktion
            return velocity;

        if (velocity.magnitude < (normalForce.magnitude * staticFriction)) // Om vi inte kan övervinna den statiska friktionen
        {
            velocity.x = 0f;
            velocity.z = 0f;
            return velocity;
        }
        
        velocity += -velocity.normalized * (normalForce.magnitude * dynamicFriction);
        return velocity;
    }

    void ModifyVelocity(object[] args) => this.Velocity += (Vector3)args[0];
    public void SetCollisionLowPoint(float value)
    {
        feetOffset = value;
    }
    public void SetCollisionHighPoint(float value)
    {
        heightOffset = value;
    }
}
