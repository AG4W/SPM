using System;
using System.Linq;

namespace BehaviourTree.composites
{
    public class RandomSelector : Node
    {
        public override Status Tick(Context context)
        {
            Status s;

            foreach(int i in Enumerable.Range(0, base.Children.Length).OrderBy(x => ((Random)context.data["random"]).Next()))
            {
                s = base.Children[i].Tick(context);

                if (s != Status.Failed)
                    return s;
            }

            return Status.Failed;
        }
    }
}
