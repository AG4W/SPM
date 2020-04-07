using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]GameObject torch;
    [SerializeField]Transform jig;

    [SerializeField]Vector3 targetInput;

    [SerializeField]float targetStance;
    [SerializeField]bool isSprinting;

    [Header("Movement/Collision Properties")]
    [SerializeField]float characterHeight = 1.8f;
    [SerializeField]float characterStepOverHeight = .25f;
    [SerializeField]float characterRadius = .25f;
    [SerializeField]float groundCheckDistance = 1f;
    [SerializeField]float jumpForce = 2f;

    [SerializeField]float gravitationalConstant = 9.82f;
    float fallDuration;

    [SerializeField]bool isGrounded;
    bool lastFrameGroundStatus;

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

        UpdateAnimator();
        UpdateRotation();

        lastFrameGroundStatus = isGrounded;
    }
    void OnAnimatorMove()
    {
        Vector3 movement = Vector3.zero;

        movement += this.animator.deltaPosition;
        //Debug.DrawRay(this.transform.position, this.animator.deltaPosition.normalized * (this.animator.deltaPosition.magnitude + .2f), Color.blue);

        Vector3 pointA = this.transform.position + (Vector3.up * characterHeight);
        Vector3 pointB = this.transform.position + (Vector3.up * characterStepOverHeight);

        if (Physics.CapsuleCast(pointA, pointB, characterRadius, this.animator.deltaPosition.normalized, out RaycastHit hit, this.animator.deltaPosition.magnitude + .2f))
            movement += this.animator.deltaPosition.GetNormalForce(hit.normal);

        Vector3 gravity = Vector3.down * gravitationalConstant * Time.deltaTime;

        if (isGrounded)
            movement += gravity.GetNormalForce(lastGroundHit.normal);
        else
            movement += gravity;

        this.transform.position += movement;
    }

    void GatherInput()
    {
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        targetInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        targetStance = Input.GetKey(KeyCode.C) ? 0f : 1f;

        if (!isGrounded)
            return;

        if (Input.GetKeyDown(KeyCode.F))
            torch.SetActive(!torch.activeSelf);
        if (Input.GetKeyDown(KeyCode.Space))
            Jump();
        if (Input.GetKeyDown(KeyCode.V))
            animator.SetTrigger("roll");

        if (isSprinting)
            return;

        if (Input.GetKey(KeyCode.Mouse0))
            weapon.Shoot();
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

        if (lastFrameGroundStatus && !isGrounded)
            fallDuration = 0f;

        //Debug.DrawRay(ray.origin, ray.direction * (characterStepOverHeight + groundCheckDistance), isGrounded ? Color.green : Color.red);
    }

    void Jump()
    {
        animator.SetTrigger("jump");
        this.transform.position += this.transform.up * jumpForce; 
    }
}
