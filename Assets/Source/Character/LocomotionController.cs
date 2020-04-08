using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]GameObject torch;
    [SerializeField]Transform jig;

    [SerializeField]Vector3 targetInput;

    [SerializeField]float targetStance;
    [SerializeField]bool isSprinting;

    [Header("Movement/Collision Properties")]
    [SerializeField]float characterStandingHeight = 1.8f;
    [SerializeField]float characterCrouchingHeight = 1.4f;
    [SerializeField]float characterStepOverHeight = .25f;
    [SerializeField]float characterRadius = .25f;
    [SerializeField]float groundCheckDistance = 1f;
    [SerializeField]bool canStand = true;

    [Header("Jumping")]
    [SerializeField]Vector3 jumpForce = new Vector3(1f, 1f, 0f);
    [SerializeField]float jumpDuration = 1f;
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

            if(jumpTimer >= jumpDuration || isGrounded)
            {
                jumpTimer = 0f;
                isJumping = false;
            }
        }

        UpdateAnimator();
        UpdateRotation();
    }
    void OnAnimatorMove()
    {
        Vector3 movement = Vector3.zero;
        movement += this.animator.deltaPosition;

        /****** Vägg-tak-kollision ******/
        Vector3 pointA;
        Vector3 pointB;
        if (targetStance == 1f) // Standing
        {
            pointA = this.transform.position + (Vector3.up * (characterStandingHeight - characterRadius));
            pointB = this.transform.position + (Vector3.up * characterStepOverHeight);
            if (Physics.CapsuleCast(pointA, pointB, characterRadius, this.animator.deltaPosition.normalized, out RaycastHit hit, this.animator.deltaPosition.magnitude + .2f))
                movement += this.animator.deltaPosition.GetNormalForce(hit.normal);
        }
        else if(targetStance == 0f) // Crouching
        {
            pointA = this.transform.position + (Vector3.up * (characterCrouchingHeight - characterRadius));
            pointB = this.transform.position + (Vector3.up * characterRadius);
            if (Physics.CapsuleCast(pointA, pointB, characterRadius, this.animator.deltaPosition.normalized, out RaycastHit hit, this.animator.deltaPosition.magnitude + .2f))
                movement += this.animator.deltaPosition.GetNormalForce(hit.normal);
        }
        /***** CanStand? *****/
        pointA = this.transform.position + (Vector3.up * (characterCrouchingHeight - characterRadius - .2f));
        pointB = this.transform.position + (Vector3.up * characterRadius);
        if (Physics.CapsuleCast(pointA, pointB, characterRadius, Vector3.up, characterStandingHeight - characterCrouchingHeight))
        {
            animator.SetFloat("stance", 0f, combatInterpolationSpeed, Time.deltaTime);
            canStand = false;
        }
        else
            canStand = true;
        /****** Vägg-tak-kollision ******/

        Vector3 gravity = Vector3.down * gravitationalConstant * Time.deltaTime;

        if (isGrounded)
            movement += movement.GetNormalForce(lastGroundHit.normal);
        else
            movement += gravity;

        this.transform.position += movement;

        if (isGrounded && !isJumping)
        {
            Vector3 temp = this.transform.position;
            temp.y = lastGroundHit.point.y;
            this.transform.position = temp;
        }
    }
    void LateUpdate()
    {
        wasGroundedLastFrame = isGrounded;
    }

    void GatherInput()
    {
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        targetInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        targetStance = Input.GetKey(KeyCode.C) ? 0f : 1f;

        if (!isGrounded)
            return;

        if (Input.GetKey(KeyCode.Mouse0))
            AttemptFire();
        if (Input.GetKeyDown(KeyCode.R))
            Reload();
        if (Input.GetKeyDown(KeyCode.F))
            torch.SetActive(!torch.activeSelf);
        if (Input.GetKeyDown(KeyCode.Space))
            Jump();
        if (Input.GetKeyDown(KeyCode.V))
            animator.SetTrigger("roll");

        if (isSprinting)
            return;
    }

    void UpdateRotation()
    {
        this.transform.rotation = Quaternion.Euler(0f, jig.transform.eulerAngles.y, 0f);
    }
    void UpdateAnimator()
    {
        animator.SetFloat("x", targetInput.x, combatInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("z", targetInput.z, combatInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("velocity", animator.velocity.magnitude);
        if(canStand)
            animator.SetFloat("stance", targetStance, combatInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("fallDuration", fallDuration);

        animator.SetBool("isGrounded", isGrounded);
    }
    void UpdateGroundedStatus()
    {
        fallDuration += isGrounded ? 0f : Time.deltaTime;

        Ray ray = new Ray(this.transform.position + (Vector3.up * characterStepOverHeight), Vector3.down);
        //offsetta lite uppåt för att få en mer reliable ground check
        isGrounded = Physics.Raycast(ray, out lastGroundHit, characterStepOverHeight + groundCheckDistance);

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
}
