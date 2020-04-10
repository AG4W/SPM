using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]GameObject[] torches;
    [SerializeField]Transform jig;

    [SerializeField]Vector3 velocity;

    [SerializeField]Vector3 targetInput;
    [SerializeField]Vector3 actualInput;

    [SerializeField]float targetStance;
    [SerializeField]float actualStance;
    [SerializeField]bool isSprinting;

    [Header("Movement/Collision Properties")]
    [SerializeField]float minHeight = 1.1f;
    [SerializeField]float maxHeight = 1.9f;
    [SerializeField]float collisionRadius = .25f;

    [SerializeField]float staticFriction = .8f;
    [SerializeField]float dynamicFriction = .5f;
    [SerializeField]float airResistance = .2f;

    [SerializeField]float stepOverHeight = .25f;
    [SerializeField]float groundCheckDistance = .2f;

    [Header("Jumping")]
    [SerializeField]float jumpAcceleration = 7f;
    [SerializeField]AnimationCurve jumpCurve;

    [SerializeField]float jumpDuration = 1f;
    [SerializeField]float minimumJumpDuration = .1f;
    [SerializeField]float jumpTimer;

    [Header("Falling")]
    [SerializeField]float gravitationalConstant = 9.82f;
    [SerializeField]float fallDuration;

    [SerializeField]bool isGrounded;
    [SerializeField]bool wasGroundedLastFrame;

    [SerializeField]bool isJumping = false;
    [SerializeField]bool isWalking = false;
    [SerializeField]bool inIronSights = false;

    [Header("Combat Mode")]
    [SerializeField]float combatInterpolationSpeed = 2.5f;

    [Header("Equipment")]
    [SerializeField]WeaponController weapon;
    
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

    Animator animator;
    void Awake()
    {
        animator = this.GetComponentInChildren<Animator>();

        GlobalEvents.Subscribe(GlobalEvent.ForcePowerActivated, (object[] args) => 
        {
            this.animator.SetFloat("castIndex", (int)((Ability)args[0]).AnimationIndex);
            this.animator.SetTrigger("cast");
            this.animator.SetLayerWeight(2, 1f);
        });
        GlobalEvents.Subscribe(GlobalEvent.Fire, (object[] args) =>
        {
            if (isSprinting || !isGrounded)
                return;

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

            weapon.Shoot(hit.transform != null ? hit.point : ray.GetPoint(300f));
        });
        GlobalEvents.Subscribe(GlobalEvent.Reload, (object[] args) =>
        {
            if (isSprinting || isJumping || !isGrounded)
                return;

            weapon.Reload();
        });
        GlobalEvents.Subscribe(GlobalEvent.Jump, (object[] args) =>
        {
            if (isJumping || !isGrounded)
                return;

            animator.SetTrigger("jump");
            isJumping = true;
        });
        GlobalEvents.Subscribe(GlobalEvent.Roll, (object[] args) =>
        {
            if (isJumping || !isGrounded)
                return;

            if (actualInput.magnitude < .1f)
                actualInput = Vector3.forward;

            animator.SetTrigger("roll");
        });
        //flytta detta till en separat controller sen, borde nog inte vara här
        GlobalEvents.Subscribe(GlobalEvent.ToggleTorches, (object[] args) =>
        {
            for (int i = 0; i < torches.Length; i++)
                torches[i].SetActive(!torches[i].activeSelf);
        });
    }

    void Update()
    {
        UpdateGroundedStatus();
        GatherInput();

        if (isJumping)
        {
            jumpTimer += (Time.deltaTime / Time.timeScale);

            if (jumpTimer >= jumpDuration || (isGrounded && jumpTimer >= minimumJumpDuration))
            {
                jumpTimer = 0f;
                isJumping = false;
            }
        }

        CorrectStance();
        UpdateAnimator();
        UpdateRotation();
    }
    void OnAnimatorMove()
    {
        velocity = this.animator.velocity;
        //apply gravity
        velocity += Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale);
        //applicera jump
        velocity += GetJumpVelocity() * jumpAcceleration  * (Time.deltaTime / Time.timeScale);

        //snapa mot marken ifall vi precis landat så att vi inte flyter groundDistance ovanför
        //if (!wasGroundedLastFrame && isGrounded)
        //    velocity += Vector3.down * groundCheckDistance;

        // Vi kollar detta sist så att vi inte råkar förflytta karaktären efter att vi har kollat för kollision
        Vector3 pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
        Vector3 pointB = this.transform.position + (Vector3.up * collisionRadius);

        Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out RaycastHit hit, float.PositiveInfinity);

        while (hit.transform != null)
        {

            float allowedMoveDistance = .03f / Vector3.Dot(velocity.normalized, hit.normal); // får negativt avstånd som måste dras av från träffdistance för att hamna på SkinWidth avstånd (faller vi rakt ner, 90 deg, får vi -SkinWidth. Som går i oändlighet till 0)
            allowedMoveDistance += hit.distance; // distans till träff minus anpassad SkinWidth
            if (allowedMoveDistance > velocity.magnitude * Time.deltaTime) { break; } // fritt fram att röra sig om distansen är större än vad vi kommer röra oss denna frame
            else if (allowedMoveDistance >= 0) // om distansen är kortare än vad vi vill röra oss, så vill vi röra oss fram dit och stanna...
            {                                                                           //DELTA TIME????
                //this.transform.position += CollisionTestT(velocity.normalized * allowedMoveDistance); // eftersom det vi vill röra oss denna frame är större än allowedMoveDist beöver vi inte köra *Time.deltaTime
                this.transform.position += velocity.normalized * allowedMoveDistance;
            }
            //else if (allowedMoveDistance < 0) // något blev fel
            //{
            //    break;
            //}
            Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out hit, velocity.magnitude + .03f);

            Vector3 tnf = velocity.GetNormalForce(hit.normal);
            velocity += tnf;
            ApplyFriction(tnf);

            Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out hit, velocity.magnitude + .03f);
        }

        velocity *= Mathf.Pow(airResistance, Time.deltaTime);
        this.transform.position += velocity * Time.deltaTime;
    }
    void LateUpdate()
    {
        wasGroundedLastFrame = isGrounded;
    }

    void UpdateGroundedStatus()
    {
        fallDuration += (isGrounded || isJumping) ? 0f : (Time.deltaTime / Time.timeScale);

        Ray ray = new Ray(this.transform.position + (Vector3.up * stepOverHeight), Vector3.down);
        //offsetta lite uppåt för att få en mer reliable ground check
        isGrounded = Physics.Raycast(ray, stepOverHeight + groundCheckDistance);

        if (wasGroundedLastFrame && !isGrounded)
            fallDuration = 0f;

        //Debug.DrawRay(ray.origin, ray.direction * (characterStepOverHeight + groundCheckDistance), isGrounded ? Color.green : Color.red);
    }
    void GatherInput()
    {
        if (Input.GetKeyDown(KeyCode.CapsLock))
            isWalking = !isWalking;

        isSprinting = Input.GetKey(KeyCode.LeftShift);
        inIronSights = Input.GetKey(KeyCode.Mouse1);

        targetInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        targetInput *= (isWalking || inIronSights) ? .5f : (isSprinting ? 2f : 1f);
        actualInput = Vector3.Lerp(actualInput, targetInput, combatInterpolationSpeed * (Time.deltaTime / Time.timeScale));

        targetStance = Input.GetKey(KeyCode.C) ? 0f : 1f;
        actualStance = Mathf.Lerp(actualStance, targetStance, (combatInterpolationSpeed * 2f) * (Time.deltaTime / Time.timeScale));

        if (Input.GetKey(KeyCode.Mouse0))
            GlobalEvents.Raise(GlobalEvent.Fire);
        if (Input.GetKeyDown(KeyCode.R))
            GlobalEvents.Raise(GlobalEvent.Reload);
        if (Input.GetKeyDown(KeyCode.F))
            GlobalEvents.Raise(GlobalEvent.ToggleTorches);

        if (!isGrounded)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            GlobalEvents.Raise(GlobalEvent.Jump);
        if (Input.GetKeyDown(KeyCode.V))
            GlobalEvents.Raise(GlobalEvent.Roll);
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
    void UpdateAnimator()
    {
        animator.SetFloat("x", actualInput.x);
        animator.SetFloat("z", actualInput.z);
        animator.SetFloat("inputMagnitude", targetInput.magnitude);

        animator.SetFloat("actualStance", actualStance);
        animator.SetFloat("fallDuration", fallDuration);

        animator.SetBool("isGrounded", isGrounded);
    }
    void UpdateRotation()
    {
        this.transform.rotation = Quaternion.Euler(0f, jig.transform.eulerAngles.y, 0f);
    }

    //enkapsulering av småskit
    Vector3 GetJumpVelocity()
    {
        if (!isJumping)
            return Vector3.zero;

        //if (Physics.Raycast(this.topPoint.position, Vector3.up))
        //    return Vector3.zero;

        //använder kurvor för att få bättre feel i hoppet
        //behöver multiplicera med nuvarande velocity på något sätt här
        //så att spelaren inte hoppar framåt om hen inte har framåtrörelse
        //men för trött för den matten atm

        //input heading
        return this.transform.up * jumpCurve.Evaluate(jumpTimer);
    }
    void ApplyFriction(Vector3 normalForce)
    {
        /* Om magnituden av vår hastighet är mindre än den statiska friktionen (normalkraften multiplicerat med den statiska friktionskoefficienten)
         * sätter vi vår hastighet till noll, annars adderar vi den motsatta riktningen av hastigheten multiplicerat med den dynamiska friktionen 
         * (normalkraften multiplicerat med den dynamiska friktionskoefficienten).
         */
        if (velocity.magnitude < (normalForce.magnitude * staticFriction))
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }
        else
        {
            velocity += -velocity.normalized * (normalForce.magnitude * dynamicFriction);
        }
    } // Here is Friction calculated with normalForce and applied to velocity
}
