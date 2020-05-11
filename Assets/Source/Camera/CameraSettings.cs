using UnityEngine;

[System.Serializable]
public struct CameraSettings
{
    [SerializeField]float fov;
    [SerializeField]float farClipDistance;
    
    [SerializeField]Vector3 position;
    [SerializeField]Vector3 rotation;

    public float FoV => fov;
    public float FarClipDistance => farClipDistance;

    public Vector3 Position => position;
    public Quaternion Rotation => rotation == Vector3.zero? Quaternion.identity : Quaternion.LookRotation(rotation, Vector3.up);

    public CameraSettings(float fov, float farClipDistance, Vector3 position, Vector3 rotation)
    {
        this.fov = fov;
        this.farClipDistance = farClipDistance;

        this.position = position;
        this.rotation = rotation;
    }
}
