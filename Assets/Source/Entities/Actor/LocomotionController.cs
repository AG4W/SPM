using UnityEngine;

using System.Collections.Generic;
public class LocomotionController : HumanoidActor
{
    [SerializeField]GameObject[] torches;

    Transform jig;

    protected override void Initalize()
    {
        base.Initalize();

        jig = FindObjectOfType<CameraController>().transform;

        if (jig == null)
            Debug.LogError("LocomotionController could not find Camera Jig, did you forget to drag the prefab into your scene?");

        //flytta detta till en separat controller sen, borde nog inte vara här
        GlobalEvents.Subscribe(GlobalEvent.ToggleTorches, (object[] args) =>
        {
            for (int i = 0; i < torches.Length; i++)
                torches[i].SetActive(!torches[i].activeSelf);
        });

        base.Animator.SetBool("isAlert", true);
    }
    protected override StateMachine InitializeStateMachine()
    {
        return new StateMachine(this,
            Resources.LoadAll<State>(base.Path),
            new Dictionary<string, object>
            {
                ["jig"] = FindObjectOfType<CameraController>(),
                ["animator"] = this.GetComponent<Animator>(),
                ["actor"] = this,
                ["weapon"] = base.Weapon,
            },
            typeof(IdleState));
    }

    protected override void Update()
    {
        base.Update();

        DispatchInput();
    }

    void DispatchInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            GlobalEvents.Raise(GlobalEvent.Jump); 
        if (Input.GetKeyDown(KeyCode.V))
            GlobalEvents.Raise(GlobalEvent.Roll);
        if (Input.GetKeyDown(KeyCode.R))
            this.Raise(ActorEvent.ReloadActorWeapon);
        if (Input.GetKeyDown(KeyCode.F))
            GlobalEvents.Raise(GlobalEvent.ToggleTorches);
    }

    //void CorrectStance()
    //{
    //    Physics.Raycast(this.transform.position, Vector3.up, out RaycastHit hit, base.MaxHeight);

    //    //om vi träffar något och om vi är mellan dem
    //    if (hit.transform && hit.distance <= base.MaxHeight)
    //    {
    //        //hämta vart vi är mellan minHöjd och maxHöjd <-- RELATIVT i skala 0 .. 1
    //        float location = Mathf.InverseLerp(base.MinHeight, base.MaxHeight, hit.distance);
    //        //översätt den platsen till vår skala för animatorns crouch
    //        float stance = Mathf.Lerp(0f, 1f, location);

    //        //skriv bara över targetStance ifall vår stance är lägre, annars låter vi spelaren croucha fritt.
    //        if (stance < actualStance)
    //            actualStance = stance;
    //    }
    //}

}
public enum MovementMode
{
    Crouch,
    Walk,
    Jog,
    Sprint
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
