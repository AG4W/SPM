namespace BehaviourTree.composite
{
    public class Selector : Node
    {
        public Selector(params Node[] children) : base(children)
        {

        }

        public override Status Tick(Context context)
        {
            Status s;

            for (int i = 0; i < base.Children.Length; i++)
            {
                s = base.Children[i].Tick(context);

                if (s != Status.Failed)
                    return s;
            }

            return Status.Failed;
        }
    }
}
