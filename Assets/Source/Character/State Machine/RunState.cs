using UnityEngine;

[CreateAssetMenu(menuName = "PlayerState/RunState")]
public class RunState : BaseState
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
