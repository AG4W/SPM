using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEntity : MonoBehaviour
{ 
    float timer;
    [SerializeField]float projectileLifetime = 2f;
    [SerializeField]float projectileSpeed = 50f;

    Vector3 lastFramePos;

    private void Update()
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
