using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    [SerializeField]Transform jig;

    [SerializeField]Vector3 targetInput;

    [SerializeField]float movementSpeed = 2f;

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
        targetInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
    }

    void UpdatePosition()
    {
        this.transform.rotation = Quaternion.Euler(0f, jig.transform.eulerAngles.y, 0f);
        //this.transform.position += animator.deltaPosition;

        //Vector3 movement = targetInput.normalized;
        //this.transform.position += ((movement.x * jig.right) + (movement.z * jig.forward)) * (targetInput.normalized.magnitude * movementSpeed) * Time.deltaTime;
    }
    void UpdateAnimator()
    {
        animator.SetFloat("x", targetInput.x);
        animator.SetFloat("z", targetInput.z);
    }
}
