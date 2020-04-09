using UnityEngine;

[System.Serializable]
public class SlowTime : Ability
{
    [SerializeField]AnimationCurve curve;

    public override void Tick()
    {
        base.Tick();
        Time.timeScale = curve.Evaluate(base.DurationTimer01);
    }

    public override void Deactivate()
    {
        Time.timeScale = 1f;
        base.Deactivate();
    }
}
