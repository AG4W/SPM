namespace BehaviourTree.decorators
{
    public class Inverter : Decorator
    {
        public override Status Tick(Context context)
        {
            Status s = base.child.Tick(context);

            if (s == Status.Success)
                return Status.Failed;
            else if (s == Status.Failed)
                return Status.Success;
            else
                return s;
        }
    }
}
