using UnityEngine;

namespace BehaviourTree
{
    public class CanSeeTarget : Leaf
    {
        public override Status Execute(Context context)
        {
            return Status.Failed;
        }
    }
}