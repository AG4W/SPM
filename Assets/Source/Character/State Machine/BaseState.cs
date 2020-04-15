using UnityEngine;

public class BaseState : State
{
    [SerializeField]float gravitationalConstant = 9.82f;
    [SerializeField]float airResistance = .5f;

    public float GravitationalConstant { get { return gravitationalConstant; } }
    public float AirResistance { get { return airResistance; } }

    public override void Initialize()
    {
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void Tick()
    {
        GlobalEvents.Raise(GlobalEvent.UpdatePlayerRotation);
        GlobalEvents.Raise(GlobalEvent.SetTargetInput, new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")));
        GlobalEvents.Raise(GlobalEvent.SetTargetStance, Input.GetKey(KeyCode.C) ? Stance.Crouched : Stance.Standing);
        // Ground Check
        GlobalEvents.Raise(GlobalEvent.UpdatePlayerGroundedStatus);

        //Gravity
        GlobalEvents.Raise(GlobalEvent.ModifyPlayerVelocity, Vector3.down * gravitationalConstant * (Time.deltaTime / Time.timeScale));
    }
    public override void Exit()
    {
    }
}
