using UnityEngine;

public abstract class BaseState : State
{
    [Range(0f, 1f)][SerializeField]float total = 1f;
    [Range(0f, 1f)][SerializeField]float body = .25f;
    [Range(0f, 1f)][SerializeField]float head = 1f;
    [Range(0f, 1f)][SerializeField]float eyes = 1f;
    [Range(0f, 1f)][SerializeField]float clamp = .5f;
    float[] weights;

    [SerializeField]float rotationSpeed = 5f;

    [SerializeField]float gravitationalConstant = 9.82f;
    [SerializeField]float airResistance = .5f;

    [Header("Camera Settings")]
    [SerializeField]CameraSettings settings = new CameraSettings(50f, new Vector3(.6f, .4f, -1.1f), Vector3.zero);

    LayerMask ikMask;

    RaycastHit[] ikRaycastBuffer;
    Ray ray;

    protected PlayerActor Player => (PlayerActor)base.Actor;
    protected Animator Animator { get; private set; }

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
    public float GravitationalConstant => gravitationalConstant;
    public float AirResistance => airResistance;

    new public HumanoidActor Actor => (HumanoidActor)base.Actor;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        weights = new float[] { total, body, head, eyes, clamp };
        ikMask = LayerMask.NameToLayer("Default");
        ikRaycastBuffer = new RaycastHit[1];

        this.Animator = base.Get<Animator>();

        UpdateIKTarget();
    }
    public override void Enter()
    {
        base.Enter();

        GlobalEvents.Raise(GlobalEvent.SetCameraSettings, settings);
    }
    public override void Tick()
    {
        base.Actor.Raise(ActorEvent.SetTargetInput, new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")));
        // Ground Check
        //tror ej detta behövs längre?
        //base.Actor.Raise(ActorEvent.UpdateGroundedStatus);
        //Gravity
        base.Actor.Raise(ActorEvent.ModifyVelocity, Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale));
        //rotation
        base.Actor.transform.rotation = Quaternion.Slerp(base.Actor.transform.rotation, Quaternion.Euler(0f, base.Get<CameraController>().transform.eulerAngles.y, 0f), rotationSpeed * (Time.deltaTime / Time.timeScale));

        UpdateAnimatorIK();
    }
    void UpdateAnimatorIK()
    {
        //todo: Något här genererar också ett literal fuckton av garbage
        //blir glhf kul att tracea detta.

        //Look at
        ray = base.Camera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));

        if(Physics.RaycastNonAlloc(ray, ikRaycastBuffer, Mathf.Infinity, ikMask) > 0)
            base.Actor.Raise(ActorEvent.SetLookAtPosition, ikRaycastBuffer[0].point);
        else
            base.Actor.Raise(ActorEvent.SetLookAtPosition, ray.GetPoint(10f));

        //Debug.DrawLine(base.Actor.FocusPoint.position, hit.point == null ? ray.GetPoint(10f) : hit.point, Color.magenta);
        base.Actor.Raise(ActorEvent.SetLookAtWeights, weights);
    }
    void UpdateIKTarget()
    {
        base.Actor.Raise(ActorEvent.SetLeftHandTarget, base.Get<WeaponController>().LeftHandIKTarget);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 1f);
    }
}

