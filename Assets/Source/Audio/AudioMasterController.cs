using UnityEngine;
using UnityEngine.Audio;

public class AudioMasterController : MonoBehaviour
{
    [SerializeField]AudioMixerGroup master;

    [SerializeField]float minLowPassFrequency;
    [SerializeField]float maxLowPassFrequency;

    bool lowpassFilterOutsideAmbience = true;

    void Start()
    {
        GlobalEvents.Subscribe(GlobalEvent.PlayerHealthChanged, UpdateMasterLowPassFilter);

        master.audioMixer.SetFloat("Master Lowpass", maxLowPassFrequency);
        master.audioMixer.SetFloat("Outside Lowpass", lowpassFilterOutsideAmbience ? 2000f : 22000f);
    }

    void UpdateMasterLowPassFilter(object[] args)
    {
        float currentSqrd = ((Vital)args[0]).CurrentInPercent * ((Vital)args[0]).CurrentInPercent;

        master.audioMixer.SetFloat("Master Lowpass", Mathf.Lerp(minLowPassFrequency, maxLowPassFrequency, currentSqrd));
    }

    public void ToggleOutsideAmbienceFilter()
    {
        lowpassFilterOutsideAmbience = !lowpassFilterOutsideAmbience;
        master.audioMixer.SetFloat("Outside Lowpass", lowpassFilterOutsideAmbience ? 2000f : 22000f);
    }
}
