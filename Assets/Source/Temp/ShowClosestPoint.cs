using UnityEngine;

public class ShowClosestPoint : MonoBehaviour
{
    public Vector3 location;
    //public SphereCollider collider;

    public void Awake()
    {
        //collider = GetComponent<SphereCollider>();
    }

    public void OnDrawGizmos()
    {
        //var collider = GetComponent<SphereCollider>();

        //if (!collider)
        //{
        //    Debug.Log("No collider");
        //    return; // nothing to do without a collider
        //}
        Gizmos.DrawSphere(transform.position, 0.5f);

        float radius = 0.5f;
        bool overlapCheck = Physics.CheckSphere(transform.position, radius);
        
        int counter = 0;
        while (overlapCheck == true)
        {
            Gizmos.DrawSphere(transform.position, 0.5f);
            Debug.Log("Overlap på Sphere collider");
            Collider[] overlapColliders = Physics.OverlapSphere(transform.position, radius);
            if (overlapColliders.Length > 0)
                for (int i = 0; i < overlapColliders.Length; i++)
                {
                    Vector3 pointInColliderClosestToSphereCenter = overlapColliders[i].ClosestPoint(transform.position);
                    Gizmos.DrawWireSphere(pointInColliderClosestToSphereCenter, 0.1f);
                    float hitDist = Vector3.Distance(transform.position, pointInColliderClosestToSphereCenter);
                    Vector3 hitDirection = pointInColliderClosestToSphereCenter - transform.position;
                    // Vi vill flytta oss: radien minus dist
                    this.transform.position += -hitDirection.normalized * (radius - hitDist);
                    Debug.Log("Flyttar Sphere: " + (-hitDirection.normalized * (radius - hitDist)));
                }
            overlapCheck = Physics.CheckSphere(transform.position, radius);

            if (counter >= 10)
            {
                break;
            }
            counter++;
        }

        //Vector3 closestPoint = collider.ClosestPoint(location);

        //Gizmos.DrawSphere(location, 0.1f);
        //Gizmos.DrawWireSphere(closestPoint, 0.1f);
    }
}
