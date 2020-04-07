using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]GameObject torch;
    [SerializeField]Transform jig;

    [SerializeField]Vector3 targetInput;

    [SerializeField]float targetStance;

    [SerializeField]float groundedRaycastDistance = .2f;
    [SerializeField]bool isGrounded;
    [SerializeField]bool isSprinting;

    [Header("Combat Mode")]
    [SerializeField]float explorationInterpolationSpeed = 2.5f;
    [SerializeField]float explorationRotationSpeed = 5f;

    [Header("Exploration Mode")]
    [SerializeField]float combatInterpolationSpeed = 10f;

    bool inCombatMode = true; 

    [Header("Equipment")]
    [SerializeField]WeaponController weapon;

    Animator animator;

    [SerializeField]bool displayDebugGizmos = true;

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
    }

    void GatherInput()
    {
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        
        targetInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if (isSprinting)
            targetInput *= 2;

        targetStance = Input.GetKey(KeyCode.C) ? 0f : 1f;

        if (!isGrounded)
            return;

        //if (Input.GetKeyDown(KeyCode.Tab))
        //    ToggleMovementMode();
        if (Input.GetKeyDown(KeyCode.F))
            torch.SetActive(!torch.activeSelf);
        if (Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger("roll");

        if (isSprinting)
            return;

        if (inCombatMode && Input.GetKey(KeyCode.Mouse0))
            weapon.Shoot();
    }

    void UpdateRotation()
    {
        //RaycastHit hit;

        //if (Physics.Raycast(new Ray(this.transform.position, Vector3.down), out hit))
        //    this.transform.position += new Vector3(0f, hit.distance, 0f);

        if (inCombatMode)
            this.transform.rotation = Quaternion.Euler(0f, jig.transform.eulerAngles.y, 0f);
        else
            this.transform.rotation = Quaternion.Euler(0f, this.transform.eulerAngles.y + (targetInput.x * explorationRotationSpeed), 0f);
        //this.transform.position += animator.deltaPosition;

        //Vector3 movement = targetInput.normalized;
        //this.transform.position += ((movement.x * jig.right) + (movement.z * jig.forward)) * (targetInput.normalized.magnitude * movementSpeed) * Time.deltaTime;
    }
    void UpdateAnimator()
    {
        animator.SetFloat("x", targetInput.x, inCombatMode ? combatInterpolationSpeed : explorationInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("z", targetInput.z, inCombatMode ? combatInterpolationSpeed : explorationInterpolationSpeed, Time.deltaTime);

        animator.SetFloat("stance", targetStance, combatInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("fallHeight", isGrounded ? 0f : animator.GetFloat("fallHeight") + Time.deltaTime);

        animator.SetBool("isGrounded", isGrounded);
    }
    void UpdateGroundedStatus()
    {
        //offsetta lite uppåt för att få en mer reliable ground check
        Ray ray = new Ray(this.transform.position + (Vector3.up * .1f), Vector3.down);
        RaycastHit hit;

        isGrounded = Physics.Raycast(ray, out hit, groundedRaycastDistance + .1f);

        if (isGrounded)
        {
            Vector3 np = this.transform.position;
            np.y = hit.point.y;

            this.transform.position = np;
        }

        if(displayDebugGizmos)
            Debug.DrawLine(this.transform.position + (Vector3.up * .1f), this.transform.position + (Vector3.down * groundedRaycastDistance), hit.transform == null ? Color.red : Color.green);
    }

    void ToggleMovementMode()
    {
        inCombatMode = !inCombatMode;
        animator.SetBool("inCombat", inCombatMode);
    }
    void Jump()
    {

    }
}
