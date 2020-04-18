using UnityEngine;

public class ShotgunController : WeaponController
{
    [SerializeField]int shots = 4;
    [SerializeField]float spread = .25f;

    protected override void OnFireWeapon(Vector3 target, Vector3 heading)
    {
        for (int i = 0; i < shots; i++)
        {
            Vector3 h = heading.normalized + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread));
                //(target + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread))) - base.ExitPoint.position;

            Physics.Raycast(base.ExitPoint.position, h.normalized, out RaycastHit hit, Mathf.Infinity, base.Mask);

            if (hit.transform != null)
            {
                Entity e = hit.transform.root.GetComponent<Entity>();

                //hit something else, create hit marker or something    
                if (e == null)
                {
                    if (hit.transform.GetComponent<Rigidbody>())
                    {
                        //do some dropoff so we dont shoot stuff around on the entire map
                        Vector3 force = heading.normalized * base.StoppingPower;
                        force /= hit.point.DistanceTo(base.ExitPoint.position);
                        hit.transform.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
                    }
                }
                else
                    e.Health.Update(-base.Damage);
            }

            Debug.DrawRay(base.ExitPoint.position, h, Color.red, .5f);
            //Debug.DrawLine(base.ExitPoint.position, hit.transform != null ? hit.point : base.ExitPoint.position + h.normalized * 50f, Color.red);
            base.CreateVFX(h, hit);
        }

        base.CreateSFX();
    }
}
