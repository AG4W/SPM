using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]GameObject torch;
    [SerializeField]Transform jig;

    [SerializeField]Vector3 targetInput;
    [SerializeField]Vector3 correctedInput;

    [SerializeField]float targetStance;

    [SerializeField]bool isGrounded;
    [SerializeField]bool isSprinting;

    [Header("Movement/Collision Properties")]
    [SerializeField]float characterHeight = 1.8f;
    [SerializeField]float characterRadius = .25f;

    [Header("Combat Mode")]
    [SerializeField]float explorationInterpolationSpeed = 2.5f;
    [SerializeField]float explorationRotationSpeed = 5f;

    [Header("Exploration Mode")]
    [SerializeField]float combatInterpolationSpeed = 10f;

    bool inCombatMode = true; 

    [Header("Equipment")]
    [SerializeField]WeaponController weapon;

    [Header("Collision Management")]
    [SerializeField]LayerMask collisionLayer;

    Animator animator;

    void Awake()
    {
        animator = this.GetComponentInChildren<Animator>();
    }
    void Update()
    {
        //UpdateGroundedStatus();
        GatherInput();

        UpdateAnimator();
        UpdateRotation();
    }

    void OnAnimatorMove()
    {
        this.transform.position += this.animator.deltaPosition;

        Debug.DrawRay(this.transform.position, this.animator.deltaPosition);

        if (Physics.CapsuleCast(this.transform.position, this.transform.position + (Vector3.up * characterHeight), characterRadius, this.animator.deltaPosition.normalized, out RaycastHit hit, this.animator.deltaPosition.magnitude + .5f))
            this.transform.position += this.animator.deltaPosition.GetNormalForce(hit.normal);
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

        if (inCombatMode && Input.GetKey(KeyCode.Mouse0))
            weapon.Shoot();
    }

    void UpdateRotation()
    {
        this.transform.rotation = Quaternion.Euler(0f, jig.transform.eulerAngles.y, 0f);
    }
    void UpdateAnimator()
    {
        animator.SetFloat("x", targetInput.x, inCombatMode ? combatInterpolationSpeed : explorationInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("z", targetInput.z, inCombatMode ? combatInterpolationSpeed : explorationInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("velocity", animator.velocity.magnitude);

        animator.SetFloat("stance", targetStance, combatInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("fallHeight", isGrounded ? 0f : animator.GetFloat("fallHeight") + Time.deltaTime);

        animator.SetBool("isGrounded", isGrounded);
    }
    //void UpdateGroundedStatus()
    //{
    //    //offsetta lite uppåt för att få en mer reliable ground check
    //    Ray ray = new Ray(this.transform.position + (Vector3.up * .1f), Vector3.down);
    //    RaycastHit hit;

    //    isGrounded = Physics.Raycast(ray, out hit, groundedRaycastDistance + .1f);

    //    if (isGrounded)
    //    {
    //        Vector3 np = this.transform.position;
    //        np.y = hit.point.y;

    //        this.transform.position = np;
    //    }

    //    if(displayDebugGizmos)
    //        Debug.DrawLine(this.transform.position + (Vector3.up * .1f), this.transform.position + (Vector3.down * groundedRaycastDistance), hit.transform == null ? Color.red : Color.green);
    //}

    void Jump()
    {
        animator.SetTrigger("jump");
        this.transform.position += (this.transform.up + this.transform.forward) * 5f;
    }
}
