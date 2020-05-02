using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

using System;

//dåligt namn
public static class BulletAudioController 
{
    static AudioClip[][] impacts;

    static GameObject shotPrefab;
    static GameObject impactPrefab;

    static GameObject player;

    public static void Initialize()
    {
        impacts = new AudioClip[Enum.GetNames(typeof(BulletImpactSound)).Length][];

        for (int i = 0; i < impacts.Length; i++)
            impacts[i] = Resources.LoadAll<AudioClip>("Audio/Bullet Impacts/" + (BulletImpactSound)i);

        shotPrefab = Resources.Load<GameObject>("Audio/Prefabs/shotSFX");
        impactPrefab = Resources.Load<GameObject>("Audio/Prefabs/impactSFX");

        player = Object.FindObjectOfType<PlayerActor>().gameObject;

        GlobalEvents.Subscribe(GlobalEvent.PlayImpactSFX, PlayImpactSFX);
        GlobalEvents.Subscribe(GlobalEvent.PlayShotSFX, PlayShotSFX);
    }
    static void PlayShotSFX(object[] args)
    {
        GameObject g = Object.Instantiate(shotPrefab, ((Transform)args[0]).position, Quaternion.identity, null);
        AudioSource source = g.GetComponent<AudioSource>();
        AudioClip clip = args[3] as AudioClip;

        source.pitch = Random.Range((float)args[1], (float)args[2]);
        source.PlayOneShot(clip);

        Object.Destroy(g, clip.length);
    }
    static void PlayImpactSFX(object[] args)
    {
        RaycastHit hit = (RaycastHit)args[0];

        //culla skit långt bort
        if (player.transform.position.DistanceTo(hit.point) > 25f)
            return;

        GameObject g = Object.Instantiate(impactPrefab, hit.point, Quaternion.identity, null);
        AudioSource source = g.GetComponent<AudioSource>();
        AudioClip clip = GetAudioClip(hit.collider.material);

        source.volume = 1f;
        source.spatialBlend = 1f;
        source.spread = 45f;
        source.pitch = Random.Range(.8f, 1.2f);
        source.PlayOneShot(clip);

        Object.Destroy(g, clip.length);
    }

    static AudioClip GetAudioClip(PhysicMaterial material)
    {
        //yucky yuck
        for (int i = 0; i < impacts.Length; i++)
            if (material.name.Contains(((BulletImpactSound)i).ToString()))
                return impacts[i].Random();

        return impacts[(int)BulletImpactSound.Generic].Random();
    }
}
public enum BulletImpactSound
{
    Generic,
    Glass,
    Metal
}
