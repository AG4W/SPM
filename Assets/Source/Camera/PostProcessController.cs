﻿using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PostProcessController : MonoBehaviour
{
    VolumeProfile profile;

    [Header("Depth of Field")]
    [SerializeField]float focusSpeed = 5f;

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

    void Start()
    {
        profile = this.GetComponent<Volume>().profile;
        profile.TryGet(out depthOfField);
        profile.TryGet(out vignette);
        profile.TryGet(out abberation);

        GlobalEvents.Subscribe(GlobalEvent.UpdateDOFFocusDistance, UpdateDepthOfField);

        GlobalEvents.Subscribe(GlobalEvent.PlayerHealthChanged, OnHealthChanged);
        GlobalEvents.Subscribe(GlobalEvent.PlayerForceChanged, OnForceChanged);
    }
    void Update()
    {
        Interpolate();
    }
    void Interpolate()
    {
        actualDoFDistance = Mathf.Lerp(actualDoFDistance, targetDoFDistance, focusSpeed * (Time.deltaTime / Time.timeScale));
    }

    void UpdateDepthOfField(object[] args)
    {
        targetDoFDistance = (float)args[0];

        depthOfField.focusDistance.value = actualDoFDistance;
        //depthOfField.farFocusStart.value = actualDoFDistance + 2f;
        //depthOfField.farMaxBlur = mode == CameraMode.IronSight ? ironSightDoFStrength : defaultDoFStrength;

        //depthOfField.nearFocusStart.value = mode == CameraMode.IronSight ? 1f : 0f;
        //depthOfField.nearFocusEnd.value = mode == CameraMode.IronSight ? 1.5f : 0f;
    }

    void OnHealthChanged(object[] args)
    {
        Vital health = args[0] as Vital;

        vignette.intensity.value = Mathf.Lerp(.2f, .6f, (1f - health.CurrentInPercent).Interpolate(vignetteMode));
        vignette.color.value = Color.Lerp(vignetteStart, vignetteEnd, (1f - health.CurrentInPercent).Interpolate(vignetteMode));
    }
    void OnForceChanged(object[] args)
    {
        Vital force = args[0] as Vital;

        abberation.intensity.value = Mathf.Lerp(0f, 1f, (1f - force.CurrentInPercent) * (1f - force.CurrentInPercent) * (1f - force.CurrentInPercent));
    }
}
