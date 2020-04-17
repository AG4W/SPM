using UnityEngine;

public class ShotgunController : WeaponController
{
    [SerializeField]int shots = 4;
    [SerializeField]float spread = .25f;

    protected override void OnFireWeapon(Vector3 target, Vector3 heading)
    {
        for (int i = 0; i < shots; i++)
        {
            Vector3 h = (target + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread))) - base.ExitPoint.position;

            Physics.Raycast(base.ExitPoint.position, h.normalized, out RaycastHit hit, Mathf.Infinity, base.Mask);

            if (hit.transform != null)
            {
                Entity e = hit.transform.root.GetComponent<Entity>();

                //hit something else, create hit marker or something    
                if (e == null)
                {
                }
                else
                    e.Health.Update(-base.Damage);
            }

            Debug.DrawLine(base.ExitPoint.position, hit.transform != null ? hit.point : base.ExitPoint.position + h.normalized * 300f, Color.red);
            base.CreateVFX(heading, hit);
        }

        base.CreateSFX();
    }
}
