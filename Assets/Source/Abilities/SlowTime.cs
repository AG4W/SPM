using UnityEngine;

[System.Serializable]
public class SlowTime : Ability
{
    [SerializeField]AnimationCurve curve;

    public override void Activate(Context context)
    {
        base.Activate(context);
    }

    public override void Tick()
    {
        base.Tick();

        if(base.IsActive)
            Time.timeScale = curve.Evaluate(base.DurationTimer01);
    }
}
