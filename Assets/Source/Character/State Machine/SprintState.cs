using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/SprintState")]
public class SprintState : BaseState
{
    public override void Enter() 
    {
        base.Enter();
    }
    public override void Exit() 
    {
        GlobalEvents.Raise(GlobalEvent.UpdatePlayerRotation);
    }
    public override void Tick()
    { 
    
    }
}
