namespace BehaviourTree.decorators
{
    public class Inverter : Node
    {
        public override Status Tick(Context context)
        {
            Status s = base.Children[0].Tick(context);

            if (s == Status.Success)
                return Status.Failed;
            else if (s == Status.Failed)
                return Status.Success;
            else
                return s;
        }
    }
}
