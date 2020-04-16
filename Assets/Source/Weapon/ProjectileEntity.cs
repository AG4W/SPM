using UnityEngine;

public class ProjectileEntity : MonoBehaviour
{ 
    float timer;
    [SerializeField]float projectileLifetime = 2f;
    [SerializeField]float projectileSpeed = 50f;

    Vector3 lastFramePos;

    public void Initialize(RaycastHit hit)
    {
        if (hit.transform != null)
            projectileLifetime = Vector3.Distance(this.transform.position, hit.point) / projectileSpeed;
    }

    void Update()
    {
        UpdatePosition();
    }
    void UpdatePosition()
    {
        timer += Time.deltaTime;

        if (timer >= projectileLifetime)
            Object.Destroy(this.gameObject);

        this.transform.position += this.transform.forward * projectileSpeed * Time.deltaTime;
        lastFramePos = this.transform.position;
    }
}
