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

    LayerMask ikMask;

    protected PlayerActor Player => (PlayerActor)base.Actor;

    public float DistanceToGround
    {
        get
        {
            if (this.Actor.IsGrounded)
                return 0f;
            else
            {
                Physics.Raycast(base.Actor.transform.position + (Vector3.up * .25f), Vector3.down, out RaycastHit hit, Mathf.Infinity);

                return hit.transform != null ? base.Actor.transform.position.DistanceTo(hit.point) : Mathf.Infinity;
            }
        }
    }
    public float GravitationalConstant { get { return gravitationalConstant; } }
    public float AirResistance { get { return airResistance; } }

    new public HumanoidActor Actor { get { return base.Actor as HumanoidActor; } }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        UpdateIKTarget();
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void Tick()
    {
        base.Actor.Raise(ActorEvent.SetTargetInput, new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")));
        // Ground Check
        base.Actor.Raise(ActorEvent.UpdateGroundedStatus);
        //Gravity
        base.Actor.Raise(ActorEvent.ModifyVelocity, Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale));

        //rotation
        base.Actor.transform.rotation = Quaternion.Slerp(base.Actor.transform.rotation, Quaternion.Euler(0f, base.Get<CameraController>().transform.eulerAngles.y, 0f), rotationSpeed * (Time.deltaTime / Time.timeScale));

        UpdateAnimatorIK();
    }
    void UpdateAnimatorIK()
    {
        //Look at
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, LayerMask.NameToLayer("Default")))
            base.Actor.Raise(ActorEvent.SetLookAtPosition, hit.point);
        else
            base.Actor.Raise(ActorEvent.SetLookAtPosition, ray.GetPoint(10f));

        Debug.DrawLine(base.Actor.FocusPoint.position, hit.point == null ? ray.GetPoint(10f) : hit.point, Color.magenta);
        base.Actor.Raise(ActorEvent.SetLookAtWeights, new float[] { total, body, head, eyes, clamp });
    }
    void UpdateIKTarget()
    {
        base.Actor.Raise(ActorEvent.SetLeftHandTarget, base.Get<WeaponController>().LeftHandIKTarget);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 1f);
    }
}

