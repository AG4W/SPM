using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField]float projectileSpeed;
    [SerializeField]float fireCooldown;

    [SerializeField]GameObject shotPrefab;
    [SerializeField]AudioClip[] shotSounds;

    [SerializeField]Transform exitPoint;
    [SerializeField]AudioSource source;

    bool canFire = true;

    public void Shoot()
    {
        if (!canFire)
            return;

        StartCoroutine(ResetCooldown());

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 heading;

        Physics.Raycast(ray, out hit, Mathf.Infinity);

        heading = (hit.transform == null ? ray.direction * 100 : hit.point) - exitPoint.position;

        Debug.DrawRay(exitPoint.position, heading.normalized * 100f, Color.red, 5f);

        GameObject g = Instantiate(shotPrefab, exitPoint.position, Quaternion.LookRotation(heading.normalized), null);
        StartCoroutine(UpdateShotPosition(g));

        source.PlayOneShot(shotSounds.Random());
    }

    IEnumerator UpdateShotPosition(GameObject shot)
    {
        float t = 0f;

        while (t <= 3f)
        {
            t += Time.deltaTime;
            shot.transform.position += shot.transform.forward * projectileSpeed * Time.deltaTime;
            yield return null;
        }

        Destroy(shot);
    }
    IEnumerator ResetCooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }
}
