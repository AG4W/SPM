﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticFootsteps : MonoBehaviour {
    private AudioSource source;
    public AudioClip clip;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource> ();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayStaticFootstepSound()
    {
        source.PlayOneShot(clip);
    }
}
