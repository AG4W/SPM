using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

using System;
using UnityEngine.Audio;

public static class BulletAudioController 
{
    static AudioClip[][] impacts;
    static GameObject player;

    public static void Initialize()
    {
        impacts = new AudioClip[Enum.GetNames(typeof(BulletImpactSound)).Length][];

        for (int i = 0; i < impacts.Length; i++)
            impacts[i] = Resources.LoadAll<AudioClip>("Audio/Bullet Impacts/" + (BulletImpactSound)i);

        player = Object.FindObjectOfType<PlayerActor>().gameObject;

        GlobalEvents.Subscribe(GlobalEvent.PlayBulletImpactSFX, PlayImpact);
    }

    static void PlayImpact(object[] args)
    {
        RaycastHit hit = (RaycastHit)args[0];

        //culla skit långt bort
        if (player.transform.position.DistanceTo(hit.point) > 25f)
            return;

        GameObject g = new GameObject("audio sfx", typeof(AudioSource));
        AudioSource source = g.GetComponent<AudioSource>();

        source.volume = 2f;
        source.spatialBlend = 1f;
        //source.spread = 45f;
        source.pitch = Random.Range(.8f, 1.2f);
        source.PlayOneShot(GetAudioClip(hit.collider.material));

        Object.Destroy(g, 2f);
    }

    static AudioClip GetAudioClip(PhysicMaterial material)
    {
        //lös detta så att vi inte använder stringchecks
        Debug.Log(material.name);
        switch (material.name)
        {
            case "Metal (Instance)":
                return impacts[(int)BulletImpactSound.Metal].Random();
            case "Glass":
                return impacts[(int)BulletImpactSound.Glass].Random();
            default:
                return impacts[(int)BulletImpactSound.Generic].Random();
        }
    }
}
public enum BulletImpactSound
{
    Generic,
    Glass,
    Metal
}
