using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]GameObject[] torches;
    [SerializeField]Transform jig;

    [SerializeField]Vector3 targetInput;

    [SerializeField]float targetStance;
    [SerializeField]bool isSprinting;

    [Header("Movement/Collision Properties")]
    [SerializeField]Transform topPoint;
    [SerializeField]float minHeight = 1.1f;
    [SerializeField]float maxHeight = 1.9f;
    [SerializeField]float collisionRadius = .25f;

    [SerializeField]float stepOverHeight = .25f;
    [SerializeField]float groundCheckDistance = .2f;

    [Header("Jumping")]
    [SerializeField]float jumpAcceleration = 7f;
    [SerializeField]AnimationCurve jumpX;
    [SerializeField]AnimationCurve jumpY;
    [SerializeField]AnimationCurve jumpZ;

    [SerializeField]float jumpDuration = 1f;
    [SerializeField]float minimumJumpDuration = .1f;
    [SerializeField]float jumpTimer;

    [Header("Falling")]
    [SerializeField]float gravitationalConstant = 9.82f;
    [SerializeField]float fallDuration;

    [SerializeField]bool isGrounded;
    [SerializeField]bool wasGroundedLastFrame;

    [SerializeField]bool isJumping = false;

    RaycastHit lastGroundHit;

    [Header("Combat Mode")]
    [SerializeField]float combatInterpolationSpeed = 2.5f;

    [Header("Equipment")]
    [SerializeField]WeaponController weapon;

    float currentHeight { get { return topPoint.position.y - this.transform.position.y; } }
    [SerializeField]float debugHeight;

    Animator animator;
    

    void Awake()
    {
        animator = this.GetComponentInChildren<Animator>();
    }
    void Update()
    {
        UpdateGroundedStatus();
        GatherInput();

        if (isJumping)
        {
            jumpTimer += Time.deltaTime;

            if (jumpTimer >= jumpDuration || (isGrounded && jumpTimer >= minimumJumpDuration))
            {
                jumpTimer = 0f;
                isJumping = false;
            }
        }

        debugHeight = currentHeight;

        CorrectStance();
        UpdateAnimator();
        UpdateRotation();
    }
    void OnAnimatorMove()
    {
        Vector3 velocity = this.animator.deltaPosition;

        //apply gravity
        velocity += Vector3.down * gravitationalConstant * Time.deltaTime;
        //apply force from any ground we're on
        velocity += velocity.GetNormalForce(lastGroundHit.normal);
        //applicera jump
        velocity += GetJumpVelocity() * jumpAcceleration * Time.deltaTime;

        //snapa mot marken ifall vi precis landat så att vi inte flyter groundDistance ovanför
        if (!wasGroundedLastFrame && isGrounded)
            velocity += Vector3.down * groundCheckDistance;

        /* Väggkollision */
        // Vi kollar detta sist så att vi inte råkar förflytta karaktären efter att vi har kollat för kollision
        Vector3 pointA = this.transform.position + (Vector3.up * currentHeight);
        Vector3 pointB = this.transform.position + (Vector3.up * stepOverHeight);

        //Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out RaycastHit hit, velocity.magnitude);

        //while (hit.transform != null)
        //{
        //    velocity += velocity.GetNormalForce(hit.normal);

        //    Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out hit, velocity.magnitude);
        //}

        //behöver göra detta rekursivt, annars kan vi glitcha under minskande slopes
        if (Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out RaycastHit hit, velocity.magnitude))
        {
            Debug.Log("checkCollision 1");
            velocity += checkCollision(velocity.GetNormalForce(hit.normal), pointA, pointB);
        }

        this.transform.position += velocity;
    }
    public Vector3 checkCollision(Vector3 velocity, Vector3 pointA, Vector3 pointB)
    {
        if (Physics.CapsuleCast(pointA, pointB, collisionRadius, velocity.normalized, out RaycastHit hit, velocity.magnitude))
        {
            Debug.Log("checkCollision 2");
            return velocity += checkCollision(velocity.GetNormalForce(hit.normal), pointA, pointB);
        }
        else
        {
            Debug.Log("checkCollision FINE");
            return velocity;
        }
    }
    void LateUpdate()
    {
        wasGroundedLastFrame = isGrounded;
    }

    void GatherInput()
    {
        targetInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        isSprinting = Input.GetKey(KeyCode.LeftShift);
        targetStance = Input.GetKey(KeyCode.C) ? 0f : 1f;

        if (!isGrounded)
            return;

        if (Input.GetKey(KeyCode.Mouse0))
            AttemptFire();
        if (Input.GetKeyDown(KeyCode.R))
            Reload();
        if (Input.GetKeyDown(KeyCode.F))
        {
            for (int i = 0; i < torches.Length; i++)
                torches[i].SetActive(!torches[i].activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            Jump();
        if (Input.GetKeyDown(KeyCode.V))
            animator.SetTrigger("roll");
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
            if (stance < targetStance)
                targetStance = stance;
        }
    }
    void UpdateAnimator()
    {
        animator.SetFloat("x", targetInput.x, combatInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("z", targetInput.z, combatInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("velocity", animator.velocity.magnitude);
        animator.SetFloat("stance", targetStance, combatInterpolationSpeed, Time.deltaTime);

        animator.SetFloat("fallDuration", fallDuration);

        animator.SetBool("isGrounded", isGrounded);
    }
    void UpdateRotation()
    {
        this.transform.rotation = Quaternion.Euler(0f, jig.transform.eulerAngles.y, 0f);
    }

    void UpdateGroundedStatus()
    {
        fallDuration += (isGrounded || isJumping) ? 0f : Time.deltaTime;

        Ray ray = new Ray(this.transform.position + (Vector3.up * stepOverHeight), Vector3.down);
        //offsetta lite uppåt för att få en mer reliable ground check
        isGrounded = Physics.Raycast(ray, out lastGroundHit, stepOverHeight + groundCheckDistance);

        if (wasGroundedLastFrame && !isGrounded)
            fallDuration = 0f;

        //Debug.DrawRay(ray.origin, ray.direction * (characterStepOverHeight + groundCheckDistance), isGrounded ? Color.green : Color.red);
    }

    void AttemptFire()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
        weapon.Shoot(hit.transform != null ? hit.point : ray.GetPoint(300f));
    }
    void Reload()
    {
        weapon.Reload();
    }
    void Jump()
    {
        animator.SetTrigger("jump");
        isJumping = true;
    }

    Vector3 GetJumpVelocity()
    {
        if (!isJumping)
            return Vector3.zero;

        //använder kurvor för att få bättre feel i hoppet
        //behöver multiplicera med nuvarande velocity på något sätt här
        //så att spelaren inte hoppar framåt om hen inte har framåtrörelse
        //men för trött för den matten atm
        return (this.transform.right * jumpX.Evaluate(jumpTimer)) +
                (this.transform.up * jumpY.Evaluate(jumpTimer)) +
                (this.transform.forward * jumpZ.Evaluate(jumpTimer));
    }
}
