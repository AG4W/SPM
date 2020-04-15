using UnityEngine;

public abstract class BaseState : State
{
    [Range(0f, 1f)][SerializeField]float total = 1f;
    [Range(0f, 1f)][SerializeField]float body = .25f;
    [Range(0f, 1f)][SerializeField]float head = 1f;
    [Range(0f, 1f)][SerializeField]float eyes = 1f;
    [Range(0f, 1f)][SerializeField]float clamp = .5f;

    [SerializeField]float rotationSpeed = 5f;

    [SerializeField]float gravitationalConstant = 9.82f;
    [SerializeField]float airResistance = .5f;

    Transform jig { get { return ((CameraController)base.Context["jig"]).transform; } }

    public float GravitationalConstant { get { return gravitationalConstant; } }
    public float AirResistance { get { return airResistance; } }

    public override void Enter()
    {
        base.Enter();
    }
    public override void Tick()
    {
        UpdateAnimatorIK();

        ((Animator)base.Context["animator"]).SetFloat("x", base.Controller.ActualInput.x);
        ((Animator)base.Context["animator"]).SetFloat("z", base.Controller.ActualInput.z);

        GlobalEvents.Raise(GlobalEvent.SetTargetInput, new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")));
        // Ground Check
        GlobalEvents.Raise(GlobalEvent.UpdatePlayerGroundedStatus);

        //Gravity
        GlobalEvents.Raise(GlobalEvent.ModifyPlayerVelocity, Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale));

        //rotation
        base.Controller.transform.rotation = Quaternion.Slerp(base.Controller.transform.rotation, Quaternion.Euler(0f, jig.transform.eulerAngles.y, 0f), rotationSpeed * (Time.deltaTime / Time.timeScale));
    }

    void UpdateAnimatorIK()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit))
            GlobalEvents.Raise(GlobalEvent.SetPlayerLookAtPosition, hit.point);
        else
            GlobalEvents.Raise(GlobalEvent.SetPlayerLookAtPosition, ray.GetPoint(10f));

        Debug.DrawLine(((Actor)base.Context["actor"]).FocusPoint.position, hit.point == null ? ray.GetPoint(10f) : hit.point, Color.magenta);
        GlobalEvents.Raise(GlobalEvent.SetPlayerLookAtWeights, new float[] { total, body, head, eyes, clamp });
    }
}
