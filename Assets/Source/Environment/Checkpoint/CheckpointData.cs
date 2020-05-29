using UnityEngine;

public class CheckpointData
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;

    public CheckpointData(Checkpoint checkpoint)
    {
        this.Position = checkpoint.transform.position;
        this.Rotation = checkpoint.transform.rotation;
    }

    public CheckpointData(Vector3 position, Quaternion rotation)
    {
        this.Position = position;
        this.Rotation = rotation;
    }
}
