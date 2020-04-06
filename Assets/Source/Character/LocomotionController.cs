using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]GameObject torch;
    [SerializeField]Transform jig;

    [SerializeField]Vector3 targetInput;

    [SerializeField]float combatInterpolationSpeed = 10f;
    [SerializeField]float explorationInterpolationSpeed = 2.5f;

    [SerializeField]bool inCombatMode; 

    Animator animator;

    void Awake()
    {
        animator = this.GetComponentInChildren<Animator>();
    }
    void Update()
    {
        GatherInput();

        UpdateAnimator();
        UpdatePosition();
    }

    void GatherInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleMovementMode();
        if (Input.GetKeyDown(KeyCode.F))
            torch.SetActive(!torch.activeSelf);

        targetInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
    }

    void UpdatePosition()
    {
        //RaycastHit hit;

        //if (Physics.Raycast(new Ray(this.transform.position, Vector3.down), out hit))
        //    this.transform.position += new Vector3(0f, hit.distance, 0f);

        this.transform.rotation = Quaternion.Euler(0f, jig.transform.eulerAngles.y, 0f);
        //this.transform.position += animator.deltaPosition;

        //Vector3 movement = targetInput.normalized;
        //this.transform.position += ((movement.x * jig.right) + (movement.z * jig.forward)) * (targetInput.normalized.magnitude * movementSpeed) * Time.deltaTime;
    }
    void UpdateAnimator()
    {
        animator.SetFloat("x", targetInput.x, inCombatMode ? combatInterpolationSpeed : explorationInterpolationSpeed, Time.deltaTime);
        animator.SetFloat("z", targetInput.z, inCombatMode ? combatInterpolationSpeed : explorationInterpolationSpeed, Time.deltaTime);
    }

    void ToggleMovementMode()
    {
        inCombatMode = !inCombatMode;
        animator.SetTrigger("toggleMovementMode");
    }
}
