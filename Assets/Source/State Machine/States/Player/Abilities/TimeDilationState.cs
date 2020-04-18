using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Force/Time Dilation")]
public class TimeDilationState : AbilityState
{
    [Range(.01f, 1f)][SerializeField]float timeScaling = .2f;

    public override void Enter()
    {
        base.Enter();

        Time.timeScale = timeScaling;
    }
    public override void Exit()
    {
        base.Exit();

        Time.timeScale = 1f;
    }
}
