namespace BehaviourTree.composite
{
    public class Selector : Composite
    {
        public override Status Tick(Context context)
        {
            Status s;

            for (int i = 0; i < base.children.Length; i++)
            {
                s = base.children[i].Tick(context);

                if (s != Status.Failed)
                    return s;
            }

            return Status.Failed;
        }
    }
}
