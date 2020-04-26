using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PostProcessController : MonoBehaviour
{
    VolumeProfile profile;
    CameraMode mode = CameraMode.Default;

    [Header("Depth of Field")]
    [SerializeField]float focusSpeed = 5f;
    [SerializeField]float defaultDoFStrength = .25f;
    [SerializeField]float ironSightDoFStrength = 7f;

    DepthOfField depthOfField;
    float targetDoFDistance;
    float actualDoFDistance;

    [Header("Vignette")]
    [SerializeField]InterpolationMode vignetteMode = InterpolationMode.EaseOut;

    [SerializeField]Color vignetteStart = Color.black;
    [SerializeField]Color vignetteEnd = Color.red;

    Vignette vignette;

    [Header("Chromatic Abberation")]
    [SerializeField]InterpolationMode abberationMode = InterpolationMode.EaseOut;

    ChromaticAberration abberation;

    [Header("Panini Projection")]
    [SerializeField]InterpolationMode paniniMode = InterpolationMode.EaseOut;

    PaniniProjection panini;

    void Awake()
    {
        profile = this.GetComponent<Volume>().profile;

        profile.TryGet(out depthOfField);
        profile.TryGet(out vignette);
        profile.TryGet(out abberation);
        profile.TryGet(out panini);

        GlobalEvents.Subscribe(GlobalEvent.SetCameraMode, SetMode);
        GlobalEvents.Subscribe(GlobalEvent.UpdateDOFFocusDistance, UpdateDepthOfField);

        GlobalEvents.Subscribe(GlobalEvent.PlayerHealthChanged, OnHealthChanged);
    }
    void Update()
    {
        Interpolate();
    }
    void Interpolate()
    {
        actualDoFDistance = Mathf.Lerp(actualDoFDistance, targetDoFDistance, focusSpeed * (Time.deltaTime / Time.timeScale));
    }

    void SetMode(object[] args) => mode = (CameraMode)args[0];
    void UpdateDepthOfField(object[] args)
    {
        targetDoFDistance = (float)args[0];

        depthOfField.farFocusStart.value = actualDoFDistance + 2f;
        depthOfField.farMaxBlur = mode == CameraMode.IronSight ? ironSightDoFStrength : defaultDoFStrength;

        depthOfField.nearFocusStart.value = mode == CameraMode.IronSight ? 1f : 0f;
        depthOfField.nearFocusEnd.value = mode == CameraMode.IronSight ? 1.5f : 0f;
    }

    void OnHealthChanged(object[] args)
    {
        Vital health = args[0] as Vital;

        vignette.intensity.value = Mathf.Lerp(.2f, 1f, (1f - health.CurrentInPercent).Interpolate(vignetteMode));
        vignette.color.value = Color.Lerp(vignetteStart, vignetteEnd, (1f - health.CurrentInPercent).Interpolate(vignetteMode));

        abberation.intensity.value = Mathf.Lerp(0f, 1f, (1f - health.CurrentInPercent).Interpolate(abberationMode));

        panini.distance.value = Mathf.Lerp(0f, 1f, (1f - health.CurrentInPercent).Interpolate(paniniMode));
    }
}
