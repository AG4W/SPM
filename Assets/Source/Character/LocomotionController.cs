using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]GameObject[] torches;

    [SerializeField]Vector3 velocity;
    [SerializeField]Vector3 velocityBeforeLosingGroundContact;

    [SerializeField]Vector3 targetInput;
    [SerializeField]Vector3 actualInput;

    [SerializeField]float targetStance;
    [SerializeField]float actualStance;
    [SerializeField]bool isSprinting;

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

    [Header("Jumping")]
    [SerializeField]float jumpAcceleration = 7f;
    [SerializeField]AnimationCurve jumpCurve;

    [SerializeField]float jumpDuration = 1f;
    [SerializeField]float minimumJumpDuration = .1f;
    [SerializeField]float jumpTimer;

    [Header("Falling")]
    [SerializeField]float gravitationalConstant = 9.82f;
    [SerializeField]float fallDuration;
    [SerializeField]float fallForwardMomentDeceleration = .5f;
    [SerializeField]float fallForwardVelocityDivider = 1.8f;

    [SerializeField]bool isGrounded;
    [SerializeField]bool wasGroundedLastFrame;

    [SerializeField]bool isJumping = false;
    [SerializeField]bool isWalking = false;
    [SerializeField]bool inIronSights = false;

    [Header("Combat Mode")]
    [SerializeField]float combatInterpolationSpeed = 2.5f;

    [Header("Equipment")]
    [SerializeField]WeaponController weapon;

    Transform jig;
    Animator animator;

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

    void Awake()
    {
        jig = FindObjectOfType<CameraController>().transform;

        if (jig == null)
            Debug.LogError("LocomotionController could not find Camera Jig, did you forget to drag the prefab into your scene?");

        animator = this.GetComponentInChildren<Animator>();

        GlobalEvents.Subscribe(GlobalEvent.ForcePowerActivated, (object[] args) => 
        {
            if (isJumping || !isGrounded)
                return;

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

            GlobalEvents.Raise(GlobalEvent.NoiseCreated, this.transform.position);
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

            velocityBeforeLosingGroundContact = this.velocity;
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

        //undvik att låsa spelaren i idle mode
        this.animator.SetBool("isAlert", true);
    }
    void Update()
    {
        UpdateGroundedStatus();
        GatherInput();

        if (isJumping)
        {
            jumpTimer += (Time.deltaTime / Time.timeScale);

            if (jumpTimer >= jumpDuration || jumpTimer >= minimumJumpDuration && isGrounded)
            {
                jumpTimer = 0f;
                isJumping = false;

                //om vi inte är grounded när vårt hopp tar slut
                //spara vår senaste velocity och applicera den för att 
                //falla korrekt?
            }
        }

        if(velocityBeforeLosingGroundContact.magnitude > .01f)
            velocityBeforeLosingGroundContact = Vector3.Lerp(velocityBeforeLosingGroundContact, Vector3.zero, fallForwardMomentDeceleration * (Time.deltaTime / Time.timeScale));

        CorrectStance();
        UpdateAnimator();
        UpdateRotation();
    }
    void OnAnimatorMove()
    {
        Vector3 tempV = velocity;
        velocity = this.animator.velocity;
        velocity.y = tempV.y;
        
        //apply gravity
        velocity += Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale);
        //applicera jump
        if(isJumping)
            velocity += GetJumpVelocity() * (gravitationalConstant + jumpAcceleration)  * (Time.deltaTime / Time.timeScale);
        
        if (isJumping || !isGrounded)
        {
            velocity += velocityBeforeLosingGroundContact / fallForwardVelocityDivider;
            velocity *= Mathf.Pow(airResistance, (Time.deltaTime / Time.timeScale));
        }

        // Vi kollar detta sist så att vi inte råkar förflytta karaktären efter att vi har kollat för kollision
        Vector3 pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
        Vector3 pointB = this.transform.position + (Vector3.up * collisionRadius);
        Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out RaycastHit hit, float.PositiveInfinity);

        int counter = 1;
        while (hit.transform != null)
        {
            float allowedMoveDistance = skinWidth / Vector3.Dot(velocity.normalized, hit.normal); // får ett negativt tal (-skinWidh till oändlighet mot 0, i teorin) som måste dras av från träffdistance för att hamna på SkinWidth avstånd från träffpunkten(faller vi rakt ner, 90 deg, får vi -SkinWidth.)
            allowedMoveDistance += hit.distance; // distans till träff för att hamna på skinWidth
            
            if (allowedMoveDistance > velocity.magnitude * (Time.deltaTime / Time.timeScale)) 
                break;  // fritt fram att röra sig om distansen är större än vad vi kommer röra oss denna frame
            else if (allowedMoveDistance >= 0) // om distansen är kortare än vad vi vill röra oss, så vill vi flytta karaktären fram dit
                this.transform.position += velocity.normalized * allowedMoveDistance;

            if (hit.distance <= velocity.magnitude)
            {
                Vector3 tnf = velocity.GetNormalForce(hit.normal);
                velocity += tnf;
                //ApplyFriction(tnf); // root motion hanterar vår rörlse i x/z-led
            }

            pointA = this.transform.position + (Vector3.up * (currentHeight - collisionRadius));
            pointB = this.transform.position + (Vector3.up * collisionRadius);
            Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out hit, velocity.magnitude + skinWidth);

            counter++;
            if (counter == 11)
                break;
        }

        this.transform.position += velocity * (Time.deltaTime / Time.timeScale);
    }
    void LateUpdate()
    {
        wasGroundedLastFrame = isGrounded;
    }

    void UpdateGroundedStatus()
    {
        if(isJumping)
            isGrounded = false;
        else
        {
            Ray ray = new Ray(this.transform.position + (Vector3.up * stepOverHeight), Vector3.down);

            //offsetta lite uppåt för att få en mer reliable ground check
            isGrounded = Physics.Raycast(ray, stepOverHeight + groundCheckDistance);

            if (wasGroundedLastFrame && !isGrounded)
            {
                velocityBeforeLosingGroundContact = new Vector3(this.velocity.x, 0f, this.velocity.z);
                fallDuration = 0f;
            }
        }

        fallDuration += (isGrounded || isJumping) ? 0f : (Time.deltaTime / Time.timeScale);
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
        animator.SetBool("isJumping", isJumping);
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
        return Vector3.up * jumpCurve.Evaluate(jumpTimer);
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
