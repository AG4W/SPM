using UnityEngine;
using UnityEngine.Audio;

public class AudioMasterController : MonoBehaviour
{
    [SerializeField]AudioMixerGroup master;

    [SerializeField]float minLowPassFrequency;
    [SerializeField]float maxLowPassFrequency;

    void Awake()
    {
        GlobalEvents.Subscribe(GlobalEvent.PlayerHealthChanged, UpdateMasterLowPassFilter);
    }

    void UpdateMasterLowPassFilter(object[] args)
    {
        float currentSqrd = ((Vital)args[0]).CurrentInPercent * ((Vital)args[0]).CurrentInPercent;

        master.audioMixer.SetFloat("Master Lowpass", Mathf.Lerp(minLowPassFrequency, maxLowPassFrequency, currentSqrd));
    }
}
