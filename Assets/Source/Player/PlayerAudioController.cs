using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    AudioSource source;

    void Start()
    {
        source = this.GetComponent<AudioSource>();

        GlobalEvents.Subscribe(GlobalEvent.OnAbilityActivated, OnAbilityActivated);
    }

    void OnAbilityActivated(object[] args)
    {
        source.pitch = Random.Range(.9f, 1.1f);
        source.PlayOneShot(((AbilityState)args[0]).ActivateSounds.Random());
    }
}
