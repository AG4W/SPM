using UnityEngine;

public class BaseState : State
{
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
    public override void Exit()
    {
    }
}
