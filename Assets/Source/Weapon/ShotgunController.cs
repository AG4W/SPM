using UnityEngine;

public class ShotgunController : WeaponController
{
    [SerializeField]int shots = 4;

    protected override void OnFireWeapon(Vector3 target, Vector3 heading)
    {
        for (int i = 0; i < shots; i++)
        {
            Vector3 h = heading + new Vector3(Random.Range(-BaseSpread, BaseSpread), Random.Range(-BaseSpread, BaseSpread), Random.Range(-BaseSpread, BaseSpread));
            Physics.Raycast(base.ExitPoint.position, h, out RaycastHit hit, Mathf.Infinity, base.Mask);

            if (hit.transform != null)
            {
                Entity e = hit.transform.root.GetComponent<Entity>();

                //hit something else, create hit marker or something    
                if (e == null)
                {
                    if (hit.transform.GetComponent<Rigidbody>())
                    {
                        //do some dropoff so we dont shoot stuff around on the entire map
                        hit.transform.GetComponent<Rigidbody>().AddForce(h.normalized * (base.StoppingPower / hit.point.DistanceTo(base.ExitPoint.position)), ForceMode.Impulse);
                    }
                }
                else
                    e.Health.Update(-base.Damage);
            }

            Debug.DrawLine(base.ExitPoint.position, hit.transform != null ? hit.point : base.ExitPoint.position + h.normalized * 50f, hit.transform != null ? Color.red : Color.yellow, .5f);
            base.CreateVFX(h, hit);
        }

        base.CreateSFX();
    }
}
