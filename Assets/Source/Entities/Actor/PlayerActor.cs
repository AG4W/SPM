using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerActor : HumanoidActor
{
    [SerializeField]Light[] torches;
    Checkpoint[] checkpoints;

    Transform primaryAnchorPoint;

    Transform jig;

    [Header("Player Actor Collision Settings")]
    [Range(0f, 1f)][SerializeField]float overlapRadiusModifier = 0.9f;

    protected override void Initalize()
    {
        checkpoints = FindObjectsOfType<Checkpoint>();
        Debug.Assert(checkpoints != null && checkpoints.Length > 0, "Could not find any checkpoints, did you forget to drag the prefab into your scene?", this.gameObject);
        
        jig = FindObjectOfType<CameraController>().transform;
        Debug.Assert(jig != null, "LocomotionController could not find Camera Jig, did you forget to drag the prefab into your scene?");

        primaryAnchorPoint = this.transform.FindRecursively("primaryAnchorPoint");

        torches = new Light[] {
            this.transform.FindRecursively("torch").GetComponentInChildren<Light>() 
        };

        this.Subscribe(ActorEvent.ShotHit, (object[] args) => GlobalEvents.Raise(GlobalEvent.PlayerShotHit, args));
        this.Subscribe(ActorEvent.ShotMissed, (object[] args) => GlobalEvents.Raise(GlobalEvent.PlayerShotMissed, args));

        //flytta detta till en separat controller sen, borde nog inte vara här
        GlobalEvents.Subscribe(GlobalEvent.ToggleTorches, (object[] args) =>
        {
            for (int i = 0; i < torches.Length; i++)
                torches[i].enabled = !torches[i].enabled;
        });
        GlobalEvents.Subscribe(GlobalEvent.SetPlayerWeapon, (object[] args) => {
            //rensa gammal modell
            for (int i = 0; i < primaryAnchorPoint.childCount; i++)
                Destroy(primaryAnchorPoint.GetChild(i).gameObject);

            Player.SetWeapon(WeaponSlot.Primary, this.WeaponController.Weapon);

            if (Player.GetWeapon(WeaponSlot.Primary) != null)
            {
                GameObject g = Instantiate(Player.GetWeapon(WeaponSlot.Primary).Prefab, primaryAnchorPoint.position, primaryAnchorPoint.rotation, primaryAnchorPoint);

                //disable stuff
                g.transform.FindRecursively("muzzleFlash").gameObject.SetActive(false);
                g.transform.FindRecursively("world UI").gameObject.SetActive(false);

                //remove ui controller
                Destroy(g.GetComponentInChildren<WeaponWorldUIController>());
            }

            this.Raise(ActorEvent.SetWeapon, args[0]);
        });

        base.Initalize();
        //cache current weapon in savedata
        //Player.SetWeapon(WeaponSlot.Primary, this.WeaponController.Weapon);
    }
    protected override StateMachine InitializeStateMachine()
    {
        return new StateMachine(this,
            Resources.LoadAll<State>(base.Path),
            new Dictionary<Type, object>
            {
                [typeof(CameraController)] = FindObjectOfType<CameraController>(),
                [typeof(Animator)] = this.GetComponent<Animator>(),
                [typeof(Actor)] = this,
                [typeof(WeaponController)] = base.WeaponController,
            },
            typeof(IdleState));
    }

    protected override void Update()
    {
        base.Update();
        DispatchInput();
        //Debug.DrawRay(this.transform.position + (Vector3.up * 1.8f), this.Velocity);
    }
    protected override void OnAnimatorMove()
    {
        base.OnAnimatorMove();
    }
    protected override void OnAnimatorIK(int layerIndex)
    {
        base.OnAnimatorIK(layerIndex);
    }
    private void OnDrawGizmos()
    {
        //(Krulls)
        //Gizmos.DrawSphere(this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier)), base.CollisionRadius);
        //Gizmos.DrawSphere(this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier)), base.CollisionRadius);
        //Gizmos.DrawSphere(base.pointAsphere.position, base.CollisionRadius);
        //Gizmos.DrawSphere(base.pointBsphere.position, base.CollisionRadius);
    }
    void DispatchInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
            GlobalEvents.Raise(GlobalEvent.ToggleTorches);
        if (Input.GetKey(KeyCode.O))
            GlobalEvents.Raise(GlobalEvent.ModifyCameraTrauma, 1f);
        if(Input.GetKeyDown(KeyCode.Z))
            GlobalEvents.Raise(GlobalEvent.SetPlayerWeapon, this.WeaponController.Weapon == null ? Player.GetWeapon(WeaponSlot.Primary) : null);
    }

    protected override void CheckOverlap()
    {
        Vector3 pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier));
        Vector3 pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier));
        //(Krulls)
        //Vector3 pointA = base.pointAsphere.position;
        //Vector3 pointB = base.pointBsphere.position;

        Vector3 closestPoint;
        Vector3 hitDirection;
        float hitDist;

        bool overlapCheckA = Physics.CheckSphere(pointA, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);
        bool overlapCheckB = Physics.CheckSphere(pointB, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);

        int counter = 0;
        while (overlapCheckA == true || overlapCheckB == true)
        {
            Collider[] overlapCollidersA = Physics.OverlapSphere(pointA, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);
            if (overlapCollidersA.Length > 0)
                for (int i = 0; i < overlapCollidersA.Length; i++)
                {
                    closestPoint = overlapCollidersA[i].ClosestPoint(pointA); // punkt i den överlappande collidern som är närmast centrum på sfären

                    hitDist = Vector3.Distance(pointA, closestPoint);
                    hitDirection = closestPoint - pointA;

                    this.transform.position += -hitDirection.normalized * (base.CollisionRadius * overlapRadiusModifier - hitDist + base.SkinWidth); // Vi vill flytta oss bakåt: radien på sfären minus distans

                    this.Velocity += this.Velocity.GetNormalForce(-hitDirection.normalized); // Applicera normalkraft 

                    // Uppdatera pointA/B
                    pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier));
                    pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier));
                    //(Krulls)
                    //pointA = base.pointAsphere.position;
                    //pointB = base.pointBsphere.position;
                }

            Collider[] overlapCollidersB = Physics.OverlapSphere(pointB, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);
            if (overlapCollidersB.Length > 0)
                for (int i = 0; i < overlapCollidersB.Length; i++)
                {
                    closestPoint = overlapCollidersB[i].ClosestPoint(pointB);

                    hitDist = Vector3.Distance(pointB, closestPoint);
                    hitDirection = closestPoint - pointB;

                    this.transform.position += -hitDirection.normalized * (base.CollisionRadius * overlapRadiusModifier - hitDist + base.SkinWidth);

                    this.Velocity += this.Velocity.GetNormalForce(-hitDirection.normalized);

                    pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier));
                    pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier));
                    //pointA = base.pointAsphere.position;
                    //pointB = base.pointBsphere.position;
                }

            // Kolla overlap igen
            overlapCheckA = Physics.CheckSphere(pointA, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);
            overlapCheckB = Physics.CheckSphere(pointB, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);

            if (counter >= 100)
                break;
            counter++;
        }
    }
    protected override void OnHealthChanged(float change)
    {
        base.OnHealthChanged(change);
        GlobalEvents.Raise(GlobalEvent.PlayerHealthChanged, base.Health);
    }
    protected override void OnHealthZero()
    {
        //teleportera till nämrsta checkpoint
        this.transform.position = checkpoints.OrderBy(t => t.transform.position.DistanceTo(this.transform.position)).First().transform.position + Vector3.up;
        base.Health.Reset();
    }
}
public enum Stance
{
    Crouched,
    Standing,
}
public enum AimMode
{
    Default,
    IronSight,
}
