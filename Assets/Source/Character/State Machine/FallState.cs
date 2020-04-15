﻿using UnityEngine;

public class FallState : BaseState
{
    [SerializeField]Vector3 velocityBeforeLosingGroundContact;

    [SerializeField]float fallForwardVelocityDivider = 1.8f;
    [SerializeField]float fallForwardMomentDeceleration = .5f;

    public override void Enter()
    {
        base.Enter();

        velocityBeforeLosingGroundContact = base.Controller.Velocity;
        velocityBeforeLosingGroundContact.y = 0f;
    }
    public override void Tick()
    {
        base.Tick();

        if (velocityBeforeLosingGroundContact.magnitude > .01f)
            velocityBeforeLosingGroundContact = Vector3.Lerp(velocityBeforeLosingGroundContact, Vector3.zero, fallForwardMomentDeceleration * (Time.deltaTime / Time.timeScale));

        GlobalEvents.Raise(GlobalEvent.ModifyPlayerVelocity, velocityBeforeLosingGroundContact / fallForwardVelocityDivider);
    }
}
