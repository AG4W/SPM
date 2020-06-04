using UnityEngine;

using System.Collections;

public class SimpleAnimatedEntity : SwitchEntity, IInteractable
{
    [Header("Animated Entity Settings")]
    [SerializeField]string animatingText = "[ Wait ]";
    [SerializeField]Color animatingColor = Color.yellow;

    [SerializeField]float interpolationTime = 1f;

    [SerializeField]Transform start;
    [SerializeField]Transform end;

    [SerializeField]InterpolationMode mode = InterpolationMode.Linear;

    bool cachedState;
    bool isAnimating = false;

    protected override void Initalize()
    {
        base.Initalize();

        for (int i = 0; i < base.ConnectedEntities.Length; i++)
        {
            base.ConnectedEntities[i].transform.position = start.position;
            base.ConnectedEntities[i].transform.rotation = start.rotation;
        }
    }
    public override void OnInteractStart()
    {
        if (isAnimating)
            return;
        
        cachedState = base.InOffState;
        base.OnInteractStart();

        this.StopAllCoroutines();
        this.StartCoroutine(Interpolate());
    }
    public override void OnInteractAnimate()
    {
        base.OnInteractAnimate();
        base.OverrideWorldUI(animatingText, animatingColor);
    }
    public override void OnInteractComplete()
    {
        base.OnInteractComplete();
        base.UpdateWorldUI();
    }

    protected override void OnLinkedAnimate(ConnectedEntity other)
    {
        base.OnLinkedAnimate(other);
        base.OverrideWorldUI(animatingText, animatingColor);
    }

    IEnumerator Interpolate()
    {
        OnInteractAnimate();

        float t = 0f;
        isAnimating = true;

        while (t <= interpolationTime)
        {
            t += Time.deltaTime;

            for (int i = 0; i < base.ConnectedEntities.Length; i++)
            {
                if (cachedState)
                {
                    base.ConnectedEntities[i].transform.position = Vector3.Lerp(end.position, start.position, t.Interpolate(mode));
                    base.ConnectedEntities[i].transform.rotation = Quaternion.Slerp(end.rotation, start.rotation, t.Interpolate(mode));
                }
                else
                {
                    base.ConnectedEntities[i].transform.position = Vector3.Lerp(start.position, end.position, t.Interpolate(mode));
                    base.ConnectedEntities[i].transform.rotation = Quaternion.Slerp(start.rotation, end.rotation, t.Interpolate(mode));
                }

                yield return null;
            }
        }

        isAnimating = false;
        OnInteractComplete();
    }
}

