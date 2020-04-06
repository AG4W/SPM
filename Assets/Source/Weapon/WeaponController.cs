using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField]float projectileSpeed;

    [SerializeField]GameObject shotPrefab;
    [SerializeField]AudioClip[] shotSounds;

    [SerializeField]Transform exitPoint;
    [SerializeField]AudioSource source;

    public void Shoot()
    {
        GameObject g = Instantiate(shotPrefab, exitPoint.position, exitPoint.transform.rotation, null);
        StartCoroutine(UpdateShotPosition(g));

        source.PlayOneShot(shotSounds.Random());
    }

    IEnumerator UpdateShotPosition(GameObject shot)
    {
        float t = 0f;

        while (t <= 10f)
        {
            t += Time.deltaTime;
            shot.transform.position += shot.transform.forward * projectileSpeed * Time.deltaTime;
            yield return null;
        }

        Destroy(shot);
    }
}
