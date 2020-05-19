using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public void Activate()
    {
        GlobalEvents.Raise(GlobalEvent.SetPlayerCurrentCheckpoint, new CheckpointData(this));
    }
}
