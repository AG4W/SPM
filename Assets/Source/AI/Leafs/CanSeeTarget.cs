using UnityEngine;

namespace BehaviourTree
{
    public class CanSeeTarget : Node
    {
        public override Status Tick(Context context)
        {
            return Status.Failed;
        }
    }
}