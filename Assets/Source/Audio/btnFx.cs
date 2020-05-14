using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class btnFx : MonoBehaviour
{
    private AudioSource source;
    public AudioClip hoverFx;
    public AudioClip clickFx;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void HoverSound()
    {
        source.PlayOneShot(hoverFx);
    }
    public void ClickSound()
    {
        source.PlayOneShot(clickFx);
    }
}
