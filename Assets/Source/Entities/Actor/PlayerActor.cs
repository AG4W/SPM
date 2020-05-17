using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class PlayerActor : HumanoidActor
{
    [Header("Player Configuration")]
    [SerializeField]float maxForce = 200f;
    [SerializeField]float forceRegenerationAmount = .01f;

    Light[] torches;
    Checkpoint[] checkpoints;

    //animation setuff
    Transform primaryAnchorPoint;
    Transform jig;

    [Header("Player Actor Collision Settings")]
    [Range(.8f, 1f)][SerializeField]float overlapRadiusModifier = 0.9f;

    [Header("Debug Collision")]
    [SerializeField]bool drawCollisionSpheres = false;

    Collider[] overlaps;

    public Vital Force { get; private set; }

    protected override void Initalize()
    {
        this.Force = new Vital(VitalType.Force, maxForce, forceRegenerationAmount, false);
        this.Force.OnCurrentChanged += OnForceChanged;

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

        GlobalEvents.Subscribe(GlobalEvent.OnSceneExit, (object[] args) =>
        {
            Player.SetWeapon(WeaponSlot.Primary, this.WeaponController.Weapon);
        });

        base.Initalize();
        base.StateMachine.Initialize(this,
            Resources.LoadAll<State>(base.Path),
            new Dictionary<Type, object>
            {
                [typeof(CameraController)] = FindObjectOfType<CameraController>(),
                [typeof(Animator)] = this.GetComponent<Animator>(),
                [typeof(Actor)] = this,
                [typeof(WeaponController)] = base.WeaponController,
            },
            typeof(IdleState));

        GlobalEvents.Raise(GlobalEvent.SetPlayerWeapon, Player.GetWeapon(WeaponSlot.Primary));
        //cache current weapon in savedata
        //Player.SetWeapon(WeaponSlot.Primary, this.WeaponController.Weapon);
    }

    protected override void Update()
    {
        base.Update();
        DispatchInput();

        this.Force.Tick();
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
        if (drawCollisionSpheres)
        {
            Gizmos.DrawSphere(this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier)), base.CollisionRadius);
            Gizmos.DrawSphere(this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier)), base.CollisionRadius);
        }
    }
    void DispatchInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
            GlobalEvents.Raise(GlobalEvent.ToggleTorches);
        if (Input.GetKey(KeyCode.O))
            GlobalEvents.Raise(GlobalEvent.ModifyCameraTrauma, 1f);
        if(Input.GetKeyDown(KeyCode.Z))
            GlobalEvents.Raise(GlobalEvent.SetPlayerWeapon, this.WeaponController.Weapon == null ? Player.GetWeapon(WeaponSlot.Primary) : null);
        if (Input.GetKeyDown(KeyCode.K))
            base.Health.Update(-999999f);
    }

    protected override void CheckOverlap()
    {
        Vector3 pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier));
        Vector3 pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier));

        bool overlapCheckA = Physics.CheckSphere(pointA, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);
        bool overlapCheckB = Physics.CheckSphere(pointB, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);

        int counter = 0;
        while (overlapCheckA == true || overlapCheckB == true)
        {
            CheckOverlapPosition(ref pointA, ref pointA, ref pointB);
            CheckOverlapPosition(ref pointB, ref pointA, ref pointB);

            // Kolla overlap igen
            overlapCheckA = Physics.CheckSphere(pointA, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);
            overlapCheckB = Physics.CheckSphere(pointB, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);

            if (counter >= 100)
                break;
            counter++;
        }
    }
    private void CheckOverlapPosition(ref Vector3 point, ref Vector3 pointA, ref Vector3 pointB)
    {
        if (Physics.OverlapSphereNonAlloc(point, base.CollisionRadius * overlapRadiusModifier, overlaps, base.CollisionMask) > 0)
        {
            Vector3 closestPoint;
            Vector3 hitDirection;
            float hitDist;
            
            for (int i = 0; i < overlaps.Length; i++)
            {
                closestPoint = overlaps[i].ClosestPoint(point); // punkt i den överlappande collidern som är närmast centrum på sfären

                hitDist = Vector3.Distance(point, closestPoint);
                hitDirection = closestPoint - point;

                this.transform.position += -hitDirection.normalized * (base.CollisionRadius * overlapRadiusModifier - hitDist + base.SkinWidth); // Vi vill flytta oss bakåt: radien på sfären minus distans
                pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier));
                pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier));

                this.Velocity += this.Velocity.GetNormalForce(-hitDirection.normalized); // Applicera normalkraft 
            }
        }
    }

    protected override void OnHealthChanged(float change)
    {
        base.OnHealthChanged(change);
        GlobalEvents.Raise(GlobalEvent.PlayerHealthChanged, base.Health);
    }
    void OnForceChanged(float change)
    {
        GlobalEvents.Raise(GlobalEvent.PlayerForceChanged, this.Force);
    }
    protected override void OnHealthZero()
    {
        GlobalEvents.Raise(GlobalEvent.OnSceneExit, SceneManager.GetActiveScene());
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
