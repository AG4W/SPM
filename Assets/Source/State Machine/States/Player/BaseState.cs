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

    protected override void OnInitialize()
    {
        base.OnInitialize();

        //lefthand
        Transform lht = ((WeaponController)base.Context["weapon"]).LeftHandIKTarget;

        if (lht == null)
            return;

        base.Actor.Raise(ActorEvent.SetActorLeftHandTarget, lht);
        base.Actor.Raise(ActorEvent.SetActorLeftHandWeight, 1f);
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void Tick()
    {
        UpdateAnimatorIK();

        base.Actor.Raise(ActorEvent.SetActorTargetInput, new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")));
        // Ground Check
        base.Actor.Raise(ActorEvent.UpdateActorGroundedStatus);
        //Gravity
        base.Actor.Raise(ActorEvent.ModifyActorVelocity, Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale));

        //rotation
        base.Actor.transform.rotation = Quaternion.Slerp(base.Actor.transform.rotation, Quaternion.Euler(0f, jig.transform.eulerAngles.y, 0f), rotationSpeed * (Time.deltaTime / Time.timeScale));
    }

    void UpdateAnimatorIK()
    {
        //Look at
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit))
            base.Actor.Raise(ActorEvent.SetActorLookAtPosition, hit.point);
        else
            base.Actor.Raise(ActorEvent.SetActorLookAtPosition, ray.GetPoint(10f));

        Debug.DrawLine(base.Actor.FocusPoint.position, hit.point == null ? ray.GetPoint(10f) : hit.point, Color.magenta);
        base.Actor.Raise(ActorEvent.SetActorLookAtWeights, new float[] { total, body, head, eyes, clamp });
    }
}

