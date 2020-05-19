using UnityEngine;

public struct CheckpointData
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;

    public CheckpointData(Checkpoint checkpoint)
    {
        this.Position = checkpoint.transform.position;
        this.Rotation = checkpoint.transform.rotation;
    }
}
