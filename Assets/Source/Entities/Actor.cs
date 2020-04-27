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

    [SerializeField]float skinWidth = .03f;
    [SerializeField]float staticFriction = .5f;
    [SerializeField]float dynamicFriction = .4f;

    [SerializeField]float height = 1.8f;

    [SerializeField]LayerMask collisionMask;

    [Header("Equipment")]
    WeaponController weaponController;

    Vital health;
    [SerializeField]StateMachine stateMachine;

    protected virtual float CurrentHeight => height;
    protected float Height { get { return height + heightOffset; } }
    private float heightOffset = 0f;

    protected virtual float CurrentFeetOffset => feetOffset;
    private float feetOffset = 0f;
    protected float CollisionRadius { get { return collisionRadius; } }
    protected float SkinWidth { get { return skinWidth; } }

    protected LayerMask CollisionMask { get { return collisionMask; } }

    protected string Path { get { return path; } }
    protected WeaponController WeaponController { get { return weaponController; } }

    protected StateMachine StateMachine { get { return stateMachine; } private set { stateMachine = value; } }

    public Vector3 Velocity { get; protected set; }
    public Vector3 TargetInput { get { return targetInput; } }
    public Vector3 ActualInput { get { return actualInput; } }

    public Transform FocusPoint { get; private set; }

    //(Krulls)
    //protected Transform pointAsphere { get; set; }
    //protected Transform pointBsphere { get; set; }

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

        //(Krulls)
        //this.pointAsphere = this.transform.FindRecursively("pointAsphere");
        //this.pointBsphere = this.transform.FindRecursively("pointBsphere");
        //if (this.pointAsphere == null || this.pointBsphere == null)
        //    Debug.LogWarning(this.name + " is missing a pointAspehere or pointBsphere!");

        weaponController = this.GetComponent<WeaponController>();

        this.Subscribe(ActorEvent.SetActorTargetInput, SetTargetInput);

        //velocity
        this.Subscribe(ActorEvent.ModifyActorVelocity, ModifyVelocity);
        this.Subscribe(ActorEvent.SetActorWeapon, (object[] args) => weaponController.SetWeapon((Weapon)args[0]));

        this.StateMachine = InitializeStateMachine();
    }

    protected virtual StateMachine InitializeStateMachine() => null;

    protected override void Update()
    {
        base.Update();
        Interpolate();
    }
    protected virtual void Interpolate() => actualInput = Vector3.Lerp(actualInput, targetInput, inputInterpolationSpeed * (Time.deltaTime / Time.timeScale));

    public void SetFeetOffset(float value)
    {
        feetOffset = value;
    }

    public void SetHeightOffset(float value)
    {
        heightOffset = value;
    }

    void SetTargetInput(object[] args)
    {
        targetInput = (Vector3)args[0];
        targetInput *= inputModifier;
    }

    protected void SetInputModifier(float modifier) => inputModifier = modifier;

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

        //Vector3 pointA = this.pointAsphere.position;
        //Vector3 pointB = this.pointBsphere.position;

        Physics.CapsuleCast(pointA, pointB, collisionRadius, this.Velocity.normalized, out RaycastHit hit, Mathf.Infinity, collisionMask);

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
                Vector3 tempNormalForce = this.Velocity.GetNormalForce(hit.normal);
                this.Velocity += tempNormalForce;
                this.Velocity = Friction(this.Velocity, tempNormalForce);
            }

            CheckOverlap();

            pointA = this.transform.position + (Vector3.up * (CurrentHeight - collisionRadius));
            pointB = this.transform.position + (Vector3.up * (CurrentFeetOffset + collisionRadius));
            //pointA = this.pointAsphere.position;
            //pointB = this.pointBsphere.position;
            Physics.CapsuleCast(pointA, pointB, collisionRadius, this.Velocity.normalized, out hit, this.Velocity.magnitude + skinWidth, collisionMask);

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
        else if (Vector3.Dot(velocity, Vector3.down) > 0.9f) // Om vi faller applicera inte friktion
            return velocity;
        else 
        {
            velocity += -velocity.normalized * (normalForce.magnitude * dynamicFriction);
            return velocity;
        }
    }
    void ModifyVelocity(object[] args) => this.Velocity += (Vector3)args[0];
}
