using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PostProcessController : MonoBehaviour
{
    [SerializeField]VolumeProfile profile;

    [Header("Vignette")]
    Vignette vignette;
    [SerializeField]InterpolationMode vignetteMode = InterpolationMode.EaseOut;

    [SerializeField]Color vignetteStart = Color.black;
    [SerializeField]Color vignetteEnd = Color.red;

    [Header("Chromatic Abberation")]
    ChromaticAberration abberation;
    [SerializeField]InterpolationMode abberationMode = InterpolationMode.EaseOut;

    [Header("Panini Projection")]
    PaniniProjection panini;
    [SerializeField]InterpolationMode paniniMode = InterpolationMode.EaseOut;

    Vital health;

    void Start()
    {
        profile.TryGet(out vignette);
        profile.TryGet(out abberation);
        profile.TryGet(out panini);

        health = FindObjectOfType<PlayerActor>().Health;
        health.OnCurrentChanged += OnHealthChanged;
    }

    void OnHealthChanged(float changed)
    {
        vignette.intensity.value = Mathf.Lerp(.2f, 1f, (1f - health.CurrentInPercent).Interpolate(vignetteMode));
        vignette.color.value = Color.Lerp(vignetteStart, vignetteEnd, (1f - health.CurrentInPercent).Interpolate(vignetteMode));

        abberation.intensity.value = Mathf.Lerp(0f, 1f, (1f - health.CurrentInPercent).Interpolate(abberationMode));

        panini.distance.value = Mathf.Lerp(0f, 1f, (1f - health.CurrentInPercent).Interpolate(paniniMode));
    }
}
